using System;
using NPoco;
using System.Collections.Generic;
using SpeedRunApp.Model.Data;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace SpeedRunAppImport.Repository
{
    public class GameRespository : BaseRepository, IGameRepository
    {
        public GameRespository() : base()
        {
        }

        public void InsertGames(IEnumerable<Game> games)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.InsertBatch<Game>(games);
            }
        }
    }
}
