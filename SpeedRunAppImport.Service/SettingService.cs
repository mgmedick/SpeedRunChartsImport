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
using SpeedRunCommon;
using Serilog;

namespace SpeedRunAppImport.Service
{
    public class SettingService : BaseService, ISettingService
    {
        private readonly ISettingRepository _settingRepo = null;
        private readonly ILogger _logger = null;

        public SettingService(ISettingRepository settingRepo, ILogger logger)
        {
            _settingRepo = settingRepo;
            _logger = logger;
        }

        public SettingEntity GetSetting(string name)
        {
            return _settingRepo.GetSetting(name);
        }

        public void UpdateSetting(string name, DateTime value)
        {
            var setting = _settingRepo.GetSetting(name);
            setting.Dte = value;
            _settingRepo.UpdateSetting(setting);
        }

        public void UpdateSetting(string name, string value)
        {
            var setting = _settingRepo.GetSetting(name);
            setting.Str = value;
            _settingRepo.UpdateSetting(setting);
        }

        public void UpdateSetting(string name, int value)
        {
            var setting = _settingRepo.GetSetting(name);
            setting.Num = value;
            _settingRepo.UpdateSetting(setting);
        }

        public void UpdateSetting(SettingEntity setting)
        {
            _settingRepo.UpdateSetting(setting);
        }

        public string GetTwitchToken()
        {
            string token = GetSetting("TwitchToken")?.Str; ;

            if (string.IsNullOrWhiteSpace(token) || !ValidateTwitchToken(token))
            {
                token = GenerateTwitchToken();
                UpdateSetting("TwitchToken", token);
            }

            return token;
        }

        private string GenerateTwitchToken()
        {
            string result = null;

            try
            {
                var oathRequestUri = new Uri(String.Format("https://id.twitch.tv/oauth2/token?client_id={0}&client_secret={1}&grant_type=client_credentials", TwitchClientID, TwitchClientKey));
                var twitchtoken = JsonHelper.FromUriPost(oathRequestUri);
                result = (string)twitchtoken.access_token;
            }
            catch (Exception ex)
            {
                result = null;
                _logger.Error(ex, "GenerateTwitchToken");
            }

            return result;
        }

        private bool ValidateTwitchToken(string token)
        {
            bool result = false;
            var requestString = "https://id.twitch.tv/oauth2/validate";
            var parameters = new Dictionary<string, string>() { { "Authorization", "Bearer " + token } };

            try
            {
                result = JsonHelper.FromUri(new Uri(requestString), parameters) != null;
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "GetTwitchToken");
            }

            return result;
        }
    }
}


