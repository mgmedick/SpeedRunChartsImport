using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class VariableEntity
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public string Name { get; set; }
        public int GameID { get; set; }
        public string GameSpeedRunComID { get; set; }
        public int VariableScopeTypeID { get; set; }
        public int? CategoryID { get; set; }
        public string CategorySpeedRunComID { get; set; }
        public int? LevelID { get; set; }
        public string LevelSpeedRunComID { get; set; }
        public bool IsSubCategory { get; set; }
    }
} 
