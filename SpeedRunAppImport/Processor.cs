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
                IsFullImport = _config.GetValue<bool>("IsFullImport");
                NPocoBootstrapper.Configure(connString, maxBulkRows, IsFullImport);

                Processes = _config.GetValue<string>("ProcessIDs").Split(",").Select(i => (ImportProcess)Convert.ToInt32(i)).ToList();
                if (Processes.Contains(ImportProcess.All))
                {
                    Processes.RemoveAll(i => i != ImportProcess.All);
                }

                var sqlMinDateTime = (DateTime)SqlDateTime.MinValue;
                if (IsFullImport)
                {
                    GameLastImportDate = sqlMinDateTime;
                    UserLastImportDate = sqlMinDateTime;
                    PlatformLastImportDate = sqlMinDateTime;
                    SpeedRunLastImportDate = sqlMinDateTime;
                    LeaderboardLastImportDate = sqlMinDateTime;
                }
                else
                {
                    PlatformLastImportDate = _settingService.GetSetting("PlatformLastImportDate")?.Dte ?? DateTime.Now;
                    GameLastImportDate = _settingService.GetSetting("GameLastImportDate")?.Dte ?? DateTime.Now;
                    UserLastImportDate = _settingService.GetSetting("UserLastImportDate")?.Dte ?? DateTime.Now;
                    SpeedRunLastImportDate = _settingService.GetSetting("SpeedRunLastImportDate")?.Dte ?? DateTime.Now;
                    LeaderboardLastImportDate = _settingService.GetSetting("LeaderboardLastImportDate")?.Dte ?? DateTime.Now;
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
            if (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Platform))
            {
                _platformService.ProcessPlatforms(IsFullImport);
            }

            if (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.User))
            {
                _userService.ProcessUsers(UserLastImportDate, IsFullImport);
            }

            if (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Game))
            {
                _gameService.ProcessGames(GameLastImportDate, IsFullImport);
            }

            if (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.SpeedRun))
            {
                _speedRunService.ProcessSpeedRuns(SpeedRunLastImportDate, IsFullImport);
            }

            if (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Game) || Processes.Contains(ImportProcess.SpeedRun))
            {
                var lastImportDate = GameLastImportDate > SpeedRunLastImportDate ? GameLastImportDate : SpeedRunLastImportDate;

                _speedRunRepo.UpdateSpeedRunRanks(lastImportDate);
            }
        }

        public DateTime PlatformLastImportDate { get; set; }
        public DateTime GameLastImportDate { get; set; }
        public DateTime UserLastImportDate { get; set; }
        public DateTime SpeedRunLastImportDate { get; set; }
        public DateTime LeaderboardLastImportDate { get; set; }
        public bool IsFullImport { get; set; }
        public bool IsImportRunning { get; set; }
        public List<ImportProcess> Processes { get; set; }
    }
}
