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
        void InsertGames(IEnumerable<GameEntity> games, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> CategoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods);
        void SaveGames(IEnumerable<GameEntity> games, IEnumerable<LevelEntity> levels, IEnumerable<CategoryEntity> categories, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods);
        IEnumerable<VariableEntity> GetVariables();
        IEnumerable<GameEntity> GetGames();
        IEnumerable<GameSpeedRunComIDEntity> GetGameSpeedRunComIDs();
        IEnumerable<RegionSpeedRunComIDEntity> GetRegionSpeedRunComIDs();
    }
}






