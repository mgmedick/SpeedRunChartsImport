using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
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

                                IF OBJECT_ID('dbo.tbl_Game_Platform_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Platform_Full

                                IF OBJECT_ID('dbo.tbl_Game_Region_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Region_Full

                                IF OBJECT_ID('dbo.tbl_Game_Moderator_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Moderator_Full

                                IF OBJECT_ID('dbo.tbl_Game_Ruleset_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_Ruleset_Full

                                IF OBJECT_ID('dbo.tbl_Game_TimingMethod_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Game_TimingMethod_Full

                                SELECT TOP 0 * INTO dbo.tbl_Game_Full FROM dbo.tbl_Game
                                SELECT TOP 0 * INTO dbo.tbl_Level_Full FROM dbo.tbl_Level
                                SELECT TOP 0 * INTO dbo.tbl_Category_Full FROM dbo.tbl_Category
                                SELECT TOP 0 * INTO dbo.tbl_Variable_Full FROM dbo.tbl_Variable
                                SELECT TOP 0 * INTO dbo.tbl_VariableValue_Full FROM dbo.tbl_VariableValue
                                SELECT TOP 0 * INTO dbo.tbl_Game_Platform_Full FROM dbo.tbl_Game_Platform
                                SELECT TOP 0 * INTO dbo.tbl_Game_Region_Full FROM dbo.tbl_Game_Region
                                SELECT TOP 0 * INTO dbo.tbl_Game_Moderator_Full FROM dbo.tbl_Game_Moderator
                                SELECT TOP 0 * INTO dbo.tbl_Game_Ruleset_Full FROM dbo.tbl_Game_Ruleset
                                SELECT TOP 0 * INTO dbo.tbl_Game_TimingMethod_Full FROM dbo.tbl_Game_TimingMethod

                                ALTER TABLE [dbo].[tbl_Game_Full] ADD CONSTRAINT [DF_tbl_Game_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]");
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
                    db.OneTimeCommandTimeout = 32767;
                    db.Execute(@"EXEC sp_rename 'dbo.tbl_Game', 'tbl_Game_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Level', 'tbl_Level_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Category', 'tbl_Category_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Variable', 'tbl_Variable_ToRemove'
                                EXEC sp_rename 'dbo.tbl_VariableValue', 'tbl_VariableValue_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Platform', 'tbl_Game_Platform_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Region', 'tbl_Game_Region_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Moderator', 'tbl_Game_Moderator_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_Ruleset', 'tbl_Game_Ruleset_ToRemove'
                                EXEC sp_rename 'dbo.tbl_Game_TimingMethod', 'tbl_Game_TimingMethod_ToRemove'

                                EXEC sp_rename 'dbo.tbl_Game_Full', 'tbl_Game'
                                EXEC sp_rename 'dbo.tbl_Level_Full', 'tbl_Level'
                                EXEC sp_rename 'dbo.tbl_Category_Full', 'tbl_Category'
                                EXEC sp_rename 'dbo.tbl_Variable_Full', 'tbl_Variable'
                                EXEC sp_rename 'dbo.tbl_VariableValue_Full', 'tbl_VariableValue'
                                EXEC sp_rename 'dbo.tbl_Game_Platform_Full', 'tbl_Game_Platform'
                                EXEC sp_rename 'dbo.tbl_Game_Region_Full', 'tbl_Game_Region'
                                EXEC sp_rename 'dbo.tbl_Game_Moderator_Full', 'tbl_Game_Moderator'
                                EXEC sp_rename 'dbo.tbl_Game_Ruleset_Full', 'tbl_Game_Ruleset'
                                EXEC sp_rename 'dbo.tbl_Game_TimingMethod_Full', 'tbl_Game_TimingMethod'

                                DROP TABLE dbo.tbl_Game_ToRemove
                                DROP TABLE dbo.tbl_Level_ToRemove
                                DROP TABLE dbo.tbl_Category_ToRemove
                                DROP TABLE dbo.tbl_Variable_ToRemove
                                DROP TABLE dbo.tbl_VariableValue_ToRemove
                                DROP TABLE dbo.tbl_Game_Platform_ToRemove
                                DROP TABLE dbo.tbl_Game_Region_ToRemove
                                DROP TABLE dbo.tbl_Game_Moderator_ToRemove
                                DROP TABLE dbo.tbl_Game_Ruleset_ToRemove
                                DROP TABLE dbo.tbl_Game_TimingMethod_ToRemove

                                EXEC sp_rename 'dbo.DF_tbl_Game_Full_ImportedDate', 'DF_tbl_Game_ImportedDate'

                                ALTER TABLE [dbo].[tbl_Game] ADD CONSTRAINT [PK_tbl_Game] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_Game_OrderValue] ON [dbo].[tbl_Game] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Name] ON [dbo].[tbl_Game] ([Name]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Level] ADD CONSTRAINT [PK_tbl_Level] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_Level_OrderValue] ON [dbo].[tbl_Level] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Level_GameID] ON [dbo].[tbl_Level] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Category] ADD CONSTRAINT [PK_tbl_Category] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_Category_OrderValue] ON [dbo].[tbl_Category] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Category_GameID] ON [dbo].[tbl_Category] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [PK_tbl_Variable] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_Variable_OrderValue] ON [dbo].[tbl_Variable] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Variable_GameID] ON [dbo].[tbl_Variable] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_VariableValue] ADD CONSTRAINT [PK_tbl_VariableValue] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_VariableValue_OrderValue] ON [dbo].[tbl_VariableValue] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_VariableValue_GameID] ON [dbo].[tbl_VariableValue] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Game_Platform] ADD CONSTRAINT [PK_tbl_Game_Platform] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Platform_GameID] ON [dbo].[tbl_Game_Platform] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Game_Region] ADD CONSTRAINT [PK_tbl_Game_Region] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Region_GameID] ON [dbo].[tbl_Game_Region] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Game_Moderator] ADD CONSTRAINT [PK_tbl_Game_Moderator] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Moderator_GameID] ON [dbo].[tbl_Game_Moderator] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Game_Ruleset] ADD CONSTRAINT [PK_tbl_Game_Ruleset] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Ruleset_GameID] ON [dbo].[tbl_Game_Ruleset] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                ALTER TABLE [dbo].[tbl_Game_TimingMethod] ADD CONSTRAINT [PK_tbl_Game_TimingMethod] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_TimingMethod_GameID] ON [dbo].[tbl_Game_TimingMethod] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY]");

                    tran.Complete();
                }
            }
        }

        public void InsertGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods)
        {
            _logger.Information("Started InsertGames");
            int batchCount = 0;
            var gamesList = games.ToList();

            while (batchCount < gamesList.Count)
            {
                var gamesBatch = gamesList.Skip(batchCount).Take(MaxBulkRows).ToList();
                var gameIDs = gamesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var gameLinksBatch = gameLinks.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var levelsBatch = levels.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var levelIDs = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var levelRulesBatch = levelRules.Where(i => levelIDs.Contains(i.LevelSpeedRunComID)).ToList();
                var categoriesBatch = categories.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var categoryIDs = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var categoryRulesBatch = categoryRules.Where(i => categoryIDs.Contains(i.CategorySpeedRunComID)).ToList();
                var variablesBatch = variables.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var variableIDs = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var variablesValuesBatch = variableValues.Where(i => variableIDs.Contains(i.VariableSpeedRunComID)).ToList();
                var gamePlatformsBatch = gamePlatforms.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameRegionsBatch = gameRegions.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameModeratorsBatch = gameModerators.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameRulesetsBatch = gameRulesets.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameTimingMethodsBatch = gameTimingMethods.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBatch<GameEntity>(gamesBatch);

                        var gameSpeedRunComIDsBatch = gamesBatch.Select(i => new GameSpeedRunComIDEntity { GameID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<GameSpeedRunComIDEntity>(gameSpeedRunComIDsBatch);

                        gameLinksBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameLinkEntity>(gameLinksBatch);

                        levelsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<LevelEntity>(levelsBatch);

                        var levelSpeedRunComIDsBatch = levelsBatch.Select(i => new LevelSpeedRunComIDEntity { LevelID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<LevelSpeedRunComIDEntity>(levelSpeedRunComIDsBatch);

                        levelRulesBatch.ForEach(i => i.LevelID = levelsBatch.Where(g => g.SpeedRunComID == i.LevelSpeedRunComID).Select(g => g.ID).FirstOrDefault());
                        db.InsertBatch<LevelRuleEntity>(levelRulesBatch);

                        categoriesBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<CategoryEntity>(categoriesBatch);

                        var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => new CategorySpeedRunComIDEntity { CategoryID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<CategorySpeedRunComIDEntity>(categorySpeedRunComIDsBatch);

                        categoryRulesBatch.ForEach(i => i.CategoryID = categoriesBatch.Where(g => g.SpeedRunComID == i.CategorySpeedRunComID).Select(g => g.ID).FirstOrDefault());
                        db.InsertBatch<CategoryRuleEntity>(categoryRulesBatch);

                        variablesBatch.ForEach(i =>
                        {
                            i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID;
                            i.CategoryID = !string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID : (int?)null;
                            i.LevelID = !string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID : (int?)null;
                        });
                        db.InsertBatch<VariableEntity>(variablesBatch);

                        var variableSpeedRunComIDsBatch = variablesBatch.Select(i => new VariableSpeedRunComIDEntity { VariableID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableSpeedRunComIDEntity>(variableSpeedRunComIDsBatch);

                        variablesValuesBatch.ForEach(i =>
                        {
                            i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID;
                            i.VariableID = variablesBatch.Where(g => g.SpeedRunComID == i.VariableSpeedRunComID).Select(g => g.ID).FirstOrDefault();
                        });
                        db.InsertBatch<VariableValueEntity>(variablesValuesBatch);

                        var variableValueSpeedRunComIDsBatch = variablesValuesBatch.Select(i => new VariableValueSpeedRunComIDEntity { VariableValueID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableValueSpeedRunComIDEntity>(variableValueSpeedRunComIDsBatch);

                        gamePlatformsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GamePlatformEntity>(gamePlatformsBatch);

                        gameRegionsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameRegionEntity>(gameRegionsBatch);

                        gameModeratorsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameModeratorEntity>(gameModeratorsBatch);

                        gameRulesetsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameRulesetEntity>(gameRulesetsBatch);

                        gameTimingMethodsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameTimingMethodEntity>(gameTimingMethodsBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved games {@Count} / {@Total}", gamesBatch.Count, gamesList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertGames");
        }

        public void SaveGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods)
        {
            int count = 1;
            var gamesList = games.ToList();
            var gameSpeedRunComIDs = GetGameSpeedRunComIDs();

            foreach (var game in gamesList)
            {
                var gameLink = gameLinks.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                var levelsBatch = levels.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var levelIDs = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var levelRulesBatch = levelRules.Where(i => levelIDs.Contains(i.LevelSpeedRunComID)).ToList();
                var categoriesBatch = categories.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var categoryIDs = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var categoryRulesBatch = categoryRules.Where(i => categoryIDs.Contains(i.CategorySpeedRunComID)).ToList();
                var variablesBatch = variables.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var variableIDs = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var variablesValuesBatch = variableValues.Where(i => variableIDs.Contains(i.VariableSpeedRunComID)).ToList();
                var gamePlatformsBatch = gamePlatforms.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameRegionsBatch = gameRegions.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameModeratorsBatch = gameModerators.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameRuleset = gameRulesets.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                var gameTimingMethodsBatch = gameTimingMethods.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        var gameSpeedRunCom = gameSpeedRunComIDs.FirstOrDefault(i => i.SpeedRunComID == game.SpeedRunComID);
                        if (gameSpeedRunCom != null)
                        {
                            game.ModifiedDate = DateTime.Now;
                            db.DeleteWhere<GameSpeedRunComIDEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableValueSpeedRunComIDEntity>("VariableID IN (SELECT ID FROM dbo.tbl_Variable WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableValueEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableSpeedRunComIDEntity>("VariableID IN (SELECT ID FROM dbo.tbl_Variable WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<LevelSpeedRunComIDEntity>("LevelID IN (SELECT ID FROM dbo.tbl_Level WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<LevelRuleEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<LevelEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<CategorySpeedRunComIDEntity>("CategoryID IN (SELECT ID FROM dbo.tbl_Category WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<CategoryRuleEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<CategoryEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameLinkEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameRulesetEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GamePlatformEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameRegionEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameModeratorEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameTimingMethodEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                        }

                        db.Save<GameEntity>(game);

                        var gameSpeedRunComID = new GameSpeedRunComIDEntity { GameID = game.ID, SpeedRunComID = game.SpeedRunComID };
                        db.Insert<GameSpeedRunComIDEntity>(gameSpeedRunComID);

                        levelsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<LevelEntity>(levelsBatch);

                        var levelSpeedRunComIDsBatch = levelsBatch.Select(i => new LevelSpeedRunComIDEntity { LevelID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<LevelSpeedRunComIDEntity>(levelSpeedRunComIDsBatch);

                        levelRulesBatch.ForEach(i => i.LevelID = levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID);
                        db.InsertBatch<LevelRuleEntity>(levelRulesBatch);

                        categoriesBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<CategoryEntity>(categoriesBatch);

                        var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => new CategorySpeedRunComIDEntity { CategoryID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<CategorySpeedRunComIDEntity>(categorySpeedRunComIDsBatch);

                        categoryRulesBatch.ForEach(i => i.CategoryID = categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID);
                        db.InsertBatch<CategoryRuleEntity>(categoryRulesBatch);

                        variablesBatch.ForEach(i =>
                        {
                            i.GameID = game.ID;
                            i.CategoryID = !string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID : (int?)null;
                            i.LevelID = !string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID : (int?)null;
                        });
                        db.InsertBatch<VariableEntity>(variablesBatch);

                        var variableSpeedRunComIDsBatch = variablesBatch.Select(i => new VariableSpeedRunComIDEntity { VariableID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableSpeedRunComIDEntity>(variableSpeedRunComIDsBatch);

                        variablesValuesBatch.ForEach(i =>
                        {
                            i.GameID = game.ID;
                            i.VariableID = variablesBatch.Find(g => g.SpeedRunComID == i.VariableSpeedRunComID).ID;
                        });
                        db.InsertBatch<VariableValueEntity>(variablesValuesBatch);

                        var variableValueSpeedRunComIDsBatch = variablesValuesBatch.Select(i => new VariableValueSpeedRunComIDEntity { VariableValueID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableValueSpeedRunComIDEntity>(variableValueSpeedRunComIDsBatch);

                        if (gameLink != null)
                        {
                            gameLink.GameID = game.ID;
                            db.Insert<GameLinkEntity>(gameLink);
                        }

                        if (gameRuleset != null)
                        {
                            gameRuleset.GameID = game.ID;
                            db.Insert<GameRulesetEntity>(gameRuleset);
                        }

                        gamePlatformsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GamePlatformEntity>(gamePlatformsBatch);

                        gameRegionsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GameRegionEntity>(gameRegionsBatch);

                        gameModeratorsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GameModeratorEntity>(gameModeratorsBatch);

                        gameTimingMethodsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GameTimingMethodEntity>(gameTimingMethodsBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved games {@Count} / {@Total}", count, gamesList.Count);
                count++;
            }
        }

        public IEnumerable<VariableEntity> GetVariables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<VariableEntity>("SELECT OrderValue, ID, [Name], GameID, VariableScopeTypeID, CategoryID, LevelID, IsSubCategory FROM dbo.tbl_Variable WITH (NOLOCK) ORDER BY OrderValue").ToList();
            }
        }

        public IEnumerable<GameEntity> GetGames()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<GameEntity>("SELECT [OrderValue], [ID], [Name], [JapaneseName], [Abbreviation], [IsRomHack], [YearOfRelease], [SpeedRunComUrl], [CoverImageUrl], [CreatedDate], [ImportedDate], [ModifiedDate] FROM [dbo].[tbl_Game] WITH (NOLOCK) ORDER BY OrderValue").ToList();
            }
        }

        public IEnumerable<RegionSpeedRunComIDEntity> GetRegionSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<RegionSpeedRunComIDEntity>("SELECT RegionID, SpeedRunComID FROM dbo.tbl_Region_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }

        public IEnumerable<GameSpeedRunComIDEntity> GetGameSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<GameSpeedRunComIDEntity>("SELECT GameID, SpeedRunComID FROM dbo.tbl_User_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }
    }
}
