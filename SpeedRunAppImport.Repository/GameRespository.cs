using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.IO;

namespace SpeedRunAppImport.Repository
{
    public class GameRespository : BaseRepository, IGameRepository
    {
        private readonly ILogger _logger;

        public GameRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void InsertGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods)
        {
            _logger.Information("Started InsertGames");
            int batchCount = 0;
            var gamesList = games.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                while (batchCount < gamesList.Count)
                {
                    var gamesBatch = gamesList.Skip(batchCount).Take(MaxBulkRows).ToList();
                    var gameSpeedRunComIDs = gamesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var gameLinksBatch = gameLinks.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var levelsBatch = levels.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var levelSpeedRunComIDs = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var levelRulesBatch = levelRules.Where(i => levelSpeedRunComIDs.Contains(i.LevelSpeedRunComID)).ToList();
                    var categoriesBatch = categories.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var categorySpeedRunComIDs = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var categoryRulesBatch = categoryRules.Where(i => categorySpeedRunComIDs.Contains(i.CategorySpeedRunComID)).ToList();
                    var variablesBatch = variables.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var variableSpeedRunComIDs = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var variablesValuesBatch = variableValues.Where(i => variableSpeedRunComIDs.Contains(i.VariableSpeedRunComID)).ToList();
                    var gamePlatformsBatch = gamePlatforms.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var gameRegionsBatch = gameRegions.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var gameModeratorsBatch = gameModerators.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var gameRulesetsBatch = gameRulesets.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                    var gameTimingMethodsBatch = gameTimingMethods.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();

                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBatch<GameEntity>(gamesBatch);
                        var gameIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_Game_Full ORDER BY ID DESC LIMIT @0;", gamesBatch.Count).Reverse().ToArray() :
                                                db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Game_Full ORDER BY ID DESC", gamesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < gamesBatch.Count; i++)
                        {
                            gamesBatch[i].ID = gameIDs[i];
                        }

                        var gameSpeedRunComIDsBatch = gamesBatch.Select(i => new GameSpeedRunComIDEntity { GameID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<GameSpeedRunComIDEntity>(gameSpeedRunComIDsBatch);

                        gameLinksBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameLinkEntity>(gameLinksBatch);

                        levelsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<LevelEntity>(levelsBatch);
                        var levelIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_Level_Full ORDER BY ID DESC LIMIT @0;", levelsBatch.Count).Reverse().ToArray() :
                                                 db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Level_Full ORDER BY ID DESC", levelsBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < levelsBatch.Count; i++)
                        {
                            levelsBatch[i].ID = levelIDs[i];
                        }

                        var levelSpeedRunComIDsBatch = levelsBatch.Select(i => new LevelSpeedRunComIDEntity { LevelID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<LevelSpeedRunComIDEntity>(levelSpeedRunComIDsBatch);

                        levelRulesBatch.ForEach(i => i.LevelID = levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID);
                        db.InsertBatch<LevelRuleEntity>(levelRulesBatch);

                        categoriesBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<CategoryEntity>(categoriesBatch);
                        var categoryIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_Category_Full ORDER BY ID DESC LIMIT @0;", categoriesBatch.Count).Reverse().ToArray() :
                                                    db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Category_Full ORDER BY ID DESC", categoriesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < categoriesBatch.Count; i++)
                        {
                            categoriesBatch[i].ID = categoryIDs[i];
                        }

                        var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => new CategorySpeedRunComIDEntity { CategoryID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<CategorySpeedRunComIDEntity>(categorySpeedRunComIDsBatch);

                        categoryRulesBatch.ForEach(i => i.CategoryID = categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID);
                        db.InsertBatch<CategoryRuleEntity>(categoryRulesBatch);

                        variablesBatch.ForEach(i =>
                        {
                            i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID;
                            i.CategoryID = !string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID : (int?)null;
                            i.LevelID = !string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID : (int?)null;
                        });
                        db.InsertBatch<VariableEntity>(variablesBatch);
                        var variableIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_Variable_Full ORDER BY ID DESC LIMIT @0;", variablesBatch.Count).Reverse().ToArray() :
                                                    db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Variable_Full ORDER BY ID DESC", variablesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < variablesBatch.Count; i++)
                        {
                            variablesBatch[i].ID = variableIDs[i];
                        }

                        var variableSpeedRunComIDsBatch = variablesBatch.Select(i => new VariableSpeedRunComIDEntity { VariableID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableSpeedRunComIDEntity>(variableSpeedRunComIDsBatch);

                        variablesValuesBatch.ForEach(i =>
                        {
                            i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID;
                            i.VariableID = variablesBatch.Find(g => g.SpeedRunComID == i.VariableSpeedRunComID).ID;
                        });
                        db.InsertBatch<VariableValueEntity>(variablesValuesBatch);
                        var variableValueIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_VariableValue_Full ORDER BY ID DESC LIMIT @0;", variablesValuesBatch.Count).Reverse().ToArray() :
                                                         db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_VariableValue_Full ORDER BY ID DESC", variablesValuesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < variablesValuesBatch.Count; i++)
                        {
                            variablesValuesBatch[i].ID = variableValueIDs[i];
                        }

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

                    _logger.Information("Saved games {@Count} / {@Total}", gamesBatch.Count, gamesList.Count);
                    batchCount += MaxBulkRows;
                }
            }
            _logger.Information("Completed InsertGames");
        }

        public void RemoveObsoleteGameSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                db.Execute(@"DELETE dn
                            FROM tbl_Game_SpeedRunComID dn
                            WHERE NOT EXISTS (SELECT 1 from tbl_Game where ID = dn.GameID);

                            DELETE dn
                            FROM tbl_Category_SpeedRunComID dn
                            WHERE NOT EXISTS (SELECT 1 from tbl_Category where ID = dn.CategoryID);

                            DELETE dn
                            FROM tbl_Level_SpeedRunComID dn
                            WHERE NOT EXISTS (SELECT 1 from tbl_Level where ID = dn.LevelID);

                            DELETE dn
                            FROM tbl_Variable_SpeedRunComID dn
                            WHERE NOT EXISTS (SELECT 1 from tbl_Variable where ID = dn.VariableID);

                            DELETE dn
                            FROM tbl_VariableValue_SpeedRunComID dn
                            WHERE NOT EXISTS (SELECT 1 from tbl_VariableValue where ID = dn.VariableValueID);");
            }
        }

        public void SaveGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods)
        {
            _logger.Information("Started SaveGames");
            int count = 1;
            var gamesList = games.ToList();
            var maxBatchCount = 500;
            var batchCount = 0;

            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var game in gamesList)
                {
                    _logger.Information("Saving GameID: {@GameID}, GameSpeedRunComID: {@GameSpeedRunComID}", game.ID, game.SpeedRunComID);
                    var gameLink = gameLinks.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                    var levelsBatch = levels.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                    var levelSpeedRunComIDsBatch = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var levelRulesBatch = levelRules.Where(i => levelSpeedRunComIDsBatch.Contains(i.LevelSpeedRunComID)).ToList();
                    var categoriesBatch = categories.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                    var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var categoryRulesBatch = categoryRules.Where(i => categorySpeedRunComIDsBatch.Contains(i.CategorySpeedRunComID)).ToList();
                    //var variablesBatch = variables.Where(i => i.GameSpeedRunComID == game.SpeedRunComID
                    //                                        && (string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) || categorySpeedRunComIDsBatch.Contains(i.CategorySpeedRunComID))
                    //                                        && (string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) || levelSpeedRunComIDsBatch.Contains(i.LevelSpeedRunComID)))
                    //                              .ToList();
                    var variablesBatch = variables.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                    var variableSpeedRunComIDsBatch = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var variablesValuesBatch = variableValues.Where(i => variableSpeedRunComIDsBatch.Contains(i.VariableSpeedRunComID)).ToList();
                    var gamePlatformsBatch = gamePlatforms.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                    var gameRegionsBatch = gameRegions.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                    var gameModeratorsBatch = gameModerators.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                    var gameRuleset = gameRulesets.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                    var gameTimingMethodsBatch = gameTimingMethods.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                    var gameExists = false;

                    using (var tran = db.GetTransaction())
                    {
                        try
                        {
                            if (game.ID != 0)
                            {
                                gameExists = true;
                                _logger.Information("Deleting secondary game entities");
                                game.ModifiedDate = DateTime.UtcNow;
                                game.IsChanged = null;
                                //db.DeleteWhere<GameLinkEntity>("GameID = @gameID", new { gameID = game.ID });
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteWhere<GameRulesetEntity>("GameID = @gameID", new { gameID = game.ID });
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteWhere<GamePlatformEntity>("GameID = @gameID", new { gameID = game.ID });
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteWhere<GameRegionEntity>("GameID = @gameID", new { gameID = game.ID });
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteWhere<GameModeratorEntity>("GameID = @gameID", new { gameID = game.ID });
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteWhere<GameTimingMethodEntity>("GameID = @gameID", new { gameID = game.ID });
                            }

                            _logger.Information("Saving game");
                            db.Save<GameEntity>(game);
                            db.Save<GameSpeedRunComIDEntity>(new GameSpeedRunComIDEntity { GameID = game.ID, SpeedRunComID = game.SpeedRunComID });

                            //levels
                            _logger.Information("Saving levels");
                            foreach (var level in levelsBatch)
                            {
                                level.GameID = game.ID;
                                db.Save<LevelEntity>(level);
                                db.Save<LevelSpeedRunComIDEntity>(new LevelSpeedRunComIDEntity { LevelID = level.ID, SpeedRunComID = level.SpeedRunComID });

                                var levelRule = levelRulesBatch.FirstOrDefault(i => i.LevelSpeedRunComID == level.SpeedRunComID);
                                if (levelRule != null)
                                {
                                    levelRule.LevelID = level.ID;
                                    db.Save<LevelRuleEntity>(levelRule);
                                }
                            }

                            _logger.Information("Pulling levelsToDelete");
                            var levelIDs = levelsBatch.Select(i => i.ID).ToList();
                            var levelIDsToDelete = db.Query<LevelEntity>().Where(i => i.GameID == game.ID && !levelIDs.Contains(i.ID)).ToList().Select(i => i.ID);
                            var levelRunIDsToDelete = db.Query<SpeedRunEntity>().Where(i => i.GameID == game.ID && levelIDsToDelete.Contains(i.LevelID.Value)).ToList().Select(i => i.ID);
                            var levelVariableIDsToDelete = db.Query<VariableEntity>().Where(i => i.GameID == game.ID && levelIDsToDelete.Contains(i.LevelID.Value)).ToList().Select(i => i.ID);
                            var levelVariableValueIDsToDelete = db.Query<VariableValueEntity>().Where(i => levelVariableIDsToDelete.Contains(i.VariableID)).ToList().Select(i => i.ID);

                            _logger.Information("Deleting levelsToDelete related speedruns");
                            batchCount = 0;
                            while (batchCount < levelRunIDsToDelete.Count())
                            {
                                var levelRunIDsToDeleteBatch = levelRunIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<SpeedRunVariableValueEntity>().Where(i => levelRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunPlayerEntity>().Where(i => levelRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunGuestEntity>().Where(i => levelRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunVideoEntity>().Where(i => levelRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunSpeedRunComIDEntity>().Where(i => levelRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunEntity>().Where(i => levelRunIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Deleting levelsToDelete related variablevalues");
                            batchCount = 0;
                            while (batchCount < levelVariableValueIDsToDelete.Count())
                            {
                                var levelVariableValueIDsToDeleteBatch = levelVariableValueIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<VariableValueSpeedRunComIDEntity>().Where(i => levelVariableValueIDsToDeleteBatch.Contains(i.VariableValueID)).Execute();
                                db.DeleteMany<VariableValueEntity>().Where(i => levelVariableValueIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Deleting levelsToDelete related variables");
                            if (levelVariableIDsToDelete.Any())
                            {
                                db.DeleteMany<VariableSpeedRunComIDEntity>().Where(i => levelVariableIDsToDelete.Contains(i.VariableID)).Execute();
                                db.DeleteMany<VariableEntity>().Where(i => levelVariableIDsToDelete.Contains(i.ID)).Execute();
                            }

                            _logger.Information("Deleting levelsToDelete related levels");
                            if (levelIDsToDelete.Any())
                            {
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteMany<LevelRuleEntity>().Where(i => levelIDsToDelete.Contains(i.LevelID)).Execute();
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteMany<LevelSpeedRunComIDEntity>().Where(i => levelIDsToDelete.Contains(i.LevelID)).Execute();
                                db.OneTimeCommandTimeout = 32767;
                                db.DeleteMany<LevelEntity>().Where(i => levelIDsToDelete.Contains(i.ID)).Execute();
                            }

                            //categories
                            _logger.Information("Saving categories");
                            foreach (var category in categoriesBatch)
                            {
                                category.GameID = game.ID;
                                db.Save<CategoryEntity>(category);
                                db.Save<CategorySpeedRunComIDEntity>(new CategorySpeedRunComIDEntity { CategoryID = category.ID, SpeedRunComID = category.SpeedRunComID });

                                var categoryRule = categoryRulesBatch.FirstOrDefault(i => i.CategorySpeedRunComID == category.SpeedRunComID);
                                if (categoryRule != null)
                                {
                                    categoryRule.CategoryID = category.ID;
                                    db.Save<CategoryRuleEntity>(categoryRule);
                                }
                            }

                            _logger.Information("Pulling categoriesToDelete");
                            var categoryIDs = categoriesBatch.Select(i => i.ID).ToList();
                            var categoryIDsToDelete = db.Query<CategoryEntity>().Where(i => i.GameID == game.ID && !categoryIDs.Contains(i.ID)).ToList().Select(i => i.ID).ToList();
                            var categoryRunIDsToDelete = db.Query<SpeedRunEntity>().Where(i => i.GameID == game.ID && categoryIDsToDelete.Contains(i.CategoryID)).ToList().Select(i => i.ID).ToList();
                            var categoryVariableIDsToDelete = db.Query<VariableEntity>().Where(i => i.GameID == game.ID && categoryIDsToDelete.Contains(i.CategoryID.Value)).ToList().Select(i => i.ID).ToList();
                            var categoryVariableValueIDsToDelete = db.Query<VariableValueEntity>().Where(i => categoryVariableIDsToDelete.Contains(i.VariableID)).ToList().Select(i => i.ID).ToList();

                            _logger.Information("Deleting categoriesToDelete related speedruns");
                            batchCount = 0;
                            while (batchCount < categoryRunIDsToDelete.Count())
                            {
                                var categoryRunIDsToDeleteBatch = categoryRunIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<SpeedRunVariableValueEntity>().Where(i => categoryRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunPlayerEntity>().Where(i => categoryRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunGuestEntity>().Where(i => categoryRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunVideoEntity>().Where(i => categoryRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunSpeedRunComIDEntity>().Where(i => categoryRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunEntity>().Where(i => categoryRunIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Deleting categoriesToDelete related variablevalues");
                            batchCount = 0;
                            while (batchCount < categoryVariableValueIDsToDelete.Count())
                            {
                                var categoryVariableValueIDsToDeleteBatch = categoryVariableValueIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<VariableValueSpeedRunComIDEntity>().Where(i => categoryVariableValueIDsToDeleteBatch.Contains(i.VariableValueID)).Execute();
                                db.DeleteMany<VariableValueEntity>().Where(i => categoryVariableValueIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Deleting categoriesToDelete related variables");
                            if (categoryVariableIDsToDelete.Any())
                            {
                                db.DeleteMany<VariableSpeedRunComIDEntity>().Where(i => categoryVariableIDsToDelete.Contains(i.VariableID)).Execute();
                                db.DeleteMany<VariableEntity>().Where(i => categoryVariableIDsToDelete.Contains(i.ID)).Execute();
                            }

                            _logger.Information("Deleting categoriesToDelete related categories");
                            if (categoryIDsToDelete.Any())
                            {
                                db.DeleteMany<CategoryRuleEntity>().Where(i => categoryIDsToDelete.Contains(i.CategoryID)).Execute();
                                db.DeleteMany<CategorySpeedRunComIDEntity>().Where(i => categoryIDsToDelete.Contains(i.CategoryID)).Execute();
                                db.DeleteMany<CategoryEntity>().Where(i => categoryIDsToDelete.Contains(i.ID)).Execute();
                            }

                            //variables
                            _logger.Information("Pulling variablesToDelete");
                            var variableIDs = variablesBatch.Select(i => i.ID).ToList();
                            db.OneTimeCommandTimeout = 32767;
                            var variableIDsToDelete = db.Query<VariableEntity>().Where(i => i.GameID == game.ID && (game.IsVariablesOrderChanged || !variableIDs.Contains(i.ID))).ToList().Select(i => i.ID).ToList();
                            db.OneTimeCommandTimeout = 32767;
                            var variableRunIDsToDelete = db.Query<SpeedRunVariableValueEntity>().Where(i => variableIDsToDelete.Contains(i.VariableID)).ToList().Select(i => i.SpeedRunID).Distinct().ToList();
                            db.OneTimeCommandTimeout = 32767;
                            var variableVariableValueIDsToDelete = db.Query<VariableValueEntity>().Where(i => variableIDsToDelete.Contains(i.VariableID)).ToList().Select(i => i.ID).ToList();

                            _logger.Information("Deleting variablesToDelete related speedruns");
                            batchCount = 0;
                            while (batchCount < variableRunIDsToDelete.Count())
                            {
                                var variableRunIDsToDeleteBatch = variableRunIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<SpeedRunVariableValueEntity>().Where(i => variableRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunPlayerEntity>().Where(i => variableRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunGuestEntity>().Where(i => variableRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunVideoEntity>().Where(i => variableRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunSpeedRunComIDEntity>().Where(i => variableRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunEntity>().Where(i => variableRunIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Deleting variablesToDelete related variablevalues");
                            batchCount = 0;
                            while (batchCount < variableVariableValueIDsToDelete.Count())
                            {
                                var variableVariableValueIDsToDeleteBatch = variableVariableValueIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<VariableValueSpeedRunComIDEntity>().Where(i => variableVariableValueIDsToDeleteBatch.Contains(i.VariableValueID)).Execute();
                                db.DeleteMany<VariableValueEntity>().Where(i => variableVariableValueIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Deleting variablesToDelete related variables");
                            if (variableIDsToDelete.Any())
                            {
                                db.DeleteMany<VariableSpeedRunComIDEntity>().Where(i => variableIDsToDelete.Contains(i.VariableID)).Execute();
                                db.DeleteMany<VariableEntity>().Where(i => variableIDsToDelete.Contains(i.ID)).Execute();
                            }

                            _logger.Information("Saving variables");
                            foreach (var variable in variablesBatch)
                            {
                                if (game.IsVariablesOrderChanged)
                                {
                                    variable.ID = 0;
                                }

                                variable.GameID = game.ID;
                                variable.CategoryID = !string.IsNullOrWhiteSpace(variable.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == variable.CategorySpeedRunComID).ID : (int?)null;
                                variable.LevelID = !string.IsNullOrWhiteSpace(variable.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == variable.LevelSpeedRunComID).ID : (int?)null;
                                db.Save<VariableEntity>(variable);
                                db.Save<VariableSpeedRunComIDEntity>(new VariableSpeedRunComIDEntity { VariableID = variable.ID, SpeedRunComID = variable.SpeedRunComID });
                            }

                            //variableValues
                            _logger.Information("Saving variableValues");
                            foreach (var variableValue in variablesValuesBatch)
                            {
                                if (game.IsVariablesOrderChanged)
                                {
                                    variableValue.ID = 0;
                                }

                                variableValue.GameID = game.ID;
                                variableValue.VariableID = variablesBatch.Find(g => g.SpeedRunComID == variableValue.VariableSpeedRunComID).ID;
                                db.Save<VariableValueEntity>(variableValue);
                                db.Save<VariableValueSpeedRunComIDEntity>(new VariableValueSpeedRunComIDEntity { VariableValueID = variableValue.ID, SpeedRunComID = variableValue.SpeedRunComID });
                            }

                            _logger.Information("Pulling variableValuesToDelete");
                            var variableValueIDs = variablesValuesBatch.Select(i => i.ID).ToList();
                            var variableValuesForGame = db.Query<VariableValueEntity>().Where(i => i.GameID == game.ID).ToList();
                            var variableValueIDsToDelete = variableValuesForGame.Where(i => !variableValueIDs.Contains(i.ID)).Select(i => i.ID).ToList();
                            var variableValueRunIDsToDelete = db.Query<SpeedRunVariableValueEntity>().Where(i => variableValueIDsToDelete.Contains(i.VariableValueID)).ToList().Select(i => i.SpeedRunID).Distinct().ToList();

                            _logger.Information("Deleting variableValuesToDelete related speedruns");
                            batchCount = 0;
                            while (batchCount < variableValueRunIDsToDelete.Count())
                            {
                                var variableValueRunIDsToDeleteBatch = variableValueRunIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<SpeedRunVariableValueEntity>().Where(i => variableValueRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunPlayerEntity>().Where(i => variableValueRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunGuestEntity>().Where(i => variableValueRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunVideoEntity>().Where(i => variableValueRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunSpeedRunComIDEntity>().Where(i => variableValueRunIDsToDeleteBatch.Contains(i.SpeedRunID)).Execute();
                                db.DeleteMany<SpeedRunEntity>().Where(i => variableValueRunIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Deleting variableValuesToDelete related variableValues");
                            batchCount = 0;
                            while (batchCount < variableValueIDsToDelete.Count())
                            {
                                var variableValueIDsToDeleteBatch = variableValueIDsToDelete.Skip(batchCount).Take(maxBatchCount).ToList();
                                db.DeleteMany<VariableValueSpeedRunComIDEntity>().Where(i => variableValueIDsToDeleteBatch.Contains(i.VariableValueID)).Execute();
                                db.DeleteMany<VariableValueEntity>().Where(i => variableValueIDsToDeleteBatch.Contains(i.ID)).Execute();
                                batchCount += maxBatchCount;
                            }

                            _logger.Information("Saving gameLink");
                            if (gameLink != null)
                            {
                                gameLink.GameID = game.ID;
                                if (gameExists)
                                {
                                    db.Update<GameLinkEntity>(gameLink, i => new { i.GameID, i.SpeedRunComUrl, i.CoverImageUrl });
                                }
                                else
                                {
                                    db.Insert<GameLinkEntity>(gameLink);
                                }
                            }

                            _logger.Information("Saving gameRuleset");
                            if (gameRuleset != null)
                            {
                                gameRuleset.GameID = game.ID;
                                db.Save<GameRulesetEntity>(gameRuleset);
                            }

                            _logger.Information("Saving gamePlatforms");
                            gamePlatformsBatch.ForEach(i => i.GameID = game.ID);
                            db.InsertBatch<GamePlatformEntity>(gamePlatformsBatch);

                            _logger.Information("Saving gameRegions");
                            gameRegionsBatch.ForEach(i => i.GameID = game.ID);
                            db.InsertBatch<GameRegionEntity>(gameRegionsBatch);

                            _logger.Information("Saving gameModerators");
                            gameModeratorsBatch.ForEach(i => i.GameID = game.ID);
                            db.InsertBatch<GameModeratorEntity>(gameModeratorsBatch);

                            _logger.Information("Saving gameTimingMethods");
                            gameTimingMethodsBatch.ForEach(i => i.GameID = game.ID);
                            db.InsertBatch<GameTimingMethodEntity>(gameTimingMethodsBatch);

                            _logger.Information("Completed Saving GameID: {@GameID}, GameSpeedRunComID: {@GameSpeedRunComID}", game.ID, game.SpeedRunComID);
                            tran.Complete();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "SaveGames GameID: {@GameID}, GameSpeedRunComID: {@GameSpeedRunComID}", game.ID, game.SpeedRunComID);
                        }
                    }

                    _logger.Information("Saved games {@Count} / {@Total}", count, gamesList.Count);
                    count++;
                }
            }
        }

        public void UpdateGameCoverImages(IEnumerable<GameLinkEntity> gameLinks)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var gameLink in gameLinks)
                {
                    try
                    {
                        db.Update<GameLinkEntity>(gameLink, i => new { i.CoverImagePath });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "UpdateGameCoverImages GameID: {@GameID}", gameLink.GameID);
                    }
                }
            }
        }

        public IEnumerable<GameEntity> GetGames(Expression<Func<GameEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<GameEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<SitemapEntity> GetGamesForSitemap()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                var results = IsMySQL ? db.Query<SitemapEntity>("CALL ImportGetGamesForSitemap;").ToList() :
                                        db.Query<SitemapEntity>("EXEC dbo.ImportGetGamesForSitemap").ToList();

                return results;
            }
        }

        public IEnumerable<GameSpeedRunComView> GetGameSpeedRunComViews(Expression<Func<GameSpeedRunComView, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<GameSpeedRunComView>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<GameSpeedRunComIDEntity> GetGameSpeedRunComIDs(Expression<Func<GameSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<GameSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<CategorySpeedRunComIDEntity> GetCategorySpeedRunComIDs(Expression<Func<CategorySpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<CategorySpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<LevelSpeedRunComIDEntity> GetLevelSpeedRunComIDs(Expression<Func<LevelSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<LevelSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<VariableSpeedRunComIDEntity> GetVaraibleSpeedRunComIDs(Expression<Func<VariableSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<VariableSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<VariableValueSpeedRunComIDEntity> GetVariableValueSpeedRunComIDs(Expression<Func<VariableValueSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<VariableValueSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<RegionSpeedRunComIDEntity> GetRegionSpeedRunComIDs(Expression<Func<RegionSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<RegionSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }
    }
}
