using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Repository
{
    public class GameRespository : BaseRepository, IGameRepository
    {
        private readonly ILogger _logger;

        public GameRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void CopyGameTables()
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

                                IF OBJECT_ID('dbo.tbl_Variable_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Variable_Full

                                IF OBJECT_ID('dbo.tbl_VariableValue_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_VariableValue_Full

                                IF OBJECT_ID('dbo.tbl_Game_Region_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Region_Full

                                IF OBJECT_ID('dbo.tbl_Game_Moderator_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Moderator_Full

                                SELECT TOP 0 * INTO dbo.tbl_Game_Full FROM dbo.tbl_Game
                                SELECT TOP 0 * INTO dbo.tbl_Level_Full FROM dbo.tbl_Level
                                SELECT TOP 0 * INTO dbo.tbl_Category_Full FROM dbo.tbl_Category
                                SELECT TOP 0 * INTO dbo.tbl_Variable_Full FROM dbo.tbl_Variable
                                SELECT TOP 0 * INTO dbo.tbl_VariableValue_Full FROM dbo.tbl_VariableValue
                                SELECT TOP 0 * INTO dbo.tbl_Game_Platform_Full FROM dbo.tbl_Game_Platform
                                SELECT TOP 0 * INTO dbo.tbl_Game_Region_Full FROM dbo.tbl_Game_Region
                                SELECT TOP 0 * INTO dbo.tbl_Game_Moderator_Full FROM dbo.tbl_Game_Moderator
                                
                                ALTER TABLE [dbo].[tbl_Game_Full] ADD CONSTRAINT [PK_tbl_Game_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Game_Full] ADD CONSTRAINT [DF_tbl_Game_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]
                                ALTER TABLE [dbo].[tbl_Level_Full] ADD CONSTRAINT [PK_tbl_Level_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Category_Full] ADD CONSTRAINT [PK_tbl_Category_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Variable_Full] ADD CONSTRAINT [PK_tbl_Variable_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_VariableValue_Full] ADD CONSTRAINT [PK_tbl_VariableValue_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Game_Platform_Full] ADD CONSTRAINT [PK_tbl_Game_Platform_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Game_Region_Full] ADD CONSTRAINT [PK_tbl_Game_Region_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Game_Moderator_Full] ADD CONSTRAINT [PK_tbl_Game_Moderator_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }

        public void RenameAndDropGameTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"EXEC sp_rename 'dbo.tbl_Game', 'tbl_Game_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Level', 'tbl_Level_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Category', 'tbl_Category_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Variable', 'tbl_Variable_ToRemove'
                                EXEC sp_rename 'dbo.tbl_VariableValue', 'tbl_VariableValue_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Platform', 'tbl_Game_Platform_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Region', 'tbl_Game_Region_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Moderator', 'tbl_Game_Moderator_ToRemove'

                                EXEC sp_rename 'dbo.tbl_Game_Full', 'tbl_Game'
                                EXEC sp_rename 'dbo.tbl_Level_Full', 'tbl_Level'
                                EXEC sp_rename 'dbo.tbl_Category_Full', 'tbl_Category'
                                EXEC sp_rename 'dbo.tbl_Variable_Full', 'tbl_Variable'
                                EXEC sp_rename 'dbo.tbl_VariableValue_Full', 'tbl_VariableValue'
                                EXEC sp_rename 'dbo.tbl_Game_Platform_Full', 'tbl_Game_Platform'
                                EXEC sp_rename 'dbo.tbl_Game_Region_Full', 'tbl_Game_Region'
                                EXEC sp_rename 'dbo.tbl_Game_Moderator_Full', 'tbl_Game_Moderator'

                                DROP TABLE dbo.tbl_Game_ToRemove
                                DROP TABLE dbo.tbl_Level_ToRemove
                                DROP TABLE dbo.tbl_Category_ToRemove
                                DROP TABLE dbo.tbl_Variable_ToRemove
                                DROP TABLE dbo.tbl_VariableValue_ToRemove
                                DROP TABLE dbo.tbl_Game_Platform_ToRemove
                                DROP TABLE dbo.tbl_Game_Region_ToRemove
                                DROP TABLE dbo.tbl_Game_Moderator_ToRemove

                                EXEC sp_rename 'dbo.PK_tbl_Game_Full', 'PK_tbl_Game'
                                EXEC sp_rename 'dbo.DF_tbl_Game_Full_ImportedDate', 'DF_tbl_Game_ImportedDate'
                                EXEC sp_rename 'dbo.PK_tbl_Level_Full', 'PK_tbl_Level'
                                EXEC sp_rename 'dbo.PK_tbl_Category_Full', 'PK_tbl_Category'
                                EXEC sp_rename 'dbo.PK_tbl_Variable_Full', 'PK_tbl_Variable'
                                EXEC sp_rename 'dbo.PK_tbl_VariableValue_Full', 'PK_tbl_VariableValue'
                                EXEC sp_rename 'dbo.PK_tbl_Game_Platform_Full', 'PK_tbl_Game_Platform'
                                EXEC sp_rename 'dbo.PK_tbl_Game_Region_Full', 'PK_tbl_Game_Region'
                                EXEC sp_rename 'dbo.PK_tbl_Game_Moderator_Full', 'PK_tbl_Game_Moderator'");
                    tran.Complete();
                }
            }
        }

        public void InsertGames(IEnumerable<GameEntity> games, IEnumerable<LevelEntity> levels, IEnumerable<CategoryEntity> categories, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators)
        {
            _logger.Information("Started InsertGames");
            int batchCount = 0;
            var gamesList = games.ToList();
            while (batchCount < gamesList.Count)
            {
                var gamesBatch = gamesList.Skip(batchCount).Take(MaxBulkRows).ToList();
                var gameIDs = gamesBatch.Select(i => i.ID).Distinct().ToList();
                var levelsBatch = levels.Where(i => gameIDs.Contains(i.GameID)).ToList();
                var categoriesBatch = categories.Where(i => gameIDs.Contains(i.GameID)).ToList();
                var variablesBatch = variables.Where(i => gameIDs.Contains(i.GameID)).ToList();
                var variableIDs = variablesBatch.Select(i => i.ID).Distinct().ToList();
                var variablesValuesBatch = variableValues.Where(i => variableIDs.Contains(i.VariableID)).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBulk<GameEntity>(gamesBatch);
                        db.InsertBulk<LevelEntity>(levelsBatch);
                        db.InsertBulk<CategoryEntity>(categoriesBatch);
                        db.InsertBulk<VariableEntity>(variablesBatch);
                        db.InsertBulk<VariableValueEntity>(variablesValuesBatch);
                        db.InsertBulk<GamePlatformEntity>(gamePlatforms);
                        db.InsertBulk<GameRegionEntity>(gameRegions);
                        db.InsertBulk<GameModeratorEntity>(gameModerators);
                        tran.Complete();
                    }
                }

                _logger.Information("Saved games {@Count} / {@Total}", gamesBatch.Count, gamesList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertGames");
        }

        public IEnumerable<GameEntity> GetGameViews(Expression<Func<GameEntity, bool>> predicate)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<GameEntity>().Where(predicate).ToList();
            }
        }
    }
}
