using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Interfaces.Services;
using Microsoft.Extensions.Configuration;
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
        private readonly ISpeedRunRepository _speedRunRepo;
        private readonly ISettingService _settingService;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Processor(IGameService gameService, IUserService userService, ISpeedRunService speedRunService, IPlatformService platformService, ISpeedRunRepository speedRunRepo, ISettingService settingService, IConfiguration config, ILogger logger)
        {
            _gameService = gameService;
            _userService = userService;
            _speedRunService = speedRunService;
            _platformService = platformService;
            _speedRunRepo = speedRunRepo;
            _settingService = settingService;
            _config = config;
            _logger = logger;
        }

        public void Run()
        {
            try
            {
                Init();
                RunProcesses();
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
                IsBulkReload = _config.GetValue<bool>("IsBulkReload");
                IsFullImport = _config.GetValue<bool>("IsFullImport");
                IsProcessSpeedRunsByGame = Convert.ToBoolean(_config.GetSection("ApiSettings").GetSection("IsProcessSpeedRunsByGame").Value);
                NPocoBootstrapper.Configure(connString, maxBulkRows, IsBulkReload);

                Processes = _config.GetValue<string>("ProcessIDs").Split(",").Select(i => (ImportProcess)Convert.ToInt32(i)).ToList();
                if (IsBulkReload)
                {
                    //Processes.Add(ImportProcess.All);
                    IsFullImport = true;
                }

                if (Processes.Contains(ImportProcess.All))
                {
                    Processes.RemoveAll(i => i != ImportProcess.All);
                }

                var sqlMinDateTime = (DateTime)SqlDateTime.MinValue;
                if (IsFullImport)
                {
                    GameLastImportDateUtc = sqlMinDateTime;
                    GameLastSaveDateUtc = sqlMinDateTime;
                    UserLastImportDateUtc = sqlMinDateTime;
                    PlatformLastImportDateUtc = sqlMinDateTime;
                    SpeedRunLastImportDateUtc = sqlMinDateTime;
                    SpeedRunLastSaveDateUtc = sqlMinDateTime;
                }
                else
                {
                    PlatformLastImportDateUtc = _settingService.GetSetting("PlatformLastImportDate")?.Dte ?? DateTime.UtcNow;
                    GameLastImportDateUtc = _settingService.GetSetting("GameLastImportDate")?.Dte ?? DateTime.UtcNow;
                    GameLastSaveDateUtc = _settingService.GetSetting("GameLastSaveDateUtc")?.Dte ?? DateTime.UtcNow;
                    UserLastImportDateUtc = _settingService.GetSetting("UserLastImportDate")?.Dte ?? DateTime.UtcNow;
                    SpeedRunLastImportDateUtc = _settingService.GetSetting("SpeedRunLastImportDate")?.Dte ?? DateTime.UtcNow;
                    SpeedRunLastSaveDateUtc = _settingService.GetSetting("SpeedRunLastSaveDateUtc")?.Dte ?? DateTime.UtcNow;
                }

                BaseService.SqlMinDateTime = sqlMinDateTime;
                BaseService.MaxElementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
                BaseService.MaxRetryCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxRetryCount").Value);
                BaseService.MaxMemorySizeBytes = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxMemorySizeBytes").Value);
                BaseService.PullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("PullDelayMS").Value);
                BaseService.ErrorPullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("ErrorPullDelayMS").Value);
                BaseService.UpdateDaysBack = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("UpdateDaysBack").Value);
                BaseService.SpeedRunComLatestRunsUrl = _config.GetSection("ApiSettings").GetSection("SpeedRunComLatestRunsUrl").Value;
                _logger.Information("Completed Init");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Init");
            }
        }

        public void RunProcesses()
        {
            bool result = true;

            if (IsBulkReload)
            {
                result = _speedRunRepo.CreateFullTables();
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Platform)))
            {
                result = _platformService.ProcessPlatforms(IsFullImport, IsBulkReload);
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.User)))
            {
                result = _userService.ProcessUsers(UserLastImportDateUtc, IsFullImport, IsBulkReload);
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Game)))
            {
                result = _gameService.ProcessGames(GameLastImportDateUtc, IsFullImport, IsBulkReload);
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.SpeedRun)))
            {
                result = _speedRunService.ProcessSpeedRuns(SpeedRunLastImportDateUtc, IsFullImport, IsBulkReload, IsProcessSpeedRunsByGame);
            }

            if (result && IsBulkReload)
            {
                result = _speedRunRepo.RenameFullTables();
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Game) || Processes.Contains(ImportProcess.SpeedRun)))
            {
                var lastSaveDateUtc = GameLastSaveDateUtc < SpeedRunLastSaveDateUtc ? GameLastSaveDateUtc : SpeedRunLastSaveDateUtc;
                result = _speedRunRepo.UpdateSpeedRunRanks(lastSaveDateUtc);
            }

            if (result && IsFullImport)
            {
                _speedRunRepo.RebuildIndexes();
            }
        }

        public DateTime PlatformLastImportDateUtc { get; set; }
        public DateTime GameLastImportDateUtc { get; set; }
        public DateTime GameLastSaveDateUtc { get; set; }
        public DateTime UserLastImportDateUtc { get; set; }
        public DateTime SpeedRunLastImportDateUtc { get; set; }
        public DateTime SpeedRunLastSaveDateUtc { get; set; }
        public bool IsFullImport { get; set; }
        public bool IsBulkReload { get; set; }
        public bool IsProcessSpeedRunsByGame { get; set; }
        public bool IsImportRunning { get; set; }
        public List<ImportProcess> Processes { get; set; }
    }
}
