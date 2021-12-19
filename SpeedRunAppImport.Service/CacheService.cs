using Microsoft.Extensions.Caching.Memory;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using SpeedRunAppImport.Model.Entity;
using System.Collections.Generic;
using SpeedRunCommon;
using System;
using Serilog;

namespace SpeedRunAppImport.Service
{
    public class CacheService : BaseService, ICacheService
    {
        public IMemoryCache _cache { get; set; }
        public IGameRepository _gameRepo { get; set; }
        private readonly ILogger _logger;

        public CacheService(IMemoryCache cache, IGameRepository gameRepo, ILogger logger)
        {
            _cache = cache;
            _gameRepo = gameRepo;
            _logger = logger;
        }

        public string GetTwitchToken()
        {
            string token = null;
            var isValid = false;

            if (_cache.TryGetValue<string>("twitchtoken", out token))
            {
                var requestString = "https://id.twitch.tv/oauth2/validate";
                var parameters = new Dictionary<string, string>() { { "Authorization", "Bearer " + token } };

                try
                {
                    isValid = JsonHelper.FromUri(new Uri(requestString), parameters) != null;
                }
                catch (Exception ex)
                {
                    isValid = false;
                    _logger.Error(ex, "GetTwitchToken");
                }

                if (!isValid)
                {
                    token = GenerateTwitchToken();
                    _cache.Set("twitchtoken", token);
                }
            }
            else
            {
                token = GenerateTwitchToken();
                _cache.Set("twitchtoken", token);
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

        //public IEnumerable<VariableEntity> GetVariables()
        //{
        //    IEnumerable<VariableEntity> variables = null;
        //    if (!_cache.TryGetValue<IEnumerable<VariableEntity>>("variables", out variables))
        //    {
        //        variables = _gameRepo.GetVariables();
        //        _cache.Set("variables", variables);
        //    }

        //    return variables;
        //}
    }
}
