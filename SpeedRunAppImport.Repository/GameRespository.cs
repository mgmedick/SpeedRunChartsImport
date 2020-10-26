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
         
        public void TruncateGameDetails()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"TRUNCATE TABLE dbo.tbl_Variable
                                TRUNCATE TABLE dbo.tbl_Category
                                TRUNCATE TABLE dbo.tbl_Level
                                TRUNCATE TABLE dbo.tbl_Game_Platform
                                TRUNCATE TABLE dbo.tbl_Game_Region
                                TRUNCATE TABLE dbo.tbl_Game");
                    tran.Complete();
                }
            }
        }

        public void InsertGameDetails(IEnumerable<GameEntity> games, IEnumerable<LevelEntity> levels, IEnumerable<CategoryEntity> categories, IEnumerable<VariableEntity> variables, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions)
        {
            try
            {
                int batchCount = 0;
                while (batchCount < games.Count())
                {
                    var gamesBatch = games.Skip(batchCount).Take(MaxBulkRows).ToList();
                    var gameIDs = gamesBatch.Select(i => i.ID).Distinct().ToList();
                    var levelsBatch = levels.Where(i => gameIDs.Contains(i.GameID)).ToList();
                    var categoriesBatch = categories.Where(i => gameIDs.Contains(i.GameID)).ToList();
                    var variablesBatch = variables.Where(i => gameIDs.Contains(i.GameID)).ToList();

                    using (IDatabase db = DBFactory.GetDatabase())
                    {
                        using (var tran = db.GetTransaction())
                        {
                            db.InsertBatch<GameEntity>(gamesBatch);
                            db.InsertBatch<LevelEntity>(levelsBatch);
                            db.InsertBatch<CategoryEntity>(categoriesBatch);
                            db.InsertBatch<VariableEntity>(variablesBatch);
                            db.InsertBatch<GamePlatformEntity>(gamePlatforms);
                            db.InsertBatch<GameRegionEntity>(gameRegions);
                            tran.Complete();
                        }
                    }

                    batchCount += MaxBulkRows;
                }
            }
            catch (Exception ex)
            {

            }            
        }
    }
}
