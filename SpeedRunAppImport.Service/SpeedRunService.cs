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
            var prevTotal = 0;

            do
            {
                runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count() + prevTotal, null, null, runEmbeds, orderBy, RunStatusType.Verified);
                results.AddRange(runs);
                _logger.Information("Pulled runs: {@New}, total runs: {@Total}", runs.Count, results.Count() + prevTotal);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                var memorySize = GC.GetTotalMemory(false);
                if (memorySize > MaxMemorySizeBytes)
                {
                    prevTotal += results.Count;
                    _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                    SaveSpeedRuns(results, isBulkReload);
                    results.ClearMemory();
                }
            }
            while (runs.Count == MaxElementsPerPage && (isFullPull || runs.Min(i => i.Status.VerifyDate ?? SqlMinDateTime) >= lastImportDateUtc));

            if (!isFullPull)
            {
                results.RemoveAll(i => (i.Status.VerifyDate ?? SqlMinDateTime) <= lastImportDateUtc);
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, isBulkReload);
                var lastUpdateDate = results.Max(i => i.Status.VerifyDate) ?? DateTime.UtcNow;
                _settingService.UpdateSetting("SpeedRunLastImportDate", lastUpdateDate);
                results.ClearMemory();
            }
        }

        #region ProcessSpeedRunsByGame
        public void ProcessSpeedRunsByGame(DateTime importLastRunDateUtc, bool isFullPull, bool isBulkReload)
        {
            RunsOrdering orderBy = RunsOrdering.DateSubmitted;
            var runEmbeds = new SpeedRunEmbeds { EmbedCategory = false, EmbedGame = false, EmbedLevel = false, EmbedPlayers = true, EmbedPlatform = false, EmbedRegion = false };
            var results = new List<SpeedRun>();
            var runs = new List<SpeedRun>();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
            var gameIDs = isFullPull ? _gameRepo.GetGames().Select(i => i.ID).ToList() : _gameRepo.GetGames(i => (i.ModifiedDate ?? i.CreatedDate) >= importLastRunDateUtc).Select(i => i.ID).ToList();
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
                            if (!isBulkReload)
                            {
                                DeleteGameSpeedRunsAndProcessByCategory(gameSpeedRunComID.GameID, gameSpeedRunComID.SpeedRunComID, runEmbeds, orderBy, isBulkReload, results, ref prevTotal);
                            }
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
                        SaveSpeedRuns(results, isBulkReload);
                        results.ClearMemory();
                    }
                }
                while (runs.Count == MaxElementsPerPage);
            }

            if (results.Any())
            {
                SaveSpeedRuns(results, isBulkReload);
                results.ClearMemory();
                DeleteObsoleteSpeedRuns(gameIDs, importLastRunDateUtc);
            }
        }

        private void DeleteObsoleteSpeedRuns(List<int> gameIDs, DateTime importLastRunDateUtc)
        {
            _logger.Information("Started DeleteObsoleteSpeedRuns: GameCount: {@GameCount}, LastImportDateUTC: {@ImportLastRunDateUtc}", gameIDs.Count(), importLastRunDateUtc);

            foreach (var gameID in gameIDs)
            {
                _logger.Information("Deleting obsolete speedruns for GameID: {@GameID}", gameID);
                _speedRunRepo.DeleteSpeedRuns(i => i.GameID == gameID && ((i.ModifiedDate.HasValue && i.ModifiedDate < importLastRunDateUtc) || (i.ImportedDate < importLastRunDateUtc)));
                _logger.Information("Completed deleting obsolete speedruns for GameID: {@GameID}", gameID);
            }

            _logger.Information("Completed DeleteObsoleteSpeedRuns: GameCount: {@GameCount}, LastImportDateUTC: {@ImportLastRunDateUtc}", gameIDs.Count(), importLastRunDateUtc);
        }

        private void DeleteGameSpeedRunsAndProcessByCategory(int gameID, string gameSpeedRunComID, SpeedRunEmbeds runEmbeds, RunsOrdering orderBy, bool isBulkReload, List<SpeedRun> results, ref int prevTotal)
        {
            results.RemoveAll(i => i.GameID == gameSpeedRunComID);
            if (isBulkReload)
            {
                _speedRunRepo.DeleteSpeedRuns(i => i.GameID == gameID);
            }

            var categorySpeedRunComIDs = _gameRepo.GetGameSpeedRunComViews(i => i.SpeedRunComID == gameSpeedRunComID)
                                               .SelectMany(i => i.CategorySpeedRunComIDArray)
                                               .ToList();
            ProcessSpeedRunsByCategory(gameSpeedRunComID, categorySpeedRunComIDs, runEmbeds, orderBy, isBulkReload, results, ref prevTotal);
        }

        private void ProcessSpeedRunsByCategory(string gameSpeedRunComID, IEnumerable<string> categorySpeedRunComIDs, SpeedRunEmbeds runEmbeds, RunsOrdering orderBy, bool isBulkReload, List<SpeedRun> results, ref int prevTotal)
        {
            var runs = new List<SpeedRun>();

            foreach (var categorySpeedRunComID in categorySpeedRunComIDs)
            {
                var prevCategoryTotal = 0;
                do
                {
                    try
                    { 
                        runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count(i => i.GameID == gameSpeedRunComID && i.CategoryID == categorySpeedRunComID) + prevCategoryTotal, gameSpeedRunComID, categorySpeedRunComID, runEmbeds, orderBy, RunStatusType.Verified);
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
                    _logger.Information("GameID: {@GameID}, CategoryID: {@CategoryID}, pulled runs: {@New}, game total: {@CategoryTotal}, total runs: {@Total}",
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
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveSpeedRuns(results, isBulkReload);
                        results.ClearMemory();
                    }
                }
                while (runs.Count == MaxElementsPerPage);
            }
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
                var lastUpdateDate = results.Max(i => i.Status.VerifyDate) ?? DateTime.UtcNow;
                _settingService.UpdateSetting("SpeedRunLastImportDate", lastUpdateDate);
                results.ClearMemory();
            }

        }
        #endregion

        public List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, string gameID, string categoryID, SpeedRunEmbeds embeds, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<SpeedRun> runs = null;

            try
            {
                runs = clientContainer.Runs.GetRuns(status: statusType, elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, gameId: gameID, categoryId: categoryID, embeds: embeds, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (ex is APIException && ((APIException)ex).Message.Contains("Invalid pagination values"))
                {
                    _logger.Information(ex, "GetSpeedRunsWithRetry - Invalid pagination values - GameID: {gameID}", gameID);
                    throw ex;
                }
                else if (ex is APIException && ((APIException)ex).Message.Contains("Non-existing"))
                {
                    runs = new List<SpeedRun>();
                    _logger.Information(ex, "GetSpeedRunsWithRetry - Non-existing - GameID: {gameID}, CategoryID: {categoryID}", gameID, categoryID);
                }
                else if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull speedRuns: {@New}, total speedRuns: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    runs = GetSpeedRunsWithRetry(elementsPerPage, elementsOffset, gameID, categoryID, embeds, orderBy, statusType, retryCount);
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

        public void UpdateSpeedRunVideos()
        {
            var speedRunVideos = _speedRunRepo.GetSpeedRunVideos(i => i.EmbeddedVideoLinkUrl == null);
            foreach (var speedRunVideo in speedRunVideos)
            {
                speedRunVideo.EmbeddedVideoLinkUrl = new Uri(speedRunVideo.VideoLinkUrl).ToEmbeddedURIString();
            }

            _speedRunRepo.UpdateSpeedRunVideos(speedRunVideos);
        }

        public void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isBulkReload)
        {
            _logger.Information("Started SaveSpeedRuns: {@Count}, {@IsBulkReload}", runs.Count(), isBulkReload);

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
                            .SelectMany(i => i.PlayerUsers.Where(i => !userSpeedRunComIDs.Any(g => g.SpeedRunComID == i.ID)))
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
                //ExaminerUserID = userSpeedRunComIDs.Where(g => g.SpeedRunComID == i.Status.ExaminerUserID).Select(g => (int?)g.UserID).FirstOrDefault(),
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
                VideoLinkUrl = g?.ToString(),
                EmbeddedVideoLinkUrl = g?.ToEmbeddedURIString()
            })).Where(i => !string.IsNullOrWhiteSpace(i.VideoLinkUrl))
            .GroupBy(h => new { h.SpeedRunSpeedRunComID, h.VideoLinkUrl })
            .Select(n => n.First())
            .ToList();
            var guestPlayerEntities = runs.Where(i => i.PlayerGuests != null).SelectMany(i => i.PlayerGuests.Select(g => new SpeedRunGuestEntity()
            {
                SpeedRunSpeedRunComID = i.ID,
                GuestID = guestSpeedRunComIDs.Where(h => h.Name == g.Name).Select(h => h.ID).FirstOrDefault()
            })).Where(i => i.GuestID != 0)
            .ToList();

            if (isBulkReload)
            {
                _speedRunRepo.InsertSpeedRuns(runEntities, runLinkEntities, runSystemEntities, runTimeEntities, runCommentEntities, variableValueEntities, playerEntities, guestPlayerEntities, videoEntities);
            }
            else
            {
                _speedRunRepo.SaveSpeedRuns(runEntities, runLinkEntities, runSystemEntities, runTimeEntities, runCommentEntities, variableValueEntities, playerEntities, guestPlayerEntities, videoEntities);
            }

            _logger.Information("Completed SaveSpeedRuns");
        }
    }
}


