using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Data;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface IGameService
    {
        IEnumerable<Game> GetGames();
        void InsertGames(IEnumerable<Game> _gameRepo);
    }
}




