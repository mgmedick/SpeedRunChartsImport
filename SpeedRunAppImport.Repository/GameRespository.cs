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

        public void CopyGameDetailTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"IF OBJECT_ID('dbo.tbl_Game_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Full
                                
                                IF OBJECT_ID('dbo.tbl_Level_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Level_Full

                                IF OBJECT_ID('dbo.tbl_Category_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Category_Full

                                IF OBJECT_ID('dbo.tbl_Variable_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Variable_Full

                                IF OBJECT_ID('dbo.tbl_Game_Platform_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Platform_Full

                                IF OBJECT_ID('dbo.tbl_Game_Region_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Region_Full

                                SELECT TOP 0 * INTO dbo.tbl_Game_Full FROM dbo.tbl_Game
                                SELECT TOP 0 * INTO dbo.tbl_Level_Full FROM dbo.tbl_Level
                                SELECT TOP 0 * INTO dbo.tbl_Category_Full FROM dbo.tbl_Category
                                SELECT TOP 0 * INTO dbo.tbl_Variable_Full FROM dbo.tbl_Variable
                                SELECT TOP 0 * INTO dbo.tbl_Game_Platform_Full FROM dbo.tbl_Game_Platform
                                SELECT TOP 0 * INTO dbo.tbl_Game_Region_Full FROM dbo.tbl_Game_Region");
                    tran.Complete();
                }
            }
        }

        public void RenameAndDropGameDetailTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"EXEC sp_rename 'dbo.tbl_Game', 'tbl_Game_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Level', 'tbl_Level_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Category', 'tbl_Category_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Variable', 'tbl_Variable_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Platform', 'tbl_Game_Platform_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Region', 'tbl_Game_Region_ToRemove'

                                EXEC sp_rename 'dbo.tbl_Game_Full', 'tbl_Game'
                                EXEC sp_rename 'dbo.tbl_Level_Full', 'tbl_Level'
                                EXEC sp_rename 'dbo.tbl_Category_Full', 'tbl_Category'
                                EXEC sp_rename 'dbo.tbl_Variable_Full', 'tbl_Variable'
                                EXEC sp_rename 'dbo.tbl_Game_Platform_Full', 'tbl_Game_Platform'
                                EXEC sp_rename 'dbo.tbl_Game_Region_Full', 'tbl_Game_Region'

                                DROP TABLE dbo.tbl_Game_ToRemove
                                DROP TABLE dbo.tbl_Level_ToRemove
                                DROP TABLE dbo.tbl_Category_ToRemove
                                DROP TABLE dbo.tbl_Variable_ToRemove
                                DROP TABLE dbo.tbl_Game_Platform_ToRemove
                                DROP TABLE dbo.tbl_Game_Region_ToRemove

                                ALTER TABLE [dbo].[tbl_Game] ADD CONSTRAINT [PK_tbl_Game] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Level] ADD CONSTRAINT [PK_tbl_Level] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Category] ADD CONSTRAINT [PK_tbl_Category] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [PK_tbl_Variable] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Game_Platform] ADD CONSTRAINT [PK_tbl_Game_Platform] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Game_Region] ADD CONSTRAINT [PK_tbl_Game_Region] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
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
