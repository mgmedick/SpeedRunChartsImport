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
        private readonly IMemoryCache _cache = null;
        private readonly IGameRepository _gameRepo = null;
        private readonly ISettingService _settingService = null;
        private readonly ILogger _logger = null;

        public CacheService(IMemoryCache cache, IGameRepository gameRepo, ISettingService settingService, ILogger logger)
        {
            _cache = cache;
            _gameRepo = gameRepo;
            _settingService = settingService;
            _logger = logger;
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
