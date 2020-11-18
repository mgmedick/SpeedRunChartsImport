using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Serilog;
using SpeedRunCommon;

namespace SpeedRunAppImport.Service
{
    public class SpeedRunService : BaseService, ISpeedRunService
    {
        private readonly ISettingService _settingService = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public SpeedRunService(ISettingService settingService, ISpeedRunRepository speedRunRepo, IConfiguration config, ILogger logger)
        {
            _settingService = settingService;
            _speedRunRepo = speedRunRepo;
            _config = config;
            _logger = logger;
        }

        public void ProcessSpeedRuns(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                _logger.Information("Started ProcessSpeedRuns: {@LastImportDate}, {@IsFullImport}", lastImportDate, isFullImport);
                var newImportDate = DateTime.UtcNow;
                RunsOrdering orderBy = isFullImport ? RunsOrdering.DateSubmitted : RunsOrdering.DateSubmittedDescending;
                var results = new List<SpeedRun>();
                var runs = new List<SpeedRun>();

                do
                {
                    runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count, orderBy);
                    results.AddRange(runs);
                    _logger.Information("Pulled runs: {@New}, total runs: {@Total}", runs.Count, results.Count);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, isFullImport);
                        results.ClearMemory();
                    }
                }
                while (runs.Count == MaxElementsPerPage && runs.Min(i => i.DateSubmitted ?? SqlMinDateTime) >= lastImportDate);

                if (results.Any())
                {
                    if (!isFullImport)
                    {
                        results.RemoveAll(i => (i.DateSubmitted ?? SqlMinDateTime) < lastImportDate);
                    }

                    SaveSpeedRuns(results, isFullImport);
                    results.ClearMemory();
                }

                if (!isFullImport)
                {
                    var verifiedResults = new List<SpeedRun>();
                    var verifiedRuns = new List<SpeedRun>();

                    do
                    {
                        verifiedRuns = GetSpeedRunsWithRetry(MaxElementsPerPage, verifiedResults.Count, RunsOrdering.VerifyDateDescending, RunStatusType.Verified);
                        verifiedResults.AddRange(verifiedRuns);
                        _logger.Information("Pulled verified runs: {@New}, total runs: {@Total}", verifiedRuns.Count, verifiedResults.Count);
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                        var memorySize = GC.GetTotalMemory(false);
                        if (memorySize > MaxMemorySizeBytes)
                        {
                            _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", verifiedResults.Count, memorySize);
                            SaveSpeedRuns(verifiedResults, isFullImport);
                            verifiedResults.ClearMemory();
                        }
                    }
                    while (verifiedRuns.Count == MaxElementsPerPage && verifiedRuns.Min(i => i.VerifyDate ?? SqlMinDateTime) >= lastImportDate);

                    if (verifiedResults.Any())
                    {
                        verifiedResults.RemoveAll(i => (i.VerifyDate ?? SqlMinDateTime) < lastImportDate);
                        SaveSpeedRuns(verifiedResults, isFullImport);
                        results.ClearMemory();
                    }

                    var rejectedDate = lastImportDate.AddDays(RejectedDaysBack);
                    var rejectedResults = new List<SpeedRun>();
                    var rejectedRuns = new List<SpeedRun>();

                    do
                    {
                        rejectedRuns = GetSpeedRunsWithRetry(MaxElementsPerPage, rejectedResults.Count, RunsOrdering.DateSubmittedDescending, RunStatusType.Rejected);
                        rejectedResults.AddRange(rejectedRuns);
                        _logger.Information("Pulled rejected runs: {@New}, total runs: {@Total}", rejectedRuns.Count, rejectedResults.Count);
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                        var memorySize = GC.GetTotalMemory(false);
                        if (memorySize > MaxMemorySizeBytes)
                        {
                            _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", rejectedResults.Count, memorySize);
                            SaveSpeedRuns(rejectedResults, isFullImport);
                            rejectedResults.ClearMemory();
                        }
                    }
                    while (rejectedRuns.Count == MaxElementsPerPage && rejectedRuns.Min(i => i.DateSubmitted ?? SqlMinDateTime) >= rejectedDate);

                    if (rejectedResults.Any())
                    {
                        rejectedResults.RemoveAll(i => (i.DateSubmitted ?? SqlMinDateTime) < rejectedDate);
                        SaveSpeedRuns(rejectedResults, isFullImport);
                        results.ClearMemory();
                    }
                }
                
                _settingService.UpdateSetting("SpeedRunLastImportDate", newImportDate);
                _logger.Information("Completed ProcessSpeedRuns");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessSpeedRuns");
            }
        }

        public List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<SpeedRun> runs = null;

            try
            {
                runs = clientContainer.Runs.GetRuns(status: statusType, elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull speedRuns: {@New}, total speedRuns: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    runs = GetSpeedRunsWithRetry(elementsPerPage, elementsOffset, orderBy, statusType, retryCount);
                }
                else
                {
                    throw ex;
                }
            }

            return runs;
        }

        public SpeedRun GetSpeedRunWithRetry(string speedRunID, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            SpeedRun run = null;

            try
            {
                run = clientContainer.Runs.GetRun(runId: speedRunID);
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull speedRun: {@SpeedRunID}, retry: {@RetryCount}", speedRunID, retryCount);
                    run = GetSpeedRunWithRetry(speedRunID, retryCount);
                }
                else
                {
                    throw ex;
                }
            }

            return run;
        }

        public void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isFullImport)
        {
            var runEntities = runs.Select(i => i.ConvertToEntity()).ToList();
            var variableValueEntities = runs.SelectMany(i => i.VariableValueMappings.Select(g => new SpeedRunVariableValueEntity() { SpeedRunID = i.ID, VariableID = g.VariableID, VariableValueID = g.VariableValueID })).ToList();
            var playerEntities = runs.SelectMany(i => i.Players.Select(g => new SpeedRunPlayerEntity() { SpeedRunID = i.ID, IsUser = g.IsUser, UserID = g.UserID, GuestName = g.GuestName })).ToList();
            var videoEntities = runs.Where(i => i.Videos?.Links != null && i.Videos.Links.Any(g => g != null))
                                    .SelectMany(i => i.Videos?.Links?.Select((g, n) => new SpeedRunVideoEntity() { SpeedRunID = i.ID, VideoLinkUrl = g?.ToString(), VideoLinkEmbededUrl = i.Videos?.EmbededLinks?.ToArray()[n]?.ToString() }))
                                    .Where(i => !string.IsNullOrWhiteSpace(i.VideoLinkUrl))
                                    .ToList();

            SaveSpeedRuns(runEntities, variableValueEntities, playerEntities, videoEntities, isFullImport);
        }

        public void SaveSpeedRuns(IEnumerable<SpeedRunEntity> runEntities, IEnumerable<SpeedRunVariableValueEntity> variableValueEntities, IEnumerable<SpeedRunPlayerEntity> playerEntities, IEnumerable<SpeedRunVideoEntity> videoEntities, bool isFullImport)
        {
            if (isFullImport)
            {
                _speedRunRepo.CopySpeedRunTables();
                _speedRunRepo.InsertSpeedRuns(runEntities, variableValueEntities, playerEntities, videoEntities);
                _speedRunRepo.RenameAndDropSpeedRunTables();
            }
            else
            {
                _speedRunRepo.SaveSpeedRuns(runEntities, variableValueEntities, playerEntities, videoEntities);
            }
        }
    }
}


