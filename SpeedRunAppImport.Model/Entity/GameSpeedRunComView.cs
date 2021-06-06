using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class GameSpeedRunComView
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public string Name { get; set; }
        public bool IsRomHack { get; set; }
        public int? YearOfRelease { get; set; }
        public string CoverImageUrl { get; set; }
        public string CategorySpeedRunComIDs { get; set; }
        public string LevelSpeedRunComIDs { get; set; }
        public string VariableSpeedRunComIDs { get; set; }
        public string VariableValueSpeedRunComIDs { get; set; }
        public string PlatformSpeedRunComIDs { get; set; }
        public string ModeratorSpeedRunComIDs { get; set; }

        //ignore
        public string[] CategorySpeedRunComIDArray { get { return string.IsNullOrWhiteSpace(CategorySpeedRunComIDs) ? new string[0] : CategorySpeedRunComIDs.Split(","); } }
        public string[] LevelSpeedRunComIDArray { get { return string.IsNullOrWhiteSpace(LevelSpeedRunComIDs) ? new string[0] : LevelSpeedRunComIDs.Split(","); } }
        public string[] VariableSpeedRunComIDArray { get { return string.IsNullOrWhiteSpace(VariableSpeedRunComIDs) ? new string[0] : VariableSpeedRunComIDs.Split(","); } }
        public string[] VariableValueSpeedRunComIDArray { get { return string.IsNullOrWhiteSpace(VariableValueSpeedRunComIDs) ? new string[0] : VariableValueSpeedRunComIDs.Split(","); } }
        public string[] PlatformSpeedRunComIDArray { get { return string.IsNullOrWhiteSpace(PlatformSpeedRunComIDs) ? new string[0] : PlatformSpeedRunComIDs.Split(","); } }
        public string[] ModeratorSpeedRunComIDArray { get { return string.IsNullOrWhiteSpace(ModeratorSpeedRunComIDs) ? new string[0] : ModeratorSpeedRunComIDs.Split(","); } }
    }
} 
