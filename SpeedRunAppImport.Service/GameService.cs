using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunApp.Client;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;

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

        public IEnumerable<Game> GetGames()
        {
            List<Game> results = new List<Game>();

            ClientContainer clientContainer = new ClientContainer();
            int elementsPerPage = Convert.ToInt32(_config.GetSection("ApiSettings").GetSection("MaxElementsPerPage").Value);
            var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = true, EmbedVariables = true };
            List<Game> games = null;

            do
            {
                games = clientContainer.Games.GetGames(elementsPerPage: elementsPerPage, embeds: gameEmbeds).ToList();
                results.AddRange(games);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            while (games.Count == elementsPerPage);

            return results;
        }

        public void InsertGames(IEnumerable<Game> games)
        {
            var gameEntities = games.Select(i => new GameEntity
            {
                SpeedRunComID = i.ID,
                Name = i.Name,
                JapaneseName = i.JapaneseName,
                Abbreviation = i.Abbreviation,
                YearOfRelease = i.YearOfRelease,
                IsRomHack = i.IsRomHack,
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            });

            _gameRepo.InsertGames(gameEntities);
        }
    }
}


