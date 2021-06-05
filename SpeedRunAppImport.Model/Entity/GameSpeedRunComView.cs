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
        public string CategorySpeedRunComIDs { get; set; }
        public string LevelSpeedRunComIDs { get; set; }
        public string VariableSpeedRunComIDs { get; set; }
        public string VariableValueSpeedRunComIDs { get; set; }
        public string PlatformSpeedRunComIDs { get; set; }
        public string ModeratorSpeedRunComIDs { get; set; }
    }
} 
