﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class GameModeratorEntity
    {
        public int ID { get; set; }
        public int GameID { get; set; }
        public string GameSpeedRunComID { get; set; }
        public int UserID { get; set; }
        public string UserSpeedRunComID { get; set; }
    }
} 
