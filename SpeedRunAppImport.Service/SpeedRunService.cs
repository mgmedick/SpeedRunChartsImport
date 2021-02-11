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
        private readonly IGameRepository _gameRepo = null;
        private readonly IUserRepository _userRepo = null;
        private readonly IPlatformRepository _platformRepo = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly ILogger _logger;

        public SpeedRunService(ISettingService settingService, ICacheService cacheService, IScrapeService scrapeService, IGameRepository gameRepo, IUserRepository userRepo, IPlatformRepository platformRepo, ISpeedRunRepository speedRunRepo, ILogger logger)
        {
            _settingService = settingService;
            _cacheService = cacheService;
            _scrapeService = scrapeService;
            _gameRepo = gameRepo;
            _userRepo = userRepo;
            _platformRepo = platformRepo;
            _speedRunRepo = speedRunRepo;
            _logger = logger;
        }

        public void ProcessSpeedRuns(DateTime lastImportDate, bool isFullImport, bool isProcessSpeedRunsByGame)
        {
            try
            {
                var lastImportDateUtc = lastImportDate.ToUniversalTime();
                _logger.Information("Started ProcessSpeedRuns: {@LastImportDate}, {@LastImportDateUtc}, {@IsFullImport}", lastImportDate, lastImportDateUtc, isFullImport);

                if (isProcessSpeedRunsByGame)
                {
                    if (isFullImport)
                    {
                        ProcessSpeedRunsByGameFullImport();
                    }
                    else
                    {
                        ProcessSpeedRunsByScreenScrape();
                    }
                }
                else
                {
                    ProcessSpeedRunsDefault(lastImportDateUtc, isFullImport);
                }

                _settingService.UpdateSetting("SpeedRunLastImportDate", DateTime.Now);
                _logger.Information("Completed ProcessSpeedRuns");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessSpeedRuns");
            }
        }

        public void ProcessSpeedRunsDefault(DateTime lastImportDateUtc, bool isFullImport)
        {
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
                runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count() + prevTotal, null, orderBy, RunStatusType.Verified);
                results.AddRange(runs);
                _logger.Information("Pulled runs: {@New}, total runs: {@Total}", runs.Count, results.Count() + prevTotal);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                var memorySize = GC.GetTotalMemory(false);
                if (memorySize > MaxMemorySizeBytes)
                {
                    prevTotal += results.Count;
                    _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                    SaveSpeedRuns(results, true);
                    results.ClearMemory();
                }
            }
            while (runs.Count == MaxElementsPerPage && results.Min(i => i.DateSubmitted ?? SqlMinDateTimeUtc) >= lastImportDateUtc);

            if (!isFullImport)
            {
                results.RemoveAll(i => (i.DateSubmitted ?? SqlMinDateTimeUtc) < lastImportDateUtc);
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, isFullImport);
                results.ClearMemory();
            }

            if (isFullImport)
            {
                _speedRunRepo.RenameAndDropSpeedRunTables();
            }
        }

        #region ProcessSpeedRunsByGame
        public void ProcessSpeedRunsByGameFullImport()
        {
            RunsOrdering orderBy = RunsOrdering.DateSubmitted;
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
            var prevTotal = 0;

            _speedRunRepo.CopySpeedRunTables();

            foreach (var gameSpeedRunComID in gameSpeedRunComIDs)
            {
                var prevGameTotal = 0;
                do
                {
                    runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count(i => i.GameID == gameSpeedRunComID.SpeedRunComID) + prevGameTotal, gameSpeedRunComID.SpeedRunComID, orderBy, RunStatusType.Verified);
                    results.AddRange(runs);
                    _logger.Information("GameID: {@GameID}, pulled runs: {@New}, game total: {@GameTotal}, total runs: {@Total}", gameSpeedRunComID.SpeedRunComID, runs.Count, results.Count(i => i.GameID == gameSpeedRunComID.SpeedRunComID) + prevGameTotal, results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        prevGameTotal += results.Count(i => i.GameID == gameSpeedRunComID.SpeedRunComID);
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, true);
                        results.ClearMemory();
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

        public void ProcessSpeedRunsByScreenScrape()
        {
            var results = new List<SpeedRun>();
            var latestRunIDs = _scrapeService.GetLatestSpeedRunIDs();

            foreach (var runID in latestRunIDs)
            {
                var run = GetSpeedRunWithRetry(runID);
                if (run != null)
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
        }
        #endregion

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
                    runs = new List<SpeedRun>();
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
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs().ToList();
            var categorySpeedRunComIDs = _gameRepo.GetCategorySpeedRunComIDs().ToList();
            var levelSpeedRunComIDs = _gameRepo.GetLevelSpeedRunComIDs().ToList();
            var variableSpeedRunComIDs = _gameRepo.GetVariableSpeedRunComIDs().ToList();
            var variableValueSpeedRunComIDs = _gameRepo.GetVariableValueSpeedRunComIDs().ToList();
            var regionSpeedRunComIDs = _gameRepo.GetRegionSpeedRunComIDs().ToList();
            var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs().ToList();
            var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs().ToList();

            var runEntities = runs.Select(i => new SpeedRunEntity() {
                SpeedRunComID = i.ID,
                GameID = gameSpeedRunComIDs.Where(g => g.SpeedRunComID == i.GameID).Select(g => g.GameID).FirstOrDefault(),
                CategoryID = categorySpeedRunComIDs.Where(g => g.SpeedRunComID == i.CategoryID).Select(g => g.CategoryID).FirstOrDefault(),
                LevelID = levelSpeedRunComIDs.Where(g => g.SpeedRunComID == i.LevelID).Select(g => (int?)g.LevelID).FirstOrDefault(),
                PrimaryTime = i.Times.Primary?.Ticks,
                RunDate = i.Date,
                DateSubmitted = i.DateSubmitted
            }).Where(i => i.GameID != 0 && i.CategoryID != 0)
            .ToList();
            var runLinkEntities = runs.Select(i => new SpeedRunLinkEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                SpeedRunComUrl = i.WebLink.ToString(),
                SplitsUrl = i.SplitsUri?.ToString()
            }).ToList();
            var runStatusEntities = runs.Select(i => new SpeedRunStatusEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                StatusTypeID = (int)i.Status.Type,
                ExaminerUserID = userSpeedRunComIDs.Where(g => g.SpeedRunComID == i.Status.ExaminerUserID).Select(g => (int?)g.UserID).FirstOrDefault(),
                VerifyDate = i.Status.VerifyDate
            }).ToList();
            var runSystemEntities = runs.Select(i => new SpeedRunSystemEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                PlatformID = platformSpeedRunComIDs.Where(g => g.SpeedRunComID == i.LevelID).Select(g => (int?)g.PlatformID).FirstOrDefault(),
                RegionID = regionSpeedRunComIDs.Where(g => g.SpeedRunComID == i.System.RegionID).Select(g => (int?)g.RegionID).FirstOrDefault(),
                IsEmulated = i.System.IsEmulated
            }).ToList();
            var runTimeEntities = runs.Select(i => new SpeedRunTimeEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                PrimaryTime = i.Times.Primary?.Ticks,
                RealTime = i.Times.RealTime?.Ticks,
                RealTimeWithoutLoads = i.Times.RealTimeWithoutLoads?.Ticks,
                GameTime = i.Times.GameTime?.Ticks
            }).ToList();
            var runCommentEntities = runs.Select(i => new SpeedRunCommentEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                Comment = i.Comment
            }).ToList();
            var variableValueEntities = runs.SelectMany(i => i.VariableValueMappings.Select(g => new SpeedRunVariableValueEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                VariableID = variableSpeedRunComIDs.Where(h => h.SpeedRunComID == g.VariableID).Select(g=> g.VariableID).FirstOrDefault(),
                VariableValueID = variableValueSpeedRunComIDs.Where(h => h.SpeedRunComID == g.VariableValueID).Select(g => g.VariableValueID).FirstOrDefault(),
            })).Where(i => i.VariableID != 0 && i.VariableValueID != 0)
            .ToList();
            var playerEntities = runs.SelectMany(i => i.Players.Select(g => new SpeedRunPlayerEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                IsUser = g.IsUser,
                UserID = userSpeedRunComIDs.Where(h => h.SpeedRunComID == g.UserID).Select(h => (int?)h.UserID).FirstOrDefault(),
                GuestName = g.GuestName
            })).ToList();
            var videoEntities = runs.Where(i => i.Videos?.Links != null && i.Videos.Links.Any(g => g != null))
            .SelectMany(i => i.Videos?.Links?.Select((g, n) => new SpeedRunVideoEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                VideoLinkUrl = g?.ToString(),
            })).Where(i => !string.IsNullOrWhiteSpace(i.VideoLinkUrl))
            .ToList();

            SaveSpeedRuns(runEntities, runLinkEntities, runStatusEntities, runSystemEntities, runTimeEntities, runCommentEntities, variableValueEntities, playerEntities, videoEntities, isFullImport);
        }

        public void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunStatusEntity> speedRunStatuses, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos, bool isFullImport)
        {
            if (isFullImport)
            {
                _speedRunRepo.InsertSpeedRuns(speedRuns, speedRunLinks, speedRunStatuses, speedRunSystems, speedRunTimes, speedRunComments, variableValues, players, videos);
            }
            else
            {
                _speedRunRepo.SaveSpeedRuns(speedRuns, speedRunLinks, speedRunStatuses, speedRunSystems, speedRunTimes, speedRunComments, variableValues, players, videos);
            }
        }
    }
}


