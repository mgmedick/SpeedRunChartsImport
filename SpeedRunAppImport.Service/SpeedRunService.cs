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

        public bool ProcessSpeedRuns(DateTime lastImportDateUtc, bool isFullImport, bool isBulkReload, bool isProcessSpeedRunsByGame)
        {
            bool result = true;

            try
            {
                _logger.Information("Started ProcessSpeedRuns: {@LastImportDateUtc}, {@IsFullImport}", lastImportDateUtc, isFullImport);

                if (!isProcessSpeedRunsByGame)
                {
                    ProcessSpeedRunsDefault(lastImportDateUtc, isFullImport, isBulkReload);
                }
                else
                {
                    if (isFullImport)
                    {
                        ProcessSpeedRunsByGameFullImport(isBulkReload);
                    }
                    else
                    {
                        ProcessSpeedRunsByScreenScrape();
                    }
                }

                _logger.Information("Completed ProcessSpeedRuns");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "ProcessSpeedRuns");
            }

            return result;
        }

        public void ProcessSpeedRunsDefault(DateTime lastImportDateUtc, bool isFullImport, bool isBulkReload)
        {
            RunsOrdering orderBy = isFullImport ? RunsOrdering.VerifyDate : RunsOrdering.VerifyDateDescending;
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var prevTotal = 0;
            var updatedLastImportDateUtc = DateTime.UtcNow;

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
            while (runs.Count == MaxElementsPerPage && runs.Min(i => i.Status.VerifyDate ?? SqlMinDateTime) >= lastImportDateUtc);

            if (!isFullImport)
            {
                results.RemoveAll(i => (i.Status.VerifyDate ?? SqlMinDateTime) < lastImportDateUtc);
            }
            else
            {
                updatedLastImportDateUtc = DateTime.UtcNow;
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, isBulkReload);
                _settingService.UpdateSetting("SpeedRunLastSaveDate", DateTime.UtcNow);
                results.ClearMemory();
            }

            _settingService.UpdateSetting("SpeedRunLastImportDate", updatedLastImportDateUtc);
        }

        #region ProcessSpeedRunsByGame
        public void ProcessSpeedRunsByGameFullImport(bool isBulkReload)
        {
            RunsOrdering orderBy = RunsOrdering.DateSubmitted;
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
            var prevTotal = 0;

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
                SaveSpeedRuns(results, isBulkReload);
                results.ClearMemory();
            }

            _settingService.UpdateSetting("SpeedRunLastImportDate", DateTime.UtcNow);
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

            _settingService.UpdateSetting("SpeedRunLastImportDate", DateTime.UtcNow);
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

        public void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isBulkReload)
        {
            _logger.Information("Started SaveSpeedRuns: {@Count}, {@IsBulkReload}", runs.Count(), isBulkReload);

            runs = runs.GroupBy(i => i.ID).Select(i => i.FirstOrDefault()).ToList();
            var runIDs = runs.Select(i => i.ID).ToList();
            var speedRunSpeedRunComIDs = _speedRunRepo.GetSpeedRunSpeedRunComIDs().Where(i => runIDs.Contains(i.SpeedRunComID)).ToList();
            var gameIDs = runs.Select(i => i.GameID).Distinct().ToList();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs().Where(i => gameIDs.Contains(i.SpeedRunComID)).ToList();
            var categoryIDs = runs.Select(i => i.CategoryID).Distinct().ToList();
            var categorySpeedRunComIDs = _gameRepo.GetCategorySpeedRunComIDs().Where(i => categoryIDs.Contains(i.SpeedRunComID)).ToList();
            var levelIDs = runs.Where(i => !string.IsNullOrWhiteSpace(i.LevelID)).Select(i => i.LevelID).Distinct().ToList();
            var levelSpeedRunComIDs = _gameRepo.GetLevelSpeedRunComIDs().Where(i => levelIDs.Contains(i.SpeedRunComID)).ToList();
            var variableIDs = runs.SelectMany(i => i.VariableValueMappings.Select(g => g.VariableID)).Distinct().ToList();
            var variableSpeedRunComIDs = _gameRepo.GetVaraibleSpeedRunComIDs().Where(i => variableIDs.Contains(i.SpeedRunComID)).ToList();
            var variableValueIDs = runs.SelectMany(i => i.VariableValueMappings.Select(g => g.VariableValueID)).Distinct().ToList();
            var variableValueSpeedRunComIDs = _gameRepo.GetVariableValueSpeedRunComIDs().Where(i => variableValueIDs.Contains(i.SpeedRunComID)).ToList();
            var playerUserIDs = runs.SelectMany(i => i.Players.Where(i => !string.IsNullOrWhiteSpace(i.UserID)).Select(i => i.UserID)).Distinct().ToList();
            var examinerUserIDs = runs.Where(i => !string.IsNullOrWhiteSpace(i.Status.ExaminerUserID)).Select(i => i.Status.ExaminerUserID).Distinct().ToList();
            var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();
            var playerUserSpeedRunComIDs = userSpeedRunComIDs.Where(i => playerUserIDs.Contains(i.SpeedRunComID)).ToList();
            var examinerUserSpeedRunComIDs = userSpeedRunComIDs.Where(i => examinerUserIDs.Contains(i.SpeedRunComID)).ToList();
            var regionIDs = runs.Where(i => !string.IsNullOrWhiteSpace(i.System.RegionID)).Select(i => i.System.RegionID).Distinct().ToList();
            var regionSpeedRunComIDs = _gameRepo.GetRegionSpeedRunComIDs(i => regionIDs.Contains(i.SpeedRunComID)).ToList();
            var platformIDs = runs.Where(i => !string.IsNullOrWhiteSpace(i.System.PlatformID)).Select(i => i.System.PlatformID).Distinct().ToList();
            var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs(i => platformIDs.Contains(i.SpeedRunComID)).ToList();

            var runEntities = runs.Select(i => new SpeedRunEntity()
            {
                ID = speedRunSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.SpeedRunID).FirstOrDefault(),
                SpeedRunComID = i.ID,
                StatusTypeID = (int)i.Status.Type,
                GameID = gameSpeedRunComIDs.Where(g => g.SpeedRunComID == i.GameID).Select(g => g.GameID).FirstOrDefault(),
                CategoryID = categorySpeedRunComIDs.Where(g => g.SpeedRunComID == i.CategoryID).Select(g => g.CategoryID).FirstOrDefault(),
                LevelID = !string.IsNullOrWhiteSpace(i.LevelID) ? levelSpeedRunComIDs.Where(g => g.SpeedRunComID == i.LevelID).Select(g => g.LevelID).FirstOrDefault() : (int?)null,
                PrimaryTime = i.Times.Primary?.Ticks,
                ExaminerUserID = examinerUserSpeedRunComIDs.Where(g => g.SpeedRunComID == i.Status.ExaminerUserID).Select(g => (int?)g.UserID).FirstOrDefault(),
                RunDate = i.Date,
                DateSubmitted = i.DateSubmitted,
                VerifyDate = i.Status.VerifyDate
            }).Where(i => i.GameID != 0 && i.CategoryID != 0 && i.LevelID != 0)
            .ToList();
            var runLinkEntities = runs.Select(i => new SpeedRunLinkEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                SpeedRunComUrl = i.WebLink.ToString(),
                SplitsUrl = i.SplitsUri?.ToString()
            }).ToList();
            var runSystemEntities = runs.Select(i => new SpeedRunSystemEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                PlatformID = !string.IsNullOrWhiteSpace(i.System.PlatformID) ? platformSpeedRunComIDs.Where(g => g.SpeedRunComID == i.System.PlatformID).Select(g => g.PlatformID).FirstOrDefault() : (int?)null,
                RegionID = !string.IsNullOrWhiteSpace(i.System.RegionID) ? regionSpeedRunComIDs.Where(g => g.SpeedRunComID == i.System.RegionID).Select(g => g.RegionID).FirstOrDefault() : (int?)null,
                IsEmulated = i.System.IsEmulated
            }).Where(i => i.PlatformID != 0 && i.RegionID != 0)
            .ToList();
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
                VariableID = variableSpeedRunComIDs.Where(h => h.SpeedRunComID == g.VariableID).Select(g => g.VariableID).FirstOrDefault(),
                VariableValueID = variableValueSpeedRunComIDs.Where(h => h.SpeedRunComID == g.VariableValueID).Select(g => g.VariableValueID).FirstOrDefault(),
            })).Where(i => i.VariableID != 0 && i.VariableValueID != 0)
            .ToList();
            var playerEntities = runs.SelectMany(i => i.Players.Select(g => new SpeedRunPlayerEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                IsUser = g.IsUser,
                UserID = !string.IsNullOrWhiteSpace(g.UserID) ? playerUserSpeedRunComIDs.Where(h => h.SpeedRunComID == g.UserID).Select(h => h.UserID).FirstOrDefault() : (int?)null,
                GuestName = g.GuestName
            })).Where(i => i.UserID != 0)
            .ToList();
            var videoEntities = runs.Where(i => i.Videos?.Links != null && i.Videos.Links.Any(g => g != null))
            .SelectMany(i => i.Videos?.Links?.Select((g, n) => new SpeedRunVideoEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                VideoLinkUrl = g?.ToString(),
            })).Where(i => !string.IsNullOrWhiteSpace(i.VideoLinkUrl))
            .ToList();

            if (isBulkReload)
            {
                _speedRunRepo.InsertSpeedRuns(runEntities, runLinkEntities, runSystemEntities, runTimeEntities, runCommentEntities, variableValueEntities, playerEntities, videoEntities);
            }
            else
            {
                _speedRunRepo.SaveSpeedRuns(runEntities, runLinkEntities, runSystemEntities, runTimeEntities, runCommentEntities, variableValueEntities, playerEntities, videoEntities);
            }

            _logger.Information("Completed SaveSpeedRuns");
        }
    }
}


