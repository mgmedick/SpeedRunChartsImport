using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class GameRegionEntity
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public int GameID { get; set; }
        public string GameSpeedRunComID { get; set; }
        public int RegionID { get; set; }
    }
} 
