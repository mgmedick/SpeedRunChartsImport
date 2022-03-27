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
using System.Xml;
using System.IO;

namespace SpeedRunAppImport.Service
{
    public class SettingService : BaseService, ISettingService
    {
        private readonly ISettingRepository _settingRepo = null;
        private readonly IGameRepository _gameRepo = null;
        private readonly ILogger _logger = null;

        public SettingService(ISettingRepository settingRepo, IGameRepository gameRepo, ILogger logger)
        {
            _settingRepo = settingRepo;
            _gameRepo = gameRepo;
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

        public bool GenerateAndMoveSitemapXml()
        {
            bool result = false;
            var tempFilePath = GenerateSitemapXml();

            if (!string.IsNullOrWhiteSpace(tempFilePath))
            {
                result = MoveSitemapXml(tempFilePath);
            }

            return result;
        }

        private string GenerateSitemapXml()
        {
            var tempFilePath = Path.Combine(TempImportPath, "sitemap.xml");

            try
            {
                using (var xml = XmlWriter.Create(tempFilePath, new XmlWriterSettings { Indent = true }))
                {
                    xml.WriteStartDocument();
                    xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    xml.WriteStartElement("url");
                    xml.WriteElementString("loc", "https://speedruncharts.com/");
                    xml.WriteElementString("lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                    xml.WriteElementString("changefreq", "monthly");
                    xml.WriteEndElement();

                    xml.WriteStartElement("url");
                    xml.WriteElementString("loc", "https://speedruncharts.com/Menu/About");
                    xml.WriteElementString("changefreq", "monthly");
                    xml.WriteEndElement();

                    var games = _gameRepo.GetGamesForSitemap();
                    foreach (var game in games)
                    {
                        xml.WriteStartElement("url");
                        xml.WriteElementString("loc", string.Format("https://speedruncharts.com/Game/GameDetails/{0}", game.Abbr));
                        xml.WriteElementString("lastmod", game.LastModifiedDate.ToString("yyyy-MM-dd"));
                        xml.WriteElementString("changefreq", "monthly");
                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                }
            }
            catch (Exception ex)
            {
                tempFilePath = null;
                _logger.Error(ex, "GenerateSitemapXml");
            }

            return tempFilePath;
        }

        private bool MoveSitemapXml(string tempFilePath)
        {
            bool result = false;
            try
            {
                var fileName = Path.GetFileName(tempFilePath);
                var destFilePath = Path.Combine(BaseWebPath, fileName);
                File.Move(tempFilePath, destFilePath, true);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "MoveSitemapXml");
            }

            return result;
        }
    }
}


