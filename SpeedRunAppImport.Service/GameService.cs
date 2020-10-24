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

        public IEnumerable<Game> GetGames()
        {
            var results = new List<Game>();

            ClientContainer clientContainer = new ClientContainer();
            var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = true, EmbedVariables = true };
            List<Game> games = null;

            do
            {
                games = clientContainer.Games.GetGames(elementsPerPage: MaxElementsPerPage, embeds: gameEmbeds, orderBy: GamesOrdering.CreationDateDescending).ToList();
                results.AddRange(games);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            //while (games.Count == MaxElementsPerPage && results.Min(i => i.CreationDate ?? DateTime.MinValue) >= GameLastImportDate);
            while (1 == 0);

            return results.Where(i => !string.IsNullOrWhiteSpace(i.ID) && (i.CreationDate ?? DateTime.MinValue) >= GameLastImportDate);
        }
    }
}


