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
using SpeedRunAppImport.Service;
using SpeedRunApp.Model.Entity;

namespace SpeedRunAppImport
{
    public class Processor : IProcessor
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IGameRepository _gameRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISettingService _settingService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Processor(IGameService gameService, IUserService userService, IGameRepository gameRepo, IUserRepository userRepo, ISettingService settingService, IConfiguration config, ILogger logger)
        {
            _gameService = gameService;
            _userService = userService;
            _gameRepo = gameRepo;
            _userRepo = userRepo;
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
                var isFullImport = _config.GetValue<bool>("IsFullImport");
                NPocoBootstrapper.Configure(connString, maxBulkRows, isFullImport);

                BaseService.GameLastImportDate = _settingService.GetSetting("GameLastImportDate")?.Dte ?? DateTime.MinValue;
                BaseService.UserLastImportDate = _settingService.GetSetting("UserLastImportDate")?.Dte ?? DateTime.MinValue;
                BaseService.PlatformLastImportDate = _settingService.GetSetting("PlatformLastImportDate")?.Dte ?? DateTime.MinValue;
                BaseService.SpeedRunLastImportDate = _settingService.GetSetting("SpeedRunLastImportDate")?.Dte ?? DateTime.MinValue;
                BaseService.MaxElementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
                BaseService.MaxRetryCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxRetryCount").Value);
                BaseService.IsFullImport = isFullImport;
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
            ProcessUsers();
        }

        public void ProcessGames()
        {
            try
            {
                var games = _gameService.GetGames();
                var gameLastImportDate = DateTime.UtcNow;
                if (games.Any())
                {
                    games = games.OrderBy(i => (i.CreationDate ?? DateTime.MinValue)).ThenBy(i => i.Name);
                    var gameEntities = games.Select(i => i.ConvertToEntity());
                    var levelEntities = games.SelectMany(i => i.Levels.Select(i => i.ConvertToEntity()));
                    var categoryEntities = games.SelectMany(i => i.Categories.Select(i => i.ConvertToEntity()));
                    var variableEntities = games.SelectMany(i => i.Variables.Select(i => i.ConvertToEntity()));
                    var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity { GameID = i.ID, PlatformID = g }));
                    var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity { GameID = i.ID, RegionID = g }));

                    if (BaseService.IsFullImport)
                    {
                        _gameRepo.CopyGameDetailTables();
                    }

                    _gameRepo.InsertGameDetails(gameEntities, levelEntities, categoryEntities, variableEntities, gamePlatformEntities, gameRegionEntities);

                    if (BaseService.IsFullImport)
                    {
                        _gameRepo.RenameAndDropGameDetailTables();
                    }
                }

                var gameSetting = _settingService.GetSetting("GameLastImportDate");
                gameSetting.Dte = gameLastImportDate;
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
                var users = _userService.GetUsers();
                var userLastImportDate = DateTime.UtcNow;
                if (users.Any())
                {
                    users = users.OrderBy(i => (i.SignUpDate ?? DateTime.MinValue)).ThenBy(i => i.Name);
                    var userEntities = users.Select(i => i.ConvertToEntity());

                    if (BaseService.IsFullImport)
                    {
                        _userRepo.CopyUserTables();
                    }

                    _userRepo.InsertUsers(userEntities);

                    if (BaseService.IsFullImport)
                    {
                        _userRepo.RenameAndDropUserTables();
                    }
                }

                var userSetting = _settingService.GetSetting("UserLastImportDate");
                userSetting.Dte = userLastImportDate;
                _settingService.UpdateSetting(userSetting);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessUsers");
            }
        }
    } 
}
