using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Data;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IGameRepository
    {
        void InsertGames(IEnumerable<Game> games);
    }
}




