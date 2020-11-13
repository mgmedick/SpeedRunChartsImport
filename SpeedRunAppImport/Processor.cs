using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole;
using Serilog.Sinks.Email;
using System.Threading.Tasks;
using SpeedRunAppImport.Interfaces.Repositories;
using SpeedRunAppImport.Interfaces;
using SpeedRunAppImport.Repository.Configuration;
using System.Threading;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Service;
using SpeedRunAppImport.Model.Entity;
using System.Data.SqlTypes;

namespace SpeedRunAppImport
{
    public class Processor : IProcessor
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly ISpeedRunService _speedRunService;
        private readonly IPlatformService _platformService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly IGameRepository _gameRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISpeedRunRepository _speedRunRepo;
        private readonly IPlatformRepository _platformRepo;
        private readonly ILeaderboardRepository _leaderboardRepo;
        private readonly ISettingService _settingService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Processor(IGameService gameService, IUserService userService, ISpeedRunService speedRunService, IPlatformService platformService, ILeaderboardService leaderboardService, IGameRepository gameRepo, IUserRepository userRepo, ISpeedRunRepository speedRunRepo, IPlatformRepository platformRepo, ILeaderboardRepository leaderboardRepo, ISettingService settingService, IConfiguration config, ILogger logger)
        {
            _gameService = gameService;
            _userService = userService;
            _speedRunService = speedRunService;
            _platformService = platformService;
            _leaderboardService = leaderboardService;
            _gameRepo = gameRepo;
            _userRepo = userRepo;
            _speedRunRepo = speedRunRepo;
            _platformRepo = platformRepo;
            _leaderboardRepo = leaderboardRepo;
            _settingService = settingService;
            _config = config;
            _logger = logger;
        }

        public void Run()
        {
            try
            {
                Init();
                if (IsImportRunning)
                {
                    _logger.Information("Import already running");
                }
                else
                {
                    var importRunningSetting = _settingService.GetSetting("IsImportRunning");
                    importRunningSetting.Num = 1;
                    _settingService.UpdateSetting(importRunningSetting);

                    RunProcesses();

                    importRunningSetting.Num = 0;
                    _settingService.UpdateSetting(importRunningSetting);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Run");
            }
        }

        public void Init()
        {
            try
            {
                _logger.Information("Started Init");
                var connString = _config.GetSection("ConnectionStrings").GetSection("DBConnectionString").Value;
                var maxBulkRows = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxBulkRows").Value);
                IsFullImport = _config.GetValue<bool>("IsFullImport");
                NPocoBootstrapper.Configure(connString, maxBulkRows, IsFullImport);

                IsImportRunning = _settingService.GetSetting("IsImportRunning")?.Num == 1;

                if (IsFullImport)
                {
                    GameLastImportDate = (DateTime)SqlDateTime.MinValue;
                    UserLastImportDate = (DateTime)SqlDateTime.MinValue;
                    PlatformLastImportDate = (DateTime)SqlDateTime.MinValue;
                    SpeedRunLastImportDate = (DateTime)SqlDateTime.MinValue;
                }
                else
                {
                    GameLastImportDate = _settingService.GetSetting("GameLastImportDate")?.Dte ?? DateTime.UtcNow;
                    UserLastImportDate = _settingService.GetSetting("UserLastImportDate")?.Dte ?? DateTime.UtcNow;
                    PlatformLastImportDate = _settingService.GetSetting("PlatformLastImportDate")?.Dte ?? DateTime.UtcNow;
                    SpeedRunLastImportDate = _settingService.GetSetting("SpeedRunLastImportDate")?.Dte ?? DateTime.UtcNow;
                }

                BaseService.MaxElementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
                BaseService.MaxRetryCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxRetryCount").Value);
                BaseService.PullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("PullDelayMS").Value);
                BaseService.ErrorPullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("ErrorPullDelayMS").Value);
                _logger.Information("Completed Init");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Init");
            }
        }

        public void RunProcesses()
        {
            ProcessPlatforms();
            ProcessGames();
            ProcessUsers();
            ProcessSpeedRuns();
            ProcessLeaderboards();
        }

        public void ProcessPlatforms()
        {
            try
            {
                _logger.Information("Started ProcessPlatforms");
                var newImportDate = DateTime.UtcNow;
                var platforms = _platformService.GetAllPlatforms();
                var platformIDs = _platformRepo.GetAllPlatformIDs().ToList();
                var platformEntities = platforms.Where(i => !platformIDs.Contains(i.ID)).Select(i => i.ConvertToEntity()).ToList();

                if (platformEntities.Any())
                {
                    _platformRepo.CopyPlatformTables();
                    _platformRepo.InsertPlatforms(platformEntities);
                    _platformRepo.RenameAndDropPlatformTables();
                }

                var platformSetting = _settingService.GetSetting("PlatformLastImportDate");
                platformSetting.Dte = newImportDate;
                _settingService.UpdateSetting(platformSetting);
                _logger.Information("Completed ProcessPlatforms");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessPlatforms");
            }
        }

        public void ProcessGames()
        {
            try
            {
                _logger.Information("Started ProcessGames");
                var newImportDate = DateTime.UtcNow;
                var games = _gameService.GetGames(GameLastImportDate, IsFullImport).OrderBy(i => i.YearOfRelease);
                var gameEntities = games.Select(i => i.ConvertToEntity()).ToList();
                var levelEntities = games.SelectMany(i => i.Levels.Select(i => i.ConvertToEntity())).ToList();
                var categoryEntities = games.SelectMany(i => i.Categories.Select(i => i.ConvertToEntity())).ToList();
                var variableEntities = games.SelectMany(i => i.Variables.Select(i => i.ConvertToEntity())).ToList();
                var variableValueEntities = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => h.ConvertToEntity()))).ToList();
                var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity { GameID = i.ID, PlatformID = g })).ToList();
                var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity { GameID = i.ID, RegionID = g })).ToList();
                var gameModeratorEntities = games.SelectMany(i => i.Moderators.Select(g => new GameModeratorEntity { GameID = i.ID, UserID = g.UserID })).ToList();
                var gameRulesetEntities = games.Select(i => new GameRulesetEntity { GameID = i.ID, ShowMilliseconds = i.Ruleset.ShowMilliseconds, RequiresVerification = i.Ruleset.RequiresVerification, RequiresVideo = i.Ruleset.RequiresVideo, DefaultTimingMethodID = (int)i.Ruleset.DefaultTimingMethod, EmulatorsAllowed = i.Ruleset.EmulatorsAllowed }).ToList();
                var gameTimingMethodEntities = games.SelectMany(i => i.Ruleset.TimingMethods.Select(g => new GameTimingMethodEntity { GameID = i.ID, TimingMethodID = (int)g })).ToList();

                if (IsFullImport)
                {
                    if (gameEntities.Any())
                    {
                        _gameRepo.CopyGameTables();
                        _gameRepo.InsertGames(gameEntities, levelEntities, categoryEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities);
                        _gameRepo.RenameAndDropGameTables();
                    }
                }
                else
                {
                    if (gameEntities.Any())
                    {
                        _gameRepo.InsertGames(gameEntities, levelEntities, categoryEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities);
                    }
                }

                var gameSetting = _settingService.GetSetting("GameLastImportDate");
                gameSetting.Dte = newImportDate;
                _settingService.UpdateSetting(gameSetting);
                _logger.Information("Completed ProcessGames");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessGames");
            }
        }

        public void ProcessUsers()
        {
            try
            {
                _logger.Information("Started ProcessUsers");
                var newImportDate = DateTime.UtcNow;
                var users = _userService.GetUsers(UserLastImportDate, IsFullImport);
                var userEntities = users.Select(i => i.ConvertToEntity()).ToList();

                if (IsFullImport)
                {
                    if (userEntities.Any())
                    {
                        _userRepo.CopyUserTables();
                        _userRepo.InsertUsers(userEntities);
                        _userRepo.RenameAndDropUserTables();
                    }
                }
                else
                {
                    if (userEntities.Any())
                    {
                        _userRepo.InsertUsers(userEntities);
                    }
                }

                var userSetting = _settingService.GetSetting("UserLastImportDate");
                userSetting.Dte = newImportDate;
                _settingService.UpdateSetting(userSetting);
                _logger.Information("Completed ProcessUsers");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessUsers");
            }
        }

        public void ProcessSpeedRuns()
        {
            try
            {
                _logger.Information("Started ProcessSpeedRuns");
                var newImportDate = DateTime.UtcNow;
                var runs = _speedRunService.GetSpeedRuns(SpeedRunLastImportDate, IsFullImport);
                var runEntities = runs.Select(i => i.ConvertToEntity()).ToList();
                var variableValueEntities = runs.SelectMany(i => i.VariableValueMappings.Select(g => new SpeedRunVariableValueEntity() { SpeedRunID = i.ID, VariableID = g.VariableID, VariableValueID = g.VariableValueID })).ToList();
                var playerEntities = runs.SelectMany(i => i.Players.Select(g => new SpeedRunPlayerEntity() { SpeedRunID = i.ID, IsUser = g.IsUser, UserID = g.UserID, GuestName = g.GuestName } )).ToList();
                var videoEntities = runs.Where(i => i.Videos?.Links != null && i.Videos.Links.Any(g => g != null))
                                        .SelectMany(i => i.Videos?.Links?.Select((g, n) => new SpeedRunVideoEntity() { SpeedRunID = i.ID, VideoLinkUrl = g?.ToString(), VideoLinkEmbededUrl = i.Videos?.EmbededLinks?.ToArray()[n]?.ToString() }))
                                        .Where(i => !string.IsNullOrWhiteSpace(i.VideoLinkUrl))
                                        .ToList();

                if (IsFullImport)
                {
                    if (runEntities.Any())
                    {
                        _speedRunRepo.CopySpeedRunTables();
                        _speedRunRepo.InsertSpeedRuns(runEntities, variableValueEntities, playerEntities, videoEntities);
                        _speedRunRepo.RenameAndDropSpeedRunTables();
                    }
                }
                else
                {
                    if (runEntities.Any())
                    {
                        _speedRunRepo.InsertSpeedRuns(runEntities, variableValueEntities, playerEntities, videoEntities);
                    }

                    var verifiedRuns = _speedRunService.GetSpeedRuns(SpeedRunLastImportDate, IsFullImport, RunStatusType.Verified);
                    var verifiedRunEntities = verifiedRuns.Select(i => i.ConvertToEntity(DateTime.UtcNow));
                    var rejectedRuns = _speedRunService.GetSpeedRuns(SpeedRunLastImportDate.AddDays(-2), IsFullImport, RunStatusType.Rejected);
                    var rejectedRunsEntities = rejectedRuns.Select(i => i.ConvertToEntity(DateTime.UtcNow));

                    if (verifiedRunEntities.Any())
                    {
                        _speedRunRepo.UpdateSpeedRunStatus(verifiedRunEntities, RunStatusType.Verified);
                    }

                    if (rejectedRunsEntities.Any())
                    {
                        _speedRunRepo.UpdateSpeedRunStatusAndRejectReason(rejectedRunsEntities);
                    }
                }

                var speedRunSetting = _settingService.GetSetting("SpeedRunLastImportDate");
                speedRunSetting.Dte = newImportDate;
                _settingService.UpdateSetting(speedRunSetting);
                _logger.Information("Completed ProcessSpeedRuns");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessSpeedRuns");
            }
        }

        private void ProcessLeaderboards()
        {
            try
            {
                _logger.Information("Started ProcessLeaderboards");
                var newImportDate = DateTime.UtcNow;
                var leaderboardKeys = _leaderboardRepo.GetLeaderboardKeys(SpeedRunLastImportDate, (int)RunStatusType.Verified);
                var leaderboards = _leaderboardService.GetLeaderboards(leaderboardKeys);
                var leaderboardEntities = leaderboards.SelectMany(i => i.Records.Select(g => new LeaderboardEntity { GameID = i.GameID, CategoryID = i.CategoryID, LevelID = i.LevelID, Rank = g.Rank, SpeedRunID = g.ID }));

                if (IsFullImport)
                {
                    if (leaderboardEntities.Any())
                    {
                        _leaderboardRepo.CopyLeaderboardTables();
                        _leaderboardRepo.InsertLeaderboards(leaderboardEntities);
                        _leaderboardRepo.RenameAndDropLeaderboardTables();
                    }
                }
                else
                {
                    if (leaderboardEntities.Any())
                    {
                        _leaderboardRepo.UpdateLeaderboards(leaderboardEntities);
                    }
                }

                var leaderboardSetting = _settingService.GetSetting("LeaderboardLastImportDate");
                leaderboardSetting.Dte = newImportDate;
                _settingService.UpdateSetting(leaderboardSetting);
                _logger.Information("Completed ProcessLeaderboards");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessLeaderboards");
            }
        }

        public DateTime GameLastImportDate { get; set; }
        public DateTime UserLastImportDate { get; set; }
        public DateTime PlatformLastImportDate { get; set; }
        public DateTime SpeedRunLastImportDate { get; set; }
        public bool IsFullImport { get; set; }
        public bool IsImportRunning { get; set; }
    }
}
