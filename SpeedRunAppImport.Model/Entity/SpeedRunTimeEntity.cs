using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunTimeEntity
    {
        public int SpeedRunID { get; set; }
        public string SpeedRunSpeedRunComID { get; set; }
        public long? PrimaryTime { get; set; }
        public long? RealTime { get; set; }
        public long? RealTimeWithoutLoads { get; set; }
        public long? GameTime { get; set; }
    }
} 
