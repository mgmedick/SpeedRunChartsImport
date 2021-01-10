using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using System.Threading;
using Serilog;
using SpeedRunCommon;

namespace SpeedRunAppImport.Service
{
    public class SpeedRunService : BaseService, ISpeedRunService
    {
        private readonly ISettingService _settingService = null;
        private readonly ICacheService _cacheService = null;
        private readonly IScrapeService _scrapeService = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly ILogger _logger;

        public SpeedRunService(ISettingService settingService, ICacheService cacheService, IScrapeService scrapeService, ISpeedRunRepository speedRunRepo, ILogger logger)
        {
            _settingService = settingService;
            _cacheService = cacheService;
            _scrapeService = scrapeService;
            _speedRunRepo = speedRunRepo;
            _logger = logger;
        }

        public void ProcessSpeedRuns(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                var lastImportDateUtc = lastImportDate.ToUniversalTime();
                _logger.Information("Started ProcessSpeedRuns: {@LastImportDate}, {@LastImportDateUtc}, {@IsFullImport}", lastImportDate, lastImportDateUtc, isFullImport);

                if (isFullImport)
                {
                    ProcessSpeedRunsFullImport();
                }
                else
                {
                    ProcessLatestSpeedRuns(lastImportDateUtc);
                }

                _settingService.UpdateSetting("SpeedRunLastImportDate", DateTime.Now);
                _logger.Information("Completed ProcessSpeedRuns");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessSpeedRuns");
            }
        }

        public void ProcessSpeedRunsFullImport()
        {
            RunsOrdering orderBy = RunsOrdering.DateSubmitted;
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var games = _cacheService.GetGames();
            var prevTotal = 0;

            _speedRunRepo.CopySpeedRunTables();

            foreach (var game in games)
            {
                var prevGameTotal = 0;
                do
                {
                    runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count(i => i.GameID == game.ID) + prevGameTotal, game.ID, orderBy);
                    if(runs != null)
                    {
                        results.AddRange(runs);
                        _logger.Information("GameID: {@GameID}, pulled runs: {@New}, game total: {@GameTotal}, total runs: {@Total}", game.ID, runs.Count, results.Count(i => i.GameID == game.ID) + prevGameTotal, results.Count + prevTotal);
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                        var memorySize = GC.GetTotalMemory(false);
                        if (memorySize > MaxMemorySizeBytes)
                        {
                            prevTotal += results.Count;
                            prevGameTotal += results.Count(i => i.GameID == game.ID);
                            _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                            SaveSpeedRuns(results, true);
                            results.ClearMemory();
                        }
                    }
                }
                while (runs.Count == MaxElementsPerPage);
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, true);
                results.ClearMemory();
            }

            _speedRunRepo.RenameAndDropSpeedRunTables();
        }

        public void ProcessLatestSpeedRuns(DateTime lastImportDateUtc)
        {
            var results = new List<SpeedRun>();
            var latestRunIDs = _scrapeService.GetLatestSpeedRunIDs();

            foreach (var runID in latestRunIDs)
            {
                var run = GetSpeedRunWithRetry(runID);
                if(run != null)
                {
                    results.Add(run);
                    _logger.Information("Pulled runs: {@New}, total runs: {@Total}", results.Count, latestRunIDs.Count());
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, false);
                        results.ClearMemory();
                    }
                }
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, false);
                results.ClearMemory();
            }

            ProcessSpeedRunUpdates(lastImportDateUtc);
        }

        public void ProcessSpeedRunUpdates(DateTime lastImportDateUtc)
        {
            _logger.Information("Started ProcessSpeedRunUpdates: {@LastImportDateUtc}", lastImportDateUtc);
            var lastUpdateDateUtc = lastImportDateUtc.AddDays(UpdateDaysBack);
            var updateRunIDs = _speedRunRepo.GetSpeedRuns(i => i.StatusTypeID == (int)RunStatusType.New && i.DateSubmitted >= lastUpdateDateUtc).Select(i => i.ID).ToList();
            var updatedResults = new List<SpeedRun>();

            foreach (var runID in updateRunIDs)
            {
                var run = GetSpeedRunWithRetry(runID);
                if (run != null)
                {
                    updatedResults.Add(run);
                    _logger.Information("Pulled runs: {@New}, total runs: {@Total}", updatedResults.Count, updateRunIDs.Count());
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", updatedResults.Count, memorySize);
                        SaveSpeedRuns(updatedResults, false);
                        updatedResults.ClearMemory();
                    }
                }
            }

            if (updatedResults.Any())
            {
                SaveSpeedRuns(updatedResults, false);
                updatedResults.ClearMemory();
            }
            _logger.Information("Completed ProcessSpeedRunUpdates");
        }

        /*
        public void ProcessSpeedRuns(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                _logger.Information("Started ProcessSpeedRuns: {@LastImportDate}, {@IsFullImport}", lastImportDate, isFullImport);
                var newImportDate = DateTime.UtcNow;              
                RunsOrdering orderBy = isFullImport ? RunsOrdering.DateSubmitted : RunsOrdering.DateSubmittedDescending;
                var results = new List<SpeedRun>();
                var runs = new List<SpeedRun>();
                var prevTotal = 0;

                if (isFullImport)
                {
                    _speedRunRepo.CopySpeedRunTables();
                }

                do
                {
                    runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count + prevTotal, orderBy);
                    results.AddRange(runs);
                    _logger.Information("Pulled runs: {@New}, total runs: {@Total}", runs.Count, results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
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

                if (isFullImport)
                {
                    _speedRunRepo.RenameAndDropSpeedRunTables();
                }
                else
                {
                    ProcessSpeedRunUpdates(lastImportDate, isFullImport);
                }

                _settingService.UpdateSetting("SpeedRunLastImportDate", newImportDate);
                _logger.Information("Completed ProcessSpeedRuns");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessSpeedRuns");
            }
        }

        public void ProcessSpeedRunUpdates(DateTime lastImportDate)
        {
            var verifiedResults = new List<SpeedRun>();
            var verifiedRuns = new List<SpeedRun>();
            var verifiedPrevTotal = 0;

            do
            {
                verifiedRuns = GetSpeedRunsWithRetry(MaxElementsPerPage, verifiedResults.Count + verifiedPrevTotal, RunsOrdering.VerifyDateDescending, RunStatusType.Verified);
                verifiedResults.AddRange(verifiedRuns);
                _logger.Information("Pulled verified runs: {@New}, total runs: {@Total}", verifiedRuns.Count, verifiedResults.Count + verifiedPrevTotal);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                var memorySize = GC.GetTotalMemory(false);
                if (memorySize > MaxMemorySizeBytes)
                {
                    verifiedPrevTotal += verifiedResults.Count;
                    _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", verifiedResults.Count, memorySize);
                    SaveSpeedRuns(verifiedResults, false);
                    verifiedResults.ClearMemory();
                }
            }
            while (verifiedRuns.Count == MaxElementsPerPage && verifiedRuns.Min(i => i.VerifyDate ?? SqlMinDateTime) >= lastImportDate);

            if (verifiedResults.Any())
            {
                verifiedResults.RemoveAll(i => (i.VerifyDate ?? SqlMinDateTime) < lastImportDate);
                SaveSpeedRuns(verifiedResults, false);
                verifiedResults.ClearMemory();
            }

            var rejectedDate = lastImportDate.AddDays(RejectedDaysBack);
            var rejectedResults = new List<SpeedRun>();
            var rejectedRuns = new List<SpeedRun>();
            var rejectedPrevTotal = 0;

            do
            {
                rejectedRuns = GetSpeedRunsWithRetry(MaxElementsPerPage, rejectedResults.Count + rejectedPrevTotal, RunsOrdering.DateSubmittedDescending, RunStatusType.Rejected);
                rejectedResults.AddRange(rejectedRuns);
                _logger.Information("Pulled rejected runs: {@New}, total runs: {@Total}", rejectedRuns.Count, rejectedResults.Count + rejectedPrevTotal);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                var memorySize = GC.GetTotalMemory(false);
                if (memorySize > MaxMemorySizeBytes)
                {
                    rejectedPrevTotal += rejectedResults.Count;
                    _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", rejectedResults.Count, memorySize);
                    SaveSpeedRuns(rejectedResults, false);
                    rejectedResults.ClearMemory();
                }
            }
            while (rejectedRuns.Count == MaxElementsPerPage && rejectedRuns.Min(i => i.DateSubmitted ?? SqlMinDateTime) >= rejectedDate);

            if (rejectedResults.Any())
            {
                rejectedResults.RemoveAll(i => (i.DateSubmitted ?? SqlMinDateTime) < rejectedDate);
                SaveSpeedRuns(rejectedResults, false);
                rejectedResults.ClearMemory();
            }
        }
        */

        public List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, string gameID, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<SpeedRun> runs = null;

            try
            {
                runs = clientContainer.Runs.GetRuns(status: statusType, elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, gameId: gameID, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (ex is APIException && ((APIException)ex).Message.Contains("Invalid pagination values"))
                {
                    runs = null;
                    _logger.Information(ex, "GetSpeedRunsWithRetry");
                }
                else if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull speedRuns: {@New}, total speedRuns: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    runs = GetSpeedRunsWithRetry(elementsPerPage, elementsOffset, gameID, orderBy, statusType, retryCount);
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
                if (ex is APIException && ((APIException)ex).Message.Contains("could not be found"))
                {
                    run = null;
                    _logger.Information(ex, "GetSpeedRunWithRetry");
                }
                else if (retryCount <= MaxRetryCount)
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
            var subCategoryVariableIDs = _cacheService.GetVariables().Where(i => i.IsSubCategory).Select(i => i.ID).ToList();
            var runEntities = runs.Select(i => i.ConvertToEntity(subCategoryVariableIDs)).OrderBy(i => i.DateSubmitted).ToList();
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
                _speedRunRepo.InsertSpeedRuns(runEntities, variableValueEntities, playerEntities, videoEntities);
            }
            else
            {
                _speedRunRepo.SaveSpeedRuns(runEntities, variableValueEntities, playerEntities, videoEntities);
            }
        }
    }
}


