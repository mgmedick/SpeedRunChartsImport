using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunSystemEntity
    {
        public int SpeedRunID { get; set; }
        public string SpeedRunSpeedRunComID { get; set; }
        public int PlatformID { get; set; }
        public string PlatformSpeedRunComID { get; set; }
        public int RegionID { get; set; }
        public string RegionSpeedRunComID { get; set; }
        public bool IsEmulated { get; set; }
    }
} 
