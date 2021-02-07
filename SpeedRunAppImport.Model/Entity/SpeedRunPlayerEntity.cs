using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunPlayerEntity
    {
        public int ID { get; set; }
        public int SpeedRunID { get; set; }
        public string SpeedRunSpeedRunComID { get; set; }
        public bool IsUser { get; set; }
        public int? UserID { get; set; }
        public string UserSpeedRunComID { get; set; }
        public string GuestName { get; set; }
    }
} 
