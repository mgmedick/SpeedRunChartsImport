using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class GameImageEntity
    {
        public int GameID { get; set; }
        public string GameSpeedRunComID { get; set; }
        public byte[] CoverImage { get; set; }
    }
} 
