using Microsoft.Extensions.Caching.Memory;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using SpeedRunAppImport.Model.Entity;
using System.Collections.Generic;

namespace SpeedRunApp.Service
{
    public class CacheService : ICacheService
    {
        public IMemoryCache _cache { get; set; }
        public IGameRepository _gameRepo { get; set; }

        public CacheService(IMemoryCache cache, IGameRepository gameRepo)
        {
            _cache = cache;
            _gameRepo = gameRepo;
        }

        public IEnumerable<VariableEntity> GetVariables()
        {
            IEnumerable<VariableEntity> variables = null;
            if (!_cache.TryGetValue<IEnumerable<VariableEntity>>("variables", out variables))
            {
                variables = _gameRepo.GetVariables();
                _cache.Set("variables", variables);
            }

            return variables;
        }
    }
}
