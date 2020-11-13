using System;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace SpeedRunAppImport.Service
{
    public class GameService : BaseService, IGameService
    {
        private readonly IGameRepository _gameRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public GameService(IGameRepository gameRepo, IConfiguration config, ILogger logger)
        {
            _gameRepo = gameRepo;
            _config = config;
            _logger = logger;
        }

        public IEnumerable<Game> GetGames(DateTime lastImportDate, bool isFullImport)
        {
            _logger.Information("Started GetGames: {@lastImportDate}, {@isFullImport}", lastImportDate, isFullImport);
            var results = new List<Game>();
            List<Game> games = null;
            var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = false, EmbedVariables = true };
            GamesOrdering orderBy = isFullImport ? GamesOrdering.CreationDate : GamesOrdering.CreationDateDescending;

            do
            {
                games = GetGamesWithRetry(MaxElementsPerPage, results.Count, gameEmbeds, orderBy);
                results.AddRange(games);
                _logger.Information("Pulled games: {@New}, total games: {@Total}", games.Count, results.Count);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
            }
            while (games.Count == MaxElementsPerPage && games.Min(i => i.CreationDate ?? SqlMinDateTime) >= lastImportDate);

            if (!IsFullImport)
            {
                var gameIDsToRemove = games.Where(i => (i.CreationDate ?? SqlMinDateTime) < lastImportDate).Select(i => i.ID).ToList();
                results.RemoveAll(i => gameIDsToRemove.Contains(i.ID));
            }
            _logger.Information("Completed GetGames");

            return results.OrderBy(i => i.CreationDate);
        }

        private List<Game> GetGamesWithRetry(int elementsPerPage, int elementsOffset, GameEmbeds embeds, GamesOrdering orderBy, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<Game> games = null;
            try
            {
                games = clientContainer.Games.GetGames(elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, embeds: embeds, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    _logger.Information("Retrying pull games: {@New}, total games: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    GetGamesWithRetry(elementsPerPage, elementsOffset, embeds, orderBy, retryCount++);
                }
                else
                {
                    throw ex;
                }
            }

            return games;
        }

    }
}


