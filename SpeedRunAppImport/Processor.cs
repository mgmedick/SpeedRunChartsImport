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
using System.IO;
using MySqlX.XDevAPI.Common;

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
        private readonly IGameRepository _gameRepo;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Processor(IGameService gameService, IUserService userService, ISpeedRunService speedRunService, IPlatformService platformService, ISettingService settingService, ISpeedRunRepository speedRunRepo, IUserRepository userRepo, IGameRepository gameRepo, IConfiguration config, ILogger logger)
        {
            _gameService = gameService;
            _userService = userService;
            _speedRunService = speedRunService;
            _platformService = platformService;
            _settingService = settingService;
            _userRepo = userRepo;
            _speedRunRepo = speedRunRepo;
            _gameRepo = gameRepo;

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
                IsMySQL = Convert.ToBoolean(_config.GetSection("AppSettings").GetSection("IsMySQL").Value);
                IsBulkReload = _config.GetValue<bool>("IsBulkReload");
                IsPlatformFullPull = _config.GetValue<bool>("IsPlatformFullPull");
                IsGameFullPull = _config.GetValue<bool>("IsGameFullPull");
                IsSpeedRunFullPull = _config.GetValue<bool>("IsSpeedRunFullPull");
                IsMaintenance = _config.GetValue<bool>("IsMaintenance");
                IsUpdateSpeedRuns = _config.GetValue<bool>("IsUpdateSpeedRuns");
                Processes = _config.GetValue<string>("ProcessIDs").Split(",").Select(i => (ImportProcess)Convert.ToInt32(i)).ToList();
                NPocoBootstrapper.Configure(connString, maxBulkRows, IsBulkReload, IsMySQL);

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
                IsBulkReloadRunning = _settingService.GetSetting("IsBulkReloadRunning")?.Num == 1;
                IsBulkReloadPostProcessRunning = _settingService.GetSetting("IsBulkReloadPostProcessRunning")?.Num == 1;
                ImportLastBulkReloadDateUtc = (_settingService.GetSetting("ImportLastBulkReloadDate")?.Dte ?? currDateUtc);
                IsGetLatestSpeedRunsCheckEnabled = _settingService.GetSetting("IsGetLatestSpeedRunsCheckEnabled")?.Num == 1;

                var updateSpeedRunsTimeString = _settingService.GetSetting("UpdateSpeedRunsTime")?.Str;

                if (IsUpdateSpeedRuns)
                {
                    IsGameFullPull = true;
                    IsUpdateSpeedRunVideoDetails = true;
                }
                else if (!string.IsNullOrWhiteSpace(updateSpeedRunsTimeString) && !IsBulkReloadRunning)
                {
                    var currDateLocal = currDateUtc.ToLocalTime();
                    var ImportLastRunDateLocal = ImportLastRunDateUtc.ToLocalTime();
                    var updateSpeedRunsTime = TimeSpan.Parse(updateSpeedRunsTimeString);
                    var startDateLocal = currDateLocal.Date.Add(updateSpeedRunsTime);

                    if (startDateLocal <= currDateLocal && startDateLocal > ImportLastRunDateLocal)
                    {
                        IsGameFullPull = true;
                        IsUpdateSpeedRuns = true;
                        IsUpdateSpeedRunVideoDetails = true;
                    }
                }

                BaseService.SqlMinDateTime = sqlMinDateTime;
                BaseService.MaxElementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
                BaseService.MaxElementsPerPageSM = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPageSM").Value);
                BaseService.MaxRetryCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxRetryCount").Value);
                BaseService.MaxMemorySizeBytes = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxMemorySizeBytes").Value);
                BaseService.PullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("PullDelayMS").Value);
                BaseService.PullDelayShortMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("PullDelayShortMS").Value);
                BaseService.ErrorPullDelayMS = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("ErrorPullDelayMS").Value);
                BaseService.SpeedRunComLatestRunsUrl = _config.GetSection("ApiSettings").GetSection("SpeedRunComLatestRunsUrl").Value;
                BaseService.TwitchClientID = _config.GetSection("ApiSettings").GetSection("TwitchClientID").Value;
                BaseService.TwitchClientKey = _config.GetSection("ApiSettings").GetSection("TwitchClientKey").Value;
                BaseService.TwitchAPIEnabled = _settingService.GetSetting("TwitchAPIEnabled")?.Num == 1;
                BaseService.TwitchAPIMaxBatchCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("TwitchAPIMaxBatchCount").Value);
                BaseService.YouTubeAPIKey = _config.GetSection("ApiSettings").GetSection("YouTubeAPIKey").Value;
                BaseService.YouTubeAPIDailyRequestLimit = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("YouTubeAPIDailyRequestLimit").Value);
                BaseService.YouTubeAPIMaxBatchCount = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("YouTubeAPIMaxBatchCount").Value);
                BaseService.YouTubeAPIRequestCount = 0;
                BaseService.YouTubeAPIEnabled = _settingService.GetSetting("YouTubeAPIEnabled")?.Num == 1;
                BaseService.GameIDsToUpdateSpeedRuns = new List<int>();
                BaseService.UserIDsToUpdateSpeedRuns = new List<int>();
                BaseService.BaseWebPath = _config.GetSection("AppSettings").GetSection("BaseWebPath").Value;
                BaseService.GameImageWebPath = _config.GetSection("AppSettings").GetSection("GameImageWebPath").Value;
                BaseService.ImageFileExt = _config.GetSection("AppSettings").GetSection("ImageFileExt").Value;
                BaseService.TempImportPath = _config.GetSection("AppSettings").GetSection("TempImportPath").Value;
                BaseService.IsProcessGameCoverImages = _settingService.GetSetting("IsProcessGameCoverImages")?.Num == 1;

                if (!Directory.Exists(BaseService.TempImportPath))
                {
                    Directory.CreateDirectory(BaseService.TempImportPath);
                }

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
            _logger.Information("Started RunProcesses");

            if (IsBulkReload)
            {
                _settingService.UpdateSetting("IsBulkReloadRunning", 1);
                result = _speedRunRepo.CreateFullTables();
            }
            else if (IsBulkReloadPostProcessRunning)
            {
                return;
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Platform)))
            {
                result = _platformService.ProcessPlatforms(IsPlatformFullPull, IsBulkReload);
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.Game)))
            {
                result = _gameService.ProcessGames(GameLastImportDateUtc, IsGameFullPull, IsBulkReload);
            }

            if (result && !IsBulkReload && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.User)))
            {
                result = _userService.ProcessChangedUsers();
            }

            if (result && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.SpeedRun)))
            {
                result = _speedRunService.ProcessSpeedRuns(SpeedRunLastImportDateUtc, ImportLastRunDateUtc, IsSpeedRunFullPull, IsBulkReload, IsUpdateSpeedRuns);
            }

            if (result && !IsBulkReload && IsUpdateSpeedRunVideoDetails)
            {
                var isPostBulkImport = ImportLastRunDateUtc <= ImportLastBulkReloadDateUtc;
                result = _speedRunService.UpdateSpeedRunVideoDetails(isPostBulkImport, ImportLastRunDateUtc);
            }

            if (result)
            {
                if (IsBulkReload)
                {
                    result = _speedRunRepo.ReorderSpeedRuns();

                    if (result)
                    {
                        result = _speedRunService.ProcessYouTubeSpeedRunVideoDetails();
                    }

                    _settingService.UpdateSetting("IsBulkReloadPostProcessRunning", 1);
                    if (result)
                    {
                        result = _speedRunRepo.UpdateSpeedRunRanksFull();
                    }

                    if (result)
                    {
                        result = _speedRunRepo.RenameFullTables();
                    }
                }
                else if (BaseService.IsSpeedRunsImported && (Processes.Contains(ImportProcess.All) || Processes.Contains(ImportProcess.SpeedRun)))
                {
                    result = _speedRunRepo.UpdateSpeedRunRanks(ImportLastRunDateUtc);
                }
            }

            if (result)
            {
                if (IsBulkReload || IsUpdateSpeedRuns)
                {
                    result = _settingService.GenerateAndMoveSitemapXml();
                }
            }

            if (result)
            {
                var isLoadingResults = true;
                if (IsGetLatestSpeedRunsCheckEnabled)
                {
                    isLoadingResults = _speedRunRepo.GetLatestSpeedRuns(0, 10, null, null);
                }

                if (!isLoadingResults)
                {
                    result = _speedRunRepo.KillOtherProcesses("vw_speedrunsummary");

                    if (IsBulkReload || IsUpdateSpeedRuns)
                    {
                        result = _speedRunRepo.AnalyzeTables();
                    }

                    if (result)
                    {
                        isLoadingResults = _speedRunRepo.GetLatestSpeedRuns(0, 10, null, null);

                        if (!isLoadingResults)
                        {
                            result = _speedRunRepo.KillOtherProcesses("vw_speedrunsummary");

                            if (result)
                            {
                                result = _speedRunRepo.RecreateSpeedRunIndexes();

                                if (result)
                                {
                                    isLoadingResults = _speedRunRepo.GetLatestSpeedRuns(0, 10, null, null);

                                    if (!isLoadingResults)
                                    {
                                        _settingService.UpdateSetting("IsGetLatestSpeedRunsCheckEnabled", 0);
                                        _speedRunRepo.KillOtherProcesses("vw_speedrunsummary");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var currDateUtc = DateTime.UtcNow;
            if (IsBulkReload)
            {
                _settingService.UpdateSetting("IsBulkReloadRunning", 0);
                _settingService.UpdateSetting("IsBulkReloadPostProcessRunning", 0);
                _settingService.UpdateSetting("ImportLastBulkReloadDate", currDateUtc);

                var gameLastImportDateUtc = _gameRepo.GetMaxGameCreatedDate();
                _settingService.UpdateSetting("GameLastImportDate", gameLastImportDateUtc);

                var speedRunLastImportDateUtc = _speedRunRepo.GetMaxSpeedRunVerifyDate();
                _settingService.UpdateSetting("SpeedRunLastImportDate", speedRunLastImportDateUtc);
            }

            if (IsUpdateSpeedRuns)
            {
                _settingService.UpdateSetting("ImportLastUpdateSpeedRunsDate", currDateUtc);
            }

            _settingService.UpdateSetting("ImportLastRunDate", currDateUtc);
            _logger.Information("Completed RunProcesses");
        }

        public DateTime PlatformLastImportDateUtc { get; set; }
        public DateTime GameLastImportDateUtc { get; set; }
        public DateTime UserLastImportDateUtc { get; set; }
        public DateTime SpeedRunLastImportDateUtc { get; set; }
        public DateTime ImportLastRunDateUtc { get; set; }
        public DateTime ImportLastBulkReloadDateUtc { get; set; }
        public bool IsPlatformFullPull { get; set; }
        public bool IsGameFullPull { get; set; }
        public bool IsSpeedRunFullPull { get; set; }
        public bool IsBulkReload { get; set; }
        public bool IsBulkReloadRunning { get; set; }
        public bool IsBulkReloadPostProcessRunning { get; set; }
        public bool IsMaintenance { get; set; }
        public bool IsUpdateSpeedRuns { get; set; }
        public bool IsMySQL { get; set; }
        public bool IsUpdateSpeedRunVideoDetails { get; set; }
        public bool IsGetLatestSpeedRunsCheckEnabled { get; set; }    
        public List<ImportProcess> Processes { get; set; }
    }
}
