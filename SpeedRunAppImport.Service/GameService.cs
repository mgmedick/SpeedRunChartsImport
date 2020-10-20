using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunApp.Client;
using SpeedRunApp.Model.Data;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Service
{
    public class GameService : IGameService
    {
        private readonly IConfiguration _config = null;
        private readonly IGameRepository _gameRepo = null;

        public GameService(IConfiguration config, IGameRepository gameRepo)
        {
            _config = config;
            _gameRepo = gameRepo;
        }

        public async Task<IEnumerable<Game>> GetGames()
        {
            List<Game> results = new List<Game>();

            ClientContainer clientContainer = new ClientContainer();
            int elementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("SpeedRunListElementsPerPage").Value);
            var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = true, EmbedVariables = true };
            List<Game> games = null;

            do
            {
                games = clientContainer.Games.GetGames(elementsPerPage: elementsPerPage, embeds: gameEmbeds).ToList();
                results.AddRange(games);
                await Task.Delay(2000);
            } while (games.Count == elementsPerPage);

            return results;
        }

        public void InsertGames(IEnumerable<Game> games)
        {
            _gameRepo.InsertGames(games);
        }
    }
}


