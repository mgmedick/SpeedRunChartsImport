using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunVariableValueEntity
    {
        public int ID { get; set; }
        public int SpeedRunID { get; set; }
        public string SpeedRunSpeedRunComID { get; set; }
        public string VariableID { get; set; }
        public string VariableValueID { get; set; }
    }
} 
