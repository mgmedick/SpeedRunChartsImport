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
                var results = GetSpeedRunsWithRetry(MaxElementsPerPage, 0, orderBy);
                _logger.Information("Pulled runs: {@New}, total runs: {@Total}", results.Count, results.Count);

                while (results.Count == MaxElementsPerPage && results.Min(i => i.DateSubmitted ?? SqlMinDateTime) >= lastImportDate)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
                    var runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count, orderBy);
                    results.AddRange(runs);
                    _logger.Information("Pulled runs: {@New}, total runs: {@Total}", runs.Count, results.Count);

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, isFullImport);
                        results.ClearMemory();
                    }
                }

                if (results.Any())
                {
                    SaveSpeedRuns(results, isFullImport);
                }

                if (!isFullImport)
                {
                    var verifiedResults = GetSpeedRunsWithRetry(MaxElementsPerPage, 0, RunsOrdering.VerifyDateDescending, RunStatusType.Verified);
                    _logger.Information("Pulled verified runs: {@New}, total runs: {@Total}", verifiedResults.Count, verifiedResults.Count);

                    while (verifiedResults.Count == MaxElementsPerPage && verifiedResults.Min(i => i.VerifyDate ?? SqlMinDateTime) >= lastImportDate)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
                        var runs = GetSpeedRunsWithRetry(MaxElementsPerPage, verifiedResults.Count, RunsOrdering.VerifyDateDescending, RunStatusType.Verified);
                        verifiedResults.AddRange(runs);
                        _logger.Information("Pulled verified runs: {@New}, total runs: {@Total}", runs.Count, verifiedResults.Count);

                        var memorySize = GC.GetTotalMemory(false);
                        if (memorySize > MaxMemorySizeBytes)
                        {
                            _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                            SaveSpeedRuns(verifiedResults, isFullImport);
                            verifiedResults.ClearMemory();
                        }
                    }

                    if (verifiedResults.Any())
                    {
                        SaveSpeedRuns(verifiedResults, isFullImport);
                    }

                    var rejectedDate = lastImportDate.AddDays(RejectedDaysBack);
                    var rejectedResults = GetSpeedRunsWithRetry(MaxElementsPerPage, 0, RunsOrdering.DateSubmittedDescending, RunStatusType.Rejected);
                    _logger.Information("Pulled rejected runs: {@New}, total runs: {@Total}", rejectedResults.Count, rejectedResults.Count);

                    while (rejectedResults.Count == MaxElementsPerPage && rejectedResults.Min(i => i.DateSubmitted ?? SqlMinDateTime) >= rejectedDate)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
                        var runs = GetSpeedRunsWithRetry(MaxElementsPerPage, rejectedResults.Count, RunsOrdering.DateSubmittedDescending, RunStatusType.Rejected);
                        rejectedResults.AddRange(runs);
                        _logger.Information("Pulled rejected runs: {@New}, total runs: {@Total}", runs.Count, rejectedResults.Count);

                        var memorySize = GC.GetTotalMemory(false);
                        if (memorySize > MaxMemorySizeBytes)
                        {
                            _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                            SaveSpeedRuns(rejectedResults, isFullImport);
                            rejectedResults.ClearMemory();
                        }
                    }

                    if (rejectedResults.Any())
                    {
                        SaveSpeedRuns(rejectedResults, isFullImport);
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


