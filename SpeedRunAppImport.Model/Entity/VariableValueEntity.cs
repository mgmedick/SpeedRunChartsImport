using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class VariableValueEntity
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public int GameID { get; set; }
        public string GameSpeedRunComID { get; set; }
        public int VariableID { get; set; }
        public string VariableSpeedRunComID { get; set; }
        public string Value { get; set; }
        public bool IsCustomValue { get; set; }
    }
} 
