using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class GameLinkEntity
    {
        public int GameID { get; set; }
        public string GameSpeedRunComID { get; set; }
        public string SpeedRunComUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public string TempCoverImagePath { get; set; }
        public string LocalCoverImagePath { get; set; }
    }
} 
