using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using SpeedRunAppImport.Interfaces;
using SpeedRunAppImport.Repository.Configuration;

namespace SpeedRunAppImport.Processor
{
    public class Processor : IProcessor
    {
        private readonly IGameService _gameService = null;
        private readonly IConfiguration _config = null;

        public Processor(IGameService gameService, IConfiguration config)
        {
            _gameService = gameService;
            _config = config;

            var connString = _config.GetSection("ConnectionStrings").GetSection("DBConnectionString").Value;
            NPocoBootstrapper.Configure(connString);
        }

        public bool ProcessGames()
        {
            bool result = true;

            try
            {
                var games = Task.Run(async () => await _gameService.GetGames()).Result;
                _gameService.InsertGames(games);
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }
    }
}
