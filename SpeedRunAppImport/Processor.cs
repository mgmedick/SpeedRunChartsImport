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
using SpeedRunApp.Model;
using SpeedRunApp.Model.Data;
using SpeedRunAppImport.Service;
using SpeedRunApp.Model.Entity;

namespace SpeedRunAppImport
{
    public class Processor : IProcessor
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly ISpeedRunService _speedRunService;
        private readonly IGameRepository _gameRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISpeedRunRepository _speedRunRepo;
        private readonly ISettingService _settingService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Processor(IGameService gameService, IUserService userService, ISpeedRunService speedRunService, IGameRepository gameRepo, IUserRepository userRepo, ISpeedRunRepository speedRunRepo, ISettingService settingService, IConfiguration config, ILogger logger)
        {
            _gameService = gameService;
            _userService = userService;
            _speedRunService = speedRunService;
            _gameRepo = gameRepo;
            _userRepo = userRepo;
            _speedRunRepo = speedRunRepo;
            _settingService = settingService;
            _config = config;
            _logger = logger;
        }

        public void Init()
        {
            try
            {
                var connString = _config.GetSection("ConnectionStrings").GetSection("DBConnectionString").Value;
                var maxBulkRows = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
                IsFullImport = _config.GetValue<bool>("IsFullImport");
                NPocoBootstrapper.Configure(connString, maxBulkRows, IsFullImport);

                if (IsFullImport)
                {
                    GameLastImportDate = DateTime.MinValue;
                    UserLastImportDate = DateTime.MinValue;
                    PlatformLastImportDate = DateTime.MinValue;
                    SpeedRunLastImportDate = DateTime.MinValue;
                }
                else
                {
                    GameLastImportDate = _settingService.GetSetting("GameLastImportDate")?.Dte ?? DateTime.MinValue;
                    UserLastImportDate = _settingService.GetSetting("UserLastImportDate")?.Dte ?? DateTime.MinValue;
                    PlatformLastImportDate = _settingService.GetSetting("PlatformLastImportDate")?.Dte ?? DateTime.MinValue;
                    SpeedRunLastImportDate = _settingService.GetSetting("SpeedRunLastImportDate")?.Dte ?? DateTime.MinValue;
                }

                BaseService.MaxElementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
                BaseService.MaxRetryCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxRetryCount").Value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Init");
            }
        }

        public void RunProcesses()
        {
            Init();
            //ProcessGames();
            //ProcessUsers();
            ProcessSpeedRuns();
        }

        public void ProcessGames()
        {
            try
            {
                var newImportDate = DateTime.UtcNow;
                var games = _gameService.GetGames(GameLastImportDate, IsFullImport);
                var gameEntities = games.Select(i => i.ConvertToEntity()).ToList();
                var levelEntities = games.SelectMany(i => i.Levels.Select(i => i.ConvertToEntity())).ToList();
                var categoryEntities = games.SelectMany(i => i.Categories.Select(i => i.ConvertToEntity())).ToList();
                var variableEntities = games.SelectMany(i => i.Variables.Select(i => i.ConvertToEntity())).ToList();
                var variableValueEntities = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => h.ConvertToEntity()))).ToList();
                var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity { GameID = i.ID, PlatformID = g })).ToList();
                var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity { GameID = i.ID, RegionID = g })).ToList();
                var gameModeratorEntities = games.SelectMany(i => i.Moderators.Select(g => new GameModeratorEntity { GameID = i.ID, UserID = g.UserID })).ToList();

                if (IsFullImport)
                {
                    if (gameEntities.Any())
                    {
                        _gameRepo.CopyGameTables();
                        _gameRepo.InsertGames(gameEntities, levelEntities, categoryEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities);
                        _gameRepo.RenameAndDropGameTables();
                    }
                }
                else
                {
                    if (gameEntities.Any())
                    {
                        _gameRepo.InsertGames(gameEntities, levelEntities, categoryEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities);
                    }
                }

                var gameSetting = _settingService.GetSetting("GameLastImportDate");
                gameSetting.Dte = newImportDate;
                _settingService.UpdateSetting(gameSetting);
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
                var newImportDate = DateTime.UtcNow;
                var runs = _speedRunService.GetSpeedRuns(SpeedRunLastImportDate, IsFullImport);
                var runEntities = runs.Select(i => i.ConvertToEntity()).ToList();
                var variableValueEntities = runs.SelectMany(i => i.VariableValueMappings.Select(g => new SpeedRunVariableValueEntity() { SpeedRunID = i.ID, VariableID = g.VariableID, VariableValueID = g.VariableValueID })).ToList();
                var playerEntities = runs.SelectMany(i => i.Players.Select(g => new SpeedRunPlayerEntity() { SpeedRunID = i.ID, IsUser = g.IsUser, UserID = g.UserID, GuestName = g.GuestName } )).ToList();
                var videoEntities = runs.Where(i => i.Videos?.Links != null && i.Videos.Links.Any(g => g != null))
                                        .SelectMany(i => i.Videos?.Links?.Select((g, n) => new SpeedRunVideoEntity() { SpeedRunID = i.ID, VideoLinkUrl = g?.ToString(), VideoLinkEmbededUrl = i.Videos?.EmbededLinks?.ToArray()[n]?.ToString() }))
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
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessSpeedRuns");
            }
        }

        public DateTime GameLastImportDate { get; set; }
        public DateTime UserLastImportDate { get; set; }
        public DateTime PlatformLastImportDate { get; set; }
        public DateTime SpeedRunLastImportDate { get; set; }
        public bool IsFullImport { get; set; }
    }
}
