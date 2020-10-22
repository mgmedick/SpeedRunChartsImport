using System;
using System.Collections.Generic;
using NPoco;
using System.Linq;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace SpeedRunAppImport.Repository
{
    public class GameRespository : BaseRepository, IGameRepository
    {
        public GameRespository()
        {
        }

        public void InsertGames(IEnumerable<GameEntity> games)
        {
            for (int i = 0; i < games.Count(); i++)
            {
                if (i % 1000 == 0)
                {
                    var batch = games.Skip(i).Take(1000);
                    using (IDatabase db = DBFactory.GetDatabase())
                    {
                        db.BeginTransaction();
                        db.InsertBatch<GameEntity>(batch);
                        db.CompleteTransaction();
                    }
                }
            }
        }
    }
}
