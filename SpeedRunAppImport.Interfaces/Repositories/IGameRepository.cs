using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IGameRepository
    {
        void TruncateGameDetails();
        void InsertGameDetails(IEnumerable<GameEntity> games, IEnumerable<LevelEntity> levels, IEnumerable<CategoryEntity> categories, IEnumerable<VariableEntity> variables, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions);
    }
}




