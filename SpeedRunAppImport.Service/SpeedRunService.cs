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
using Microsoft.AspNetCore.WebUtilities;

namespace SpeedRunAppImport.Service
{
    public class SpeedRunService : BaseService, ISpeedRunService
    {
        private readonly ISettingService _settingService = null;
        private readonly ICacheService _cacheService = null;
        private readonly IScrapeService _scrapeService = null;
        private readonly IUserService _userService = null;
        private readonly IGameRepository _gameRepo = null;
        private readonly IUserRepository _userRepo = null;
        private readonly IPlatformRepository _platformRepo = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly ILogger _logger;

        public SpeedRunService(ISettingService settingService, ICacheService cacheService, IScrapeService scrapeService, IUserService userService, IGameRepository gameRepo, IUserRepository userRepo, IPlatformRepository platformRepo, ISpeedRunRepository speedRunRepo, ILogger logger)
        {
            _settingService = settingService;
            _cacheService = cacheService;
            _scrapeService = scrapeService;
            _userService = userService;
            _gameRepo = gameRepo;
            _userRepo = userRepo;
            _platformRepo = platformRepo;
            _speedRunRepo = speedRunRepo;
            _logger = logger;
        }

        public bool ProcessSpeedRuns(DateTime lastImportDateUtc, DateTime importLastRunDateUtc, bool isFullPull, bool isBulkReload, bool isUpdateSpeedRuns)
        {
            bool result = true;

            try
            {
                _logger.Information("Started ProcessSpeedRuns: {@LastImportDateUtc}, {@ImportLastRunDateUtc}, {@IsFullPull}, {@IsBulkReload}, {@IsUpdateSpeedRuns}", lastImportDateUtc, importLastRunDateUtc, isFullPull, isBulkReload, isUpdateSpeedRuns);

                if (isUpdateSpeedRuns)
                {
                    ProcessSpeedRunsByGame(importLastRunDateUtc, isFullPull, isBulkReload);
                }
                else
                {
                    ProcessSpeedRunsDefault(lastImportDateUtc, isFullPull, isBulkReload);

                    if (GameIDsToUpdateSpeedRuns.Any())
                    {
                        ProcessSpeedRunsByGame(importLastRunDateUtc, isFullPull, isBulkReload);
                    }

                    if (UserIDsToUpdateSpeedRuns.Any() && !isBulkReload)
                    {
                        ProcessSpeedRunsByUser(importLastRunDateUtc);
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

        public void ProcessSpeedRunsDefault(DateTime lastImportDateUtc, bool isFullPull, bool isBulkReload)
        {
            RunsOrdering orderBy = isFullPull ? RunsOrdering.VerifyDate : RunsOrdering.VerifyDateDescending;
            var runEmbeds = new SpeedRunEmbeds { EmbedCategory = false, EmbedGame = false, EmbedLevel = false, EmbedPlayers = true, EmbedPlatform = false, EmbedRegion = false };
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var total = 0;
            var offset = 0;

            do
            {
                runs = GetSpeedRunsWithRetry(MaxElementsPerPage, offset, null, null, runEmbeds, orderBy, RunStatusType.Verified);
                if (!isFullPull)
                {
                    runs.RemoveAll(i => (i.Status.VerifyDate ?? SqlMinDateTime) <= lastImportDateUtc);
                }

                results.AddRange(runs);
                total += runs.Count;
                offset += runs.Count;

                _logger.Information("Pulled runs: {@New}, total runs: {@Total}", runs.Count, total);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                var memorySize = GC.GetTotalMemory(false);
                if (memorySize > MaxMemorySizeBytes)
                {
                    _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                    SaveSpeedRuns(results, isBulkReload, false);
                    results.ClearMemory();
                }
            }
            while (runs.Count > 0);

            if (results.Any())
            {
                SaveSpeedRuns(results, isBulkReload, false);
                var lastUpdateDate = results.Max(i => i.Status.VerifyDate) ?? DateTime.UtcNow;
                _settingService.UpdateSetting("SpeedRunLastImportDate", lastUpdateDate);
                results.ClearMemory();
            }
        }

        #region ProcessSpeedRunsByGame
        public void ProcessSpeedRunsByGame(DateTime importLastRunDateUtc, bool isFullPull, bool isBulkReload)
        {
            RunsOrdering orderBy = default(RunsOrdering);
            var runEmbeds = new SpeedRunEmbeds { EmbedCategory = false, EmbedGame = false, EmbedLevel = false, EmbedPlayers = true, EmbedPlatform = false, EmbedRegion = false };
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
            var gameIDs = isFullPull ? _gameRepo.GetGames().Select(i => i.ID).ToList() : GameIDsToUpdateSpeedRuns;
            gameSpeedRunComIDs = gameSpeedRunComIDs.Join(gameIDs, o => o.GameID, id => id, (o, id) => o).ToList();

            _logger.Information("Found NewOrChangedGames: {@Count}, ImportLastRunDate: {ImportLastRunDateUtc}", gameSpeedRunComIDs.Count(), importLastRunDateUtc);

            var prevTotal = 0;
            foreach (var gameSpeedRunComID in gameSpeedRunComIDs)
            {
                var prevGameTotal = 0;
                do
                {
                    try
                    {
                        runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count(i => i.GameID == gameSpeedRunComID.SpeedRunComID) + prevGameTotal, gameSpeedRunComID.SpeedRunComID, null, runEmbeds, orderBy, RunStatusType.Verified);
                    }
                    catch (APIException ex)
                    {
                        if (ex.Message.Contains("Invalid pagination values"))
                        {
                            DeleteGameSpeedRunsAndProcessByCategory(gameSpeedRunComID.GameID, gameSpeedRunComID.SpeedRunComID, runEmbeds, orderBy, isBulkReload, results, ref prevTotal);
                            //if (!isBulkReload)
                            //{
                            //    DeleteGameSpeedRunsAndProcessByCategory(gameSpeedRunComID.GameID, gameSpeedRunComID.SpeedRunComID, runEmbeds, orderBy, isBulkReload, results, ref prevTotal);
                            //}
                            break;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    results.AddRange(runs);
                    _logger.Information("GameID: {@GameID}, pulled runs: {@New}, game total: {@GameTotal}, total runs: {@Total}",
                                         gameSpeedRunComID.SpeedRunComID,
                                         runs.Count,
                                         results.Count(i => i.GameID == gameSpeedRunComID.SpeedRunComID) + prevGameTotal,
                                         results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        prevGameTotal += results.Count(i => i.GameID == gameSpeedRunComID.SpeedRunComID);
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, isBulkReload, true);
                        results.ClearMemory();
                    }
                }
                while (runs.Count == MaxElementsPerPage);
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, isBulkReload, true);
                results.ClearMemory();

                if (!isBulkReload)
                {
                    DeleteObsoleteSpeedRuns(gameSpeedRunComIDs, importLastRunDateUtc);
                } 
            }
        }

        public void ProcessSpeedRunsByUser(DateTime importLastRunDateUtc)
        {
            RunsOrdering orderBy = default(RunsOrdering);
            var runEmbeds = new SpeedRunEmbeds { EmbedCategory = false, EmbedGame = false, EmbedLevel = false, EmbedPlayers = true, EmbedPlatform = false, EmbedRegion = false };
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();
            var userIDs = UserIDsToUpdateSpeedRuns;
            userSpeedRunComIDs = userSpeedRunComIDs.Join(userIDs, o => o.UserID, id => id, (o, id) => o).ToList();

            _logger.Information("Found NewOrChangedUsers: {@Count}, ImportLastRunDate: {ImportLastRunDateUtc}", userSpeedRunComIDs.Count(), importLastRunDateUtc);

            var prevTotal = 0;
            foreach (var userSpeedRunComID in userSpeedRunComIDs)
            {
                var prevUserTotal = 0;
                do
                {
                    try
                    {
                        runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count(i => i.PlayerUsers != null && i.PlayerUsers.Any(g => g.ID == userSpeedRunComID.SpeedRunComID)) + prevUserTotal, null, null, runEmbeds, orderBy, RunStatusType.Verified, userSpeedRunComID.SpeedRunComID);
                    }
                    catch (APIException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    results.AddRange(runs);
                    _logger.Information("UserID: {@UserID}, pulled runs: {@New}, user total: {@UserTotal}, total runs: {@Total}",
                                         userSpeedRunComID.SpeedRunComID,
                                         runs.Count,
                                         results.Count(i => i.PlayerUsers != null && i.PlayerUsers.Any(g => g.ID == userSpeedRunComID.SpeedRunComID)) + prevUserTotal,
                                         results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        prevUserTotal += results.Count(i => i.PlayerUsers != null && i.PlayerUsers.Any(g => g.ID == userSpeedRunComID.SpeedRunComID));
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, false, true);
                        results.ClearMemory();
                    }
                }
                while (runs.Count == MaxElementsPerPage);
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, false, true);
                results.ClearMemory();

                DeleteObsoleteSpeedRunsByUser(userSpeedRunComIDs, importLastRunDateUtc);
            }
        }

        private void DeleteObsoleteSpeedRuns(IEnumerable<GameSpeedRunComIDEntity> gameSpeedRunComIDs, DateTime importLastRunDateUtc)
        {
            _logger.Information("Started DeleteObsoleteSpeedRuns: GameCount: {@GameCount}, LastImportDateUTC: {@ImportLastRunDateUtc}", gameSpeedRunComIDs.Count(), importLastRunDateUtc);

            foreach (var gameSpeedRunComID in gameSpeedRunComIDs)
            {
                _logger.Information("Deleting obsolete speedruns for GameID: {@GameID}", gameSpeedRunComID.GameID);
                var paramString = string.Format("GameID = {0} AND COALESCE(ModifiedDate, ImportedDate) < '{1}'", gameSpeedRunComID.GameID, importLastRunDateUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                _speedRunRepo.DeleteSpeedRuns(paramString);
                _logger.Information("Completed deleting obsolete speedruns for GameID: {@GameID}", gameSpeedRunComID.GameID);
            }

            _logger.Information("Completed DeleteObsoleteSpeedRuns: GameCount: {@GameCount}, LastImportDateUTC: {@ImportLastRunDateUtc}", gameSpeedRunComIDs.Count(), importLastRunDateUtc);
        }

        private void DeleteObsoleteSpeedRunsByUser(IEnumerable<UserSpeedRunComIDEntity> userSpeedRunComIDs, DateTime importLastRunDateUtc)
        {
            _logger.Information("Started DeleteObsoleteSpeedRunsByUser: UserCount: {@UserCount}, LastImportDateUTC: {@ImportLastRunDateUtc}", userSpeedRunComIDs.Count(), importLastRunDateUtc);

            foreach (var userSpeedRunComID in userSpeedRunComIDs)
            {
                _logger.Information("Deleting obsolete speedruns for UserID: {@UserID}", userSpeedRunComID.UserID);
                var paramString = string.Format("EXISTS(SELECT 1 FROM tbl_SpeedRun_Player sp WHERE sp.SpeedRunID = ID AND sp.UserID = {0}) AND COALESCE(ModifiedDate, ImportedDate) < '{1}'", userSpeedRunComID.UserID, importLastRunDateUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                _speedRunRepo.DeleteSpeedRuns(paramString);
                _logger.Information("Completed deleting obsolete speedruns for UserID: {@UserID}", userSpeedRunComID.UserID);
            }
            _logger.Information("Completed DeleteObsoleteSpeedRunsByUser: UserCount: {@UserCount}, LastImportDateUTC: {@ImportLastRunDateUtc}", userSpeedRunComIDs.Count(), importLastRunDateUtc);
        }

        private void DeleteGameSpeedRunsAndProcessByCategory(int gameID, string gameSpeedRunComID, SpeedRunEmbeds runEmbeds, RunsOrdering orderBy, bool isBulkReload, List<SpeedRun> results, ref int prevTotal)
        {
            results.RemoveAll(i => i.GameID == gameSpeedRunComID);
            if (isBulkReload)
            {
                _speedRunRepo.DeleteSpeedRuns(i => i.GameID == gameID);
            }

            var categorySpeedRunComIDStrings = _gameRepo.GetGameSpeedRunComViews(i => i.SpeedRunComID == gameSpeedRunComID)
                                               .SelectMany(i => i.CategorySpeedRunComIDArray)
                                               .ToList();
            var categorySpeedRunComIDs = _gameRepo.GetCategorySpeedRunComIDs(i => categorySpeedRunComIDStrings.Contains(i.SpeedRunComID)).ToList();

            ProcessSpeedRunsByCategory(gameID, gameSpeedRunComID, categorySpeedRunComIDs, runEmbeds, orderBy, isBulkReload, results, ref prevTotal);
        }

        private void ProcessSpeedRunsByCategory(int gameID, string gameSpeedRunComID, IEnumerable<CategorySpeedRunComIDEntity> categorySpeedRunComIDs, SpeedRunEmbeds runEmbeds, RunsOrdering orderBy, bool isBulkReload, List<SpeedRun> results, ref int prevTotal)
        {
            var runs = new List<SpeedRun>();

            foreach (var categorySpeedRunComID in categorySpeedRunComIDs)
            {
                var prevCategoryTotal = 0;
                do
                {
                    try
                    {
                        runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID.SpeedRunComID) + prevCategoryTotal, gameSpeedRunComID, categorySpeedRunComID.SpeedRunComID, runEmbeds, RunsOrdering.DateSubmitted, RunStatusType.Verified);
                    }
                    catch (APIException ex)
                    {
                        if (ex.Message.Contains("Invalid pagination values"))
                        {
                            ProcessSpeedRunsByCategoryDesc(gameSpeedRunComID, categorySpeedRunComID.SpeedRunComID, runEmbeds, RunsOrdering.DateSubmittedDescending, isBulkReload, results, ref prevTotal);
                            break;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    results.AddRange(runs);
                    _logger.Information("GameID: {@GameID}, CategoryID: {@CategoryID}, pulled runs: {@New}, order: asc, category total: {@CategoryTotal}, total runs: {@Total}",
                                         gameSpeedRunComID,
                                         categorySpeedRunComID.SpeedRunComID,
                                         runs.Count,
                                         results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID.SpeedRunComID) + prevCategoryTotal,
                                         results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        prevCategoryTotal += results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID.SpeedRunComID);
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, isBulkReload, true);
                        results.ClearMemory();
                    }
                }
                while (runs.Count == MaxElementsPerPage);
            }
        }

        private void ProcessSpeedRunsByCategoryDesc(string gameSpeedRunComID, string categorySpeedRunComID, SpeedRunEmbeds runEmbeds, RunsOrdering orderBy, bool isBulkReload, List<SpeedRun> results, ref int prevTotal)
        {
            var runs = new List<SpeedRun>();
            var lastDateSubmitted = results.Where(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID).Select(i => i.DateSubmitted).LastOrDefault();
            var prevCategoryTotal = 0;
            var prevCategoryDescTotal = 0;

            do
            {
                try
                {
                    runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID && i.DateSubmitted > lastDateSubmitted) + prevCategoryDescTotal, gameSpeedRunComID, categorySpeedRunComID, runEmbeds, orderBy, RunStatusType.Verified);
                    runs = runs.Where(i => i.DateSubmitted > lastDateSubmitted).ToList();
                }
                catch (APIException ex)
                {
                    if (ex.Message.Contains("Invalid pagination values"))
                    {
                        break;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                results.AddRange(runs);
                _logger.Information("GameID: {@GameID}, CategoryID: {@CategoryID}, order: desc, pulled runs: {@New}, category total: {@CategoryTotal}, total runs: {@Total}",
                                        gameSpeedRunComID,
                                        categorySpeedRunComID,
                                        runs.Count,
                                        results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID) + prevCategoryTotal,
                                        results.Count + prevTotal);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                var memorySize = GC.GetTotalMemory(false);
                if (memorySize > MaxMemorySizeBytes)
                {
                    prevTotal += results.Count;
                    prevCategoryTotal += results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID);
                    prevCategoryDescTotal += results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID && i.DateSubmitted > lastDateSubmitted);
                    _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                    SaveSpeedRuns(results, isBulkReload, true);
                    results.ClearMemory();
                }
            }
            while (runs.Count == MaxElementsPerPage);
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
                        SaveSpeedRuns(results, false, false);
                        results.ClearMemory();
                    }
                }
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, false, false);
                var lastUpdateDate = results.Max(i => i.Status.VerifyDate) ?? DateTime.UtcNow;
                _settingService.UpdateSetting("SpeedRunLastImportDate", lastUpdateDate);
                results.ClearMemory();
            }

        }
        #endregion

        public List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, string gameID, string categoryID, SpeedRunEmbeds embeds, RunsOrdering orderBy, RunStatusType? statusType = null, string userID = null, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<SpeedRun> runs = null;

            try
            {
                if (string.IsNullOrWhiteSpace(userID))
                {
                    runs = clientContainer.Runs.GetRuns(status: statusType, elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, gameId: gameID, categoryId: categoryID, embeds: embeds, orderBy: orderBy).ToList();
                }
                else
                {
                    runs = clientContainer.Runs.GetRuns(status: statusType, elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, gameId: null, categoryId: null, embeds: embeds, orderBy: orderBy, userId: userID).ToList();
                }
            }
            catch (Exception ex)
            {
                if (ex is APIException && ((APIException)ex).Message.Contains("Invalid pagination values"))
                {
                    _logger.Information(ex, "GetSpeedRunsWithRetry - Invalid pagination values - GameID: {gameID}, UserID: {userID}", gameID, userID);
                    throw ex;
                }
                else if (ex is APIException && ((APIException)ex).Message.Contains("Non-existing"))
                {
                    runs = new List<SpeedRun>();
                    _logger.Information(ex, "GetSpeedRunsWithRetry - Non-existing - GameID: {gameID}, CategoryID: {categoryID}, UserID: {userID}", gameID, categoryID, userID);
                }
                else if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull speedRuns: {@New}, total speedRuns: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    runs = GetSpeedRunsWithRetry(elementsPerPage, elementsOffset, gameID, categoryID, embeds, orderBy, statusType, userID, retryCount);
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

        public void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isBulkReload, bool isUpdateSpeedRuns)
        {
            _logger.Information("Started SaveSpeedRuns: {@IsBulkReload}, {@IsUpdateSpeedRuns}", isBulkReload, isUpdateSpeedRuns);

            runs = runs.GroupBy(i => i.ID).Select(i => i.FirstOrDefault()).OrderBy(i => i.Status.VerifyDate).ToList();

            var runIDs = runs.Select(i => i.ID).ToList();
            var speedRunSpeedRunComIDs = _speedRunRepo.GetSpeedRunSpeedRunComIDs();
            speedRunSpeedRunComIDs = speedRunSpeedRunComIDs.Join(runIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var gameIDs = runs.Select(i => i.GameID).Distinct().ToList();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
            gameSpeedRunComIDs = gameSpeedRunComIDs.Join(gameIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var categoryIDs = runs.Select(i => i.CategoryID).Distinct().ToList();
            var categorySpeedRunComIDs = _gameRepo.GetCategorySpeedRunComIDs();
            categorySpeedRunComIDs = categorySpeedRunComIDs.Join(categoryIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var levelIDs = runs.Where(i => !string.IsNullOrWhiteSpace(i.LevelID)).Select(i => i.LevelID).Distinct().ToList();
            var levelSpeedRunComIDs = _gameRepo.GetLevelSpeedRunComIDs();
            levelSpeedRunComIDs = levelSpeedRunComIDs.Join(levelIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var variableIDs = runs.SelectMany(i => i.VariableValueMappings.Select(g => g.VariableID)).Distinct().ToList();
            var variableSpeedRunComIDs = _gameRepo.GetVaraibleSpeedRunComIDs();
            variableSpeedRunComIDs = variableSpeedRunComIDs.Join(variableIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var variableValueIDs = runs.SelectMany(i => i.VariableValueMappings.Select(g => g.VariableValueID)).Distinct().ToList();
            var variableValueSpeedRunComIDs = _gameRepo.GetVariableValueSpeedRunComIDs();
            variableValueSpeedRunComIDs = variableValueSpeedRunComIDs.Join(variableValueIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var regionIDs = runs.Where(i => !string.IsNullOrWhiteSpace(i.System.RegionID)).Select(i => i.System.RegionID).Distinct().ToList();
            var regionSpeedRunComIDs = _gameRepo.GetRegionSpeedRunComIDs();
            regionSpeedRunComIDs = regionSpeedRunComIDs.Join(regionIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var platformIDs = runs.Where(i => !string.IsNullOrWhiteSpace(i.System.PlatformID)).Select(i => i.System.PlatformID).Distinct().ToList();
            var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs();
            platformSpeedRunComIDs = platformSpeedRunComIDs.Join(platformIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var userIDs = runs.Where(i => i.PlayerUsers != null).SelectMany(i => i.PlayerUsers.Select(i => i.ID)).Distinct().ToList();
            var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();
            userSpeedRunComIDs = userSpeedRunComIDs.Join(userIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var guestIDs = runs.Where(i => i.PlayerGuests != null).SelectMany(i => i.PlayerGuests.Select(i => i.Name)).Distinct().ToList();
            var guestSpeedRunComIDs = _userRepo.GetGuests();
            guestSpeedRunComIDs = guestSpeedRunComIDs.Join(guestIDs, o => o.Name, id => id, (o, id) => o).ToList();

            var users = runs.Where(i => i.PlayerUsers != null)
                            //.SelectMany(i => i.PlayerUsers.Where(i => !userSpeedRunComIDs.Any(g => g.SpeedRunComID == i.ID)))
                            .SelectMany(i => i.PlayerUsers)
                            .GroupBy(g => new { g.ID })
                            .Select(i => i.First())
                            .OrderBy(i => i.ID)
                            .ToList();
            if (users.Any())
            {
                _userService.SaveUsers(users, isBulkReload, userSpeedRunComIDs);
                userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();
                userSpeedRunComIDs = userSpeedRunComIDs.Join(userIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();
            }

            var guests = runs.Where(i => i.PlayerGuests != null)
                             .SelectMany(i => i.PlayerGuests.Where(i => !guestSpeedRunComIDs.Any(g => g.Name == i.Name)))
                             .GroupBy(g => new { g.Name })
                             .Select(i => i.First())
                             .ToList();
            if (guests.Any())
            {
                _userService.SaveGuests(guests, isBulkReload, guestSpeedRunComIDs);
                guestSpeedRunComIDs = _userRepo.GetGuests();
                guestSpeedRunComIDs = guestSpeedRunComIDs.Join(guestIDs, o => o.Name, id => id, (o, id) => o).ToList();
            }

            var runEntities = runs.Select(i => new SpeedRunEntity()
            {
                ID = speedRunSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.SpeedRunID).FirstOrDefault(),
                SpeedRunComID = i.ID,
                StatusTypeID = (int)i.Status.Type,
                GameID = gameSpeedRunComIDs.Where(g => g.SpeedRunComID == i.GameID).Select(g => g.GameID).FirstOrDefault(),
                CategoryID = categorySpeedRunComIDs.Where(g => g.SpeedRunComID == i.CategoryID).Select(g => g.CategoryID).FirstOrDefault(),
                LevelID = !string.IsNullOrWhiteSpace(i.LevelID) ? levelSpeedRunComIDs.Where(g => g.SpeedRunComID == i.LevelID).Select(g => g.LevelID).FirstOrDefault() : (int?)null,
                PrimaryTime = i.Times.Primary?.Ticks,
                IsExcludeFromSpeedRunList = !(i.Status.VerifyDate ?? i.DateSubmitted).HasValue,
                RunDate = i.Date,
                DateSubmitted = i.DateSubmitted,
                VerifyDate = i.Status.VerifyDate
            }).Where(i => i.GameID != 0 && i.CategoryID != 0 && i.LevelID != 0)
            .ToList();
            if (!isBulkReload && isUpdateSpeedRuns)
            {
                foreach(var runEntity in runEntities)
                {
                    if (runEntity.ID == 0)
                    {
                        runEntity.IsExcludeFromSpeedRunList = true;
                    }
                }
            }
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
                VariableID = variableSpeedRunComIDs.Where(h => h.SpeedRunComID == g.VariableID).Select(h => h.VariableID).FirstOrDefault(),
                VariableValueID = variableValueSpeedRunComIDs.Where(h => h.SpeedRunComID == g.VariableValueID).Select(h => h.VariableValueID).FirstOrDefault(),
            })).Where(i => i.VariableID != 0 && i.VariableValueID != 0)
            .ToList();
            var playerEntities = runs.Where(i => i.PlayerUsers != null).SelectMany(i => i.PlayerUsers.Select(g => new SpeedRunPlayerEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                UserID = userSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(h => h.UserID).FirstOrDefault()
            })).Where(i => i.UserID != 0)
            .ToList();
            var videoEntities = runs.Where(i => i.Videos?.Links != null && i.Videos.Links.Any(g => g != null))
            .SelectMany(i => i.Videos?.Links?.Select((g, n) => new SpeedRunVideoEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                VideoLinkUri = g,
                VideoLinkUrl = g?.ToString(),
                EmbeddedVideoLinkUrl = g?.ToEmbeddedURIString(),
                ThumbnailLinkUrl = g?.ToThumbnailURIString()
            })).Where(i => !string.IsNullOrWhiteSpace(i.VideoLinkUrl))
            .GroupBy(h => new { h.SpeedRunSpeedRunComID, h.VideoLinkUrl })
            .Select(n => n.First())
            .ToList();
            for (int i = 0; i < videoEntities.Count; i++)
            {
                videoEntities[i].LocalID = i + 1;
            }
            var videoDetailEntities = GetSpeedRunVideoDetails(videoEntities, isBulkReload);
            foreach (var videoDetail in videoDetailEntities)
            {
                if (!string.IsNullOrWhiteSpace(videoDetail.ThumbnailLinkUrl))
                {
                    var video = videoEntities.Find(g => g.LocalID == videoDetail.SpeedRunVideoLocalID);
                    video.ThumbnailLinkUrl = videoDetail.ThumbnailLinkUrl;
                }
            }
            var guestPlayerEntities = runs.Where(i => i.PlayerGuests != null).SelectMany(i => i.PlayerGuests.Select(g => new SpeedRunGuestEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                GuestID = guestSpeedRunComIDs.Where(h => h.Name == g.Name).Select(h => h.ID).FirstOrDefault()
            })).Where(i => i.GuestID != 0)
            .ToList();

            if (isBulkReload)
            {
                _speedRunRepo.InsertSpeedRuns(runEntities, runLinkEntities, runSystemEntities, runTimeEntities, runCommentEntities, variableValueEntities, playerEntities, guestPlayerEntities, videoEntities, videoDetailEntities);
            }
            else
            {
                _speedRunRepo.SaveSpeedRuns(runEntities, runLinkEntities, runSystemEntities, runTimeEntities, runCommentEntities, variableValueEntities, playerEntities, guestPlayerEntities, videoEntities, videoDetailEntities);
            }

            IsSpeedRunsImported = true;
            _logger.Information("Completed SaveSpeedRuns");
        }

        public bool UpdateSpeedRunVideoDetails(bool isPostBulkImport, DateTime importLastRunDateUtc)
        {
            bool result = true;

            try
            {
                _logger.Information("Started UpdateSpeedRunVideoDetails {@IsPostBulkImport}, {@ImportLastRunDateUtc}", isPostBulkImport, importLastRunDateUtc);
                var maxVideoCount = YouTubeAPIDailyRequestLimit * YouTubeAPIMaxBatchCount;

                var refDate = importLastRunDateUtc.AddDays(-14);
                var videoViews = _speedRunRepo.GetSpeedRunVideoViews(i => i.VideoLinkUrl != null && (isPostBulkImport || i.VerifyDate >= refDate) && (i.VideoLinkUrl.Contains("youtube.com") || i.VideoLinkUrl.Contains("youtu.be"))).OrderByDescending(i => i.SpeedRunVideoID).Take(maxVideoCount).ToList();

                if (!isPostBulkImport)
                {
                    var twitchVideoViews = _speedRunRepo.GetSpeedRunVideoViews(i => i.VideoLinkUrl != null && i.VerifyDate >= refDate && i.VideoLinkUrl.Contains("twitch.tv")).OrderByDescending(i => i.SpeedRunVideoID).Take(maxVideoCount).ToList();
                    videoViews.AddRange(twitchVideoViews);
                }

                var videos = videoViews.Select(i => new SpeedRunVideoEntity() { ID = i.SpeedRunVideoID, SpeedRunID = i.SpeedRunID, VideoLinkUrl = i.VideoLinkUrl, VideoLinkUri = new Uri(i.VideoLinkUrl), ThumbnailLinkUrl = i.ThumbnailLinkUrl, EmbeddedVideoLinkUrl = i.EmbeddedVideoLinkUrl }).ToList();
                
                var videoDetails = GetSpeedRunVideoDetails(videos, false);
                var videosToUpdate = new List<SpeedRunVideoEntity>();
                var videoDetailsToUpdate = new List<SpeedRunVideoDetailEntity>();
                var videoDetailsToInsert = new List<SpeedRunVideoDetailEntity>();
                foreach (var videoDetail in videoDetails)
                {
                    var video = videos.Find(g => g.ID == videoDetail.SpeedRunVideoID);

                    if (!string.IsNullOrWhiteSpace(videoDetail.ThumbnailLinkUrl) && (string.IsNullOrWhiteSpace(video.ThumbnailLinkUrl) || (video.ThumbnailLinkUrl.EndsWith("hqdefault.jpg") && videoDetail.ThumbnailLinkUrl.EndsWith("maxresdefault.jpg"))))
                    {
                        video.ThumbnailLinkUrl = videoDetail.ThumbnailLinkUrl;
                        videosToUpdate.Add(video);
                    }

                    var videoVW = videoViews.Find(g => g.SpeedRunVideoID == videoDetail.SpeedRunVideoID);
                    if (!videoVW.HasDetails)
                    {
                        videoDetailsToInsert.Add(videoDetail);
                    }
                    else if (videoVW.ViewCount != videoDetail.ViewCount)
                    {
                        videoDetailsToUpdate.Add(videoDetail);
                    }
                }

                _speedRunRepo.UpdateSpeedRunVideoThumbnailLinkUrls(videosToUpdate);
                _speedRunRepo.UpdateSpeedRunVideoDetailVideoCounts(videoDetailsToUpdate);
                _speedRunRepo.InsertSpeedRunVideoDetails(videoDetailsToInsert);

                _logger.Information("Completed UpdateSpeedRunVideoDetails");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "UpdateSpeedRunVideoDetails");
            }

            return result;
        }

        public bool ProcessYouTubeSpeedRunVideoDetails()
        {
            bool result = true;

            try
            {
                _logger.Information("Started ProcessYouTubeSpeedRunVideoDetails");

                var videos = _speedRunRepo.GetSpeedRunVideos(i => i.VideoLinkUrl != null && (i.VideoLinkUrl.Contains("youtube.com") || i.VideoLinkUrl.Contains("youtu.be"))).ToList();

                var maxYouTubeVideoCount = YouTubeAPIDailyRequestLimit * YouTubeAPIMaxBatchCount;
                videos = videos.OrderByDescending(i => i.ID).Take(maxYouTubeVideoCount).ToList();

                for (int i = 0; i < videos.Count; i++)
                {
                    videos[i].LocalID = i + 1;
                    videos[i].VideoLinkUri = new Uri(videos[i].VideoLinkUrl);
                }

                var videoDetails = GetSpeedRunVideoDetails(videos, false);
                var videosToUpdate = new List<SpeedRunVideoEntity>();
                foreach (var videoDetail in videoDetails)
                {
                    if (!string.IsNullOrWhiteSpace(videoDetail.ThumbnailLinkUrl))
                    {
                        var video = videos.Find(g => g.LocalID == videoDetail.SpeedRunVideoLocalID);

                        if (string.IsNullOrWhiteSpace(video.ThumbnailLinkUrl) || (video.ThumbnailLinkUrl.EndsWith("hqdefault.jpg") && videoDetail.ThumbnailLinkUrl.EndsWith("maxresdefault.jpg")))
                        {
                            video.ThumbnailLinkUrl = videoDetail.ThumbnailLinkUrl;
                            videosToUpdate.Add(video);
                        }
                    }
                }

                _speedRunRepo.UpdateSpeedRunVideoThumbnailLinkUrls(videosToUpdate);
                _speedRunRepo.InsertSpeedRunVideoDetails(videoDetails);

                _logger.Information("Completed ProcessYouTubeSpeedRunVideoDetails");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "ProcessYouTubeSpeedRunVideoDetails");
            }

            return result;
        }

        private List<SpeedRunVideoDetailEntity> GetSpeedRunVideoDetails(List<SpeedRunVideoEntity> videos, bool isBulkReload)
        {
            var details = new List<SpeedRunVideoDetailEntity>();
            var batchCount = 0;
            _logger.Information("Started GetSpeedRunVideoDetails");

            if (TwitchAPIEnabled)
            {
                var twitchToken = _settingService.GetTwitchToken();
                var twitchIdentifiers = new List<string> { "twitch.tv" };
                var twitchVideos = videos.Where(i => i.VideoLinkUri != null && twitchIdentifiers.Any(g => i.VideoLinkUri.GetLeftPart(UriPartial.Authority).Contains(g)) && i.VideoLinkUri.AbsolutePath.StartsWith(@"/videos/")).ToList();

                foreach (var twitchVideo in twitchVideos)
                {
                    twitchVideo.VideoID = twitchVideo.VideoLinkUri.Segments.Last();
                }
                twitchVideos = twitchVideos.Where(i => !string.IsNullOrWhiteSpace(i.VideoID)).ToList();

                while (batchCount < twitchVideos.Count)
                {
                    var videosBatch = twitchVideos.Skip(batchCount).Take(TwitchAPIMaxBatchCount).ToList();
                    var videoIDsBatch = videosBatch.Select(i => i.VideoID).ToList();
                    var videoIDsString = string.Join("&id=", videoIDsBatch);
                    var requestString = string.Format(@"https://api.twitch.tv/helix/videos?id={0}", videoIDsString);
                    var parameters = new Dictionary<string, string>() { { "Client-Id", TwitchClientID }, { "Authorization", "Bearer " + twitchToken } };
                    dynamic results = null;
                    try
                    {
                        results = JsonHelper.FromUri(new Uri(requestString), parameters)?.data;
                    }
                    catch (Exception ex)
                    {
                        _logger.Information(ex, "GetSpeedRunVideoDetails");
                    }

                    foreach (var video in videosBatch)
                    {
                        dynamic result = null;
                        if (results != null)
                        {
                            foreach (var res in results)
                            {
                                if (res.id == video.VideoID)
                                {
                                    result = res;
                                    break;
                                }
                            }
                        }

                        if (result != null)
                        {
                            var thumbnailUriString = (string)result.thumbnail_url;
                            var thumbnailLinkUrl = thumbnailUriString?.Replace("%{width}", "1280").Replace("%{height}", "720");
                            details.Add(new SpeedRunVideoDetailEntity() { SpeedRunVideoLocalID = video.LocalID, SpeedRunVideoID = video.ID, SpeedRunID = video.SpeedRunID, ChannelCode = (string)result.user_id, ViewCount = (long?)result.view_count, ThumbnailLinkUrl = thumbnailLinkUrl });
                        }
                    }

                    batchCount += TwitchAPIMaxBatchCount;
                    _logger.Information("Set Twitch Video Details {@Count} / {@Total}", (batchCount > twitchVideos.Count ? twitchVideos.Count : batchCount), twitchVideos.Count);
                }
            }

            if (YouTubeAPIEnabled && !isBulkReload)
            {
                batchCount = 0;
                var youtubeIdentifiers = new List<string> { "youtube.com", "youtu.be" };
                var youtubeVideos = videos.Where(i => i.VideoLinkUri != null && youtubeIdentifiers.Any(g => i.VideoLinkUri.GetLeftPart(UriPartial.Authority).Contains(g))).ToList();

                foreach (var youtubeVideo in youtubeVideos)
                {
                    var queryDictionary = QueryHelpers.ParseQuery(youtubeVideo.VideoLinkUri.Query);
                    youtubeVideo.VideoID = queryDictionary.ContainsKey("v") ? queryDictionary["v"].ToString() : youtubeVideo.VideoLinkUri.Segments.Last();
                }
                youtubeVideos = youtubeVideos.Where(i => !string.IsNullOrWhiteSpace(i.VideoID)).ToList();

                while (batchCount < youtubeVideos.Count && YouTubeAPIRequestCount < YouTubeAPIDailyRequestLimit)
                {
                    var videosBatch = youtubeVideos.Skip(batchCount).Take(YouTubeAPIMaxBatchCount).ToList();
                    var videoIDsBatch = videosBatch.Select(i => i.VideoID).ToList();
                    var videoIDsString = string.Join(",", videoIDsBatch);
                    var requestString = string.Format(@"https://www.googleapis.com/youtube/v3/videos?part=statistics,snippet&id={0}&key={1}", videoIDsString, YouTubeAPIKey);
                    dynamic results = null;
                    try
                    {
                        results = JsonHelper.FromUri(new Uri(requestString))?.items;
                    }
                    catch (Exception ex)
                    {
                        _logger.Information(ex, "GetSpeedRunVideoDetails");
                        if (ex.Message.Contains("exceeded"))
                        {
                            break;
                        }
                    }

                    foreach (var video in videosBatch)
                    {
                        dynamic result = null;
                        if (results != null)
                        {
                            foreach (var res in results)
                            {
                                if (res.id == video.VideoID)
                                {
                                    result = res;
                                    break;
                                }
                            }
                        }

                        if (result != null)
                        {
                            var thumbnailLinkUrl = (string)(result.snippet?.thumbnails?.maxres?.url ?? result.snippet?.thumbnails?.high?.url);
                            details.Add(new SpeedRunVideoDetailEntity() { SpeedRunVideoLocalID = video.LocalID, SpeedRunVideoID = video.ID, SpeedRunID = video.SpeedRunID, ChannelCode = (string)result.snippet?.channelId, ViewCount = (long?)result.statistics?.viewCount, ThumbnailLinkUrl = thumbnailLinkUrl });
                        }

                        break;
                    }

                    batchCount += YouTubeAPIMaxBatchCount;
                    YouTubeAPIRequestCount += 1;
                    _logger.Information("Set Youtube Video Details {@Count} / {@Total}", (batchCount > youtubeVideos.Count ? youtubeVideos.Count : batchCount), youtubeVideos.Count);
                }
            }

            _logger.Information("Completed GetSpeedRunVideoDetails");
            return details;
        }
    }
}


