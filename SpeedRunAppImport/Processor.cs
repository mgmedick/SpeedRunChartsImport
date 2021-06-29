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
        private readonly ISettingService _settingService;
        private readonly ISpeedRunRepository _speedRunRepo;
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Processor(IGameService gameService, IUserService userService, ISpeedRunService speedRunService, IPlatformService platformService, ISettingService settingService, ISpeedRunRepository speedRunRepo, IUserRepository userRepo, IConfiguration config, ILogger logger)
        {
            _gameService = gameService;
            _userService = userService;
            _speedRunService = speedRunService;
            _platformService = platformService;
            _settingService = settingService;
            _userRepo = userRepo;
            _speedRunRepo = speedRunRepo;

            _config = config;
            _logger = logger;
        }

        public void Run()
        {
            try
            {
                Init();
                if (IsMaintenance)
                {
                    RunMaintenance();
                }
                else
                {
                    RunProcesses();
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
                IsBulkReload = _config.GetValue<bool>("IsBulkReload");
                IsPlatformFullPull = _config.GetValue<bool>("IsPlatformFullPull");
                IsGameFullPull = _config.GetValue<bool>("IsGameFullPull");
                IsSpeedRunFullPull = _config.GetValue<bool>("IsSpeedRunFullPull");
                IsMaintenance = _config.GetValue<bool>("IsMaintenance");
                IsUpdateSpeedRuns = _config.GetValue<bool>("IsUpdateSpeedRuns");
                Processes = _config.GetValue<string>("ProcessIDs").Split(",").Select(i => (ImportProcess)Convert.ToInt32(i)).ToList();
                NPocoBootstrapper.Configure(connString, maxBulkRows, IsBulkReload);

                if (IsBulkReload)
                {
                    IsPlatformFullPull = true;
                    IsGameFullPull = true;
                    IsSpeedRunFullPull = true;
                }

                if (Processes.Contains(ImportProcess.All))
                {
                    Processes.RemoveAll(i => i != ImportProcess.All);
                }

                var sqlMinDateTime = (DateTime)SqlDateTime.MinValue;
                var currDateUtc = DateTime.UtcNow;

                PlatformLastImportDateUtc = IsPlatformFullPull ? sqlMinDateTime : (_settingService.GetSetting("PlatformLastImportDate")?.Dte ?? currDateUtc);
                GameLastImportDateUtc = IsGameFullPull ? sqlMinDateTime : (_settingService.GetSetting("GameLastImportDate")?.Dte ?? currDateUtc);
                SpeedRunLastImportDateUtc = IsSpeedRunFullPull ? sqlMinDateTime : (_settingService.GetSetting("SpeedRunLastImportDate")?.Dte ?? currDateUtc);
                ImportLastRunDateUtc = IsBulkReload ? sqlMinDateTime : (_settingService.GetSetting("ImportLastRunDate")?.Dte ?? currDateUtc);

                var currDateLocal = currDateUtc.ToLocalTime();
                var updateSpeedRunsTimeString = _settingService.GetSetting("UpdateSpeedRunsTime")?.Str ?? "00:00";
                var updateSpeedRunsTime = TimeSpan.Parse(updateSpeedRunsTimeString);
                var startDateLocal = currDateLocal.Date.Add(updateSpeedRunsTime);
                var ImportLastRunDateLocal = ImportLastRunDateUtc.ToLocalTime();

                if ((IsUpdateSpeedRuns) || (startDateLocal <= currDateLocal && startDateLocal > ImportLastRunDateLocal))
                {
                    IsGameFullPull = true;
                    IsUpdateSpeedRuns = true;
                }

                BaseService.SqlMinDateTime = sqlMinDateTime;
                BaseService.MaxElementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
                BaseService.MaxRetryCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxRetryCount").Value);
                BaseService.MaxMemorySizeBytes = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxMemorySizeBytes").Value);
                BaseService.PullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("PullDelayMS").Value);
                BaseService.ErrorPullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("ErrorPullDelayMS").Value);
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

            //if (IsBulkReload)
            //{
            //    result = _speedRunRepo.CreateFullTables();
            //}

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Platform)))
            {
                result = _platformService.ProcessPlatforms(IsPlatformFullPull, IsBulkReload);
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Game)))
            {
                result = _gameService.ProcessGames(GameLastImportDateUtc, IsGameFullPull, IsBulkReload);
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.SpeedRun)))
            {
                result = _speedRunService.ProcessSpeedRuns(SpeedRunLastImportDateUtc, ImportLastRunDateUtc, IsSpeedRunFullPull, IsBulkReload, IsUpdateSpeedRuns);
            }

            if (result && IsBulkReload)
            {
                result = _speedRunRepo.RenameFullTables();
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Game) || Processes.Contains(ImportProcess.SpeedRun)))
            {
                result = _speedRunRepo.UpdateSpeedRunRanks(ImportLastRunDateUtc);
            }

            if (result && (IsBulkReload || IsUpdateSpeedRuns))
            {
                RunMaintenance();
            }

            _settingService.UpdateSetting("ImportLastRunDate", DateTime.UtcNow);
        }

        public void RunMaintenance()
        {
            bool result = true;
            result = _speedRunRepo.RebuildIndexes();

            if (result)
            {
                _speedRunRepo.UpdateStats();
            }
        }

        public DateTime PlatformLastImportDateUtc { get; set; }
        public DateTime GameLastImportDateUtc { get; set; }
        public DateTime UserLastImportDateUtc { get; set; }
        public DateTime SpeedRunLastImportDateUtc { get; set; }
        public DateTime ImportLastRunDateUtc { get; set; }
        public bool IsPlatformFullPull { get; set; }
        public bool IsGameFullPull { get; set; }
        public bool IsSpeedRunFullPull { get; set; }
        public bool IsBulkReload { get; set; }
        public bool IsMaintenance { get; set; }
        public bool IsUpdateSpeedRuns { get; set; }
        public List<ImportProcess> Processes { get; set; }
    }
}
