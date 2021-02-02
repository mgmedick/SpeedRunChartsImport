using Microsoft.Extensions.Caching.Memory;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using SpeedRunAppImport.Model.Entity;
using System.Collections.Generic;

namespace SpeedRunAppImport.Service
{
    public class CacheService : ICacheService
    {
        public IMemoryCache _cache { get; set; }
        public IGameRepository _gameRepo { get; set; }
        public IPlatformRepository _platformRepo { get; set; }

        public CacheService(IMemoryCache cache, IGameRepository gameRepo, IPlatformRepository platformRepo)
        {
            _cache = cache;
            _gameRepo = gameRepo;
            _platformRepo = platformRepo;
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

        public IEnumerable<GameEntity> GetGames()
        {
            IEnumerable<GameEntity> games = null;
            if (!_cache.TryGetValue<IEnumerable<GameEntity>>("games", out games))
            {
                games = _gameRepo.GetGames();
                _cache.Set("games", games);
            }

            return games;
        }

        public IEnumerable<PlatformEntity> GetPlatforms()
        {
            IEnumerable<PlatformEntity> platforms = null;
            if (!_cache.TryGetValue<IEnumerable<PlatformEntity>>("platforms", out platforms))
            {
                platforms = _platformRepo.GetPlatforms();
                _cache.Set("platforms", platforms);
            }

            return platforms;
        }

        public IEnumerable<PlatformSpeedRunComIDEntity> GetPlatformSpeedRunComIDs()
        {
            IEnumerable<PlatformSpeedRunComIDEntity> platformSpeedRunComIDs = null;
            if (!_cache.TryGetValue<IEnumerable<PlatformSpeedRunComIDEntity>>("platformspeedruncomids", out platformSpeedRunComIDs))
            {
                platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs();
                _cache.Set("platformspeedruncomids", platformSpeedRunComIDs);
            }

            return platformSpeedRunComIDs;
        }
    }
}
