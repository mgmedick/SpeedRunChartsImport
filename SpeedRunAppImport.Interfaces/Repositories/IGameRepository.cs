using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IGameRepository
    {
        void CopyGameTables();
        void RenameAndDropGameTables();
        void InsertGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods);
        void SaveGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods);
        //IEnumerable<VariableEntity> GetVariables();
        //IEnumerable<GameEntity> GetGames();
        IEnumerable<GameSpeedRunComIDEntity> GetGameSpeedRunComIDs();
        IEnumerable<GameSpeedRunComIDEntity> GetGameSpeedRunComIDs(Expression<Func<GameSpeedRunComIDEntity, bool>> predicate = null);
        IEnumerable<CategorySpeedRunComIDEntity> GetCategorySpeedRunComIDs(Expression<Func<CategorySpeedRunComIDEntity, bool>> predicate = null);
        IEnumerable<CategorySpeedRunComIDEntity> GetCategorySpeedRunComIDs();
        IEnumerable<LevelSpeedRunComIDEntity> GetLevelSpeedRunComIDs();
        IEnumerable<LevelSpeedRunComIDEntity> GetLevelSpeedRunComIDs(Expression<Func<LevelSpeedRunComIDEntity, bool>> predicate = null);
        IEnumerable<VariableSpeedRunComIDEntity> GetVariableSpeedRunComIDs();
        IEnumerable<VariableSpeedRunComIDEntity> GetVaraibleSpeedRunComIDs(Expression<Func<VariableSpeedRunComIDEntity, bool>> predicate = null);
        IEnumerable<VariableValueSpeedRunComIDEntity> GetVariableValueSpeedRunComIDs();
        IEnumerable<VariableValueSpeedRunComIDEntity> GetVariableValueSpeedRunComIDs(Expression<Func<VariableValueSpeedRunComIDEntity, bool>> predicate = null);
        IEnumerable<RegionSpeedRunComIDEntity> GetRegionSpeedRunComIDs();
        IEnumerable<RegionSpeedRunComIDEntity> GetRegionSpeedRunComIDs(Expression<Func<RegionSpeedRunComIDEntity, bool>> predicate = null);
    }
}






