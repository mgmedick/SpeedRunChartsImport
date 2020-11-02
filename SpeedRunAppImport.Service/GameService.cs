using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunApp.Client;
using SpeedRunApp.Model;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace SpeedRunAppImport.Service
{
    public class GameService : BaseService, IGameService
    {
        private readonly IConfiguration _config = null;
        private readonly IGameRepository _gameRepo = null;

        public GameService(IConfiguration config, IGameRepository gameRepo)
        {
            _config = config;
            _gameRepo = gameRepo;
        }

        public IEnumerable<Game> GetGames(DateTime lastImportDate, bool isFullImport)
        {
            var results = new List<Game>();
            List<Game> games = null;
            var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = false, EmbedVariables = true };

            do
            {
                games = GetGamesWithRetry(MaxElementsPerPage, results.Count, gameEmbeds, GamesOrdering.CreationDateDescending).ToList();
                results.AddRange(games);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            while (games.Count == MaxElementsPerPage && games.Min(i => i.CreationDate ?? DateTime.MinValue) >= lastImportDate);

            if (!IsFullImport)
            {
                var gameIDsToRemove = games.Where(i => (i.CreationDate ?? DateTime.MinValue) < lastImportDate).Select(i => i.ID).ToList();
                results.RemoveAll(i => gameIDsToRemove.Contains(i.ID));
            }

            return results;
        }

        private IEnumerable<Game> GetGamesWithRetry(int elementsPerPage, int elementsOffset, GameEmbeds embeds, GamesOrdering orderBy, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            IEnumerable<Game> games = null;
            try
            {
                games = clientContainer.Games.GetGames(elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, embeds: embeds, orderBy: orderBy);
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
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


