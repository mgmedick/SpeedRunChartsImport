using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class UserImageEntity
    {
        public int UserID { get; set; }
        public string UserSpeedRunComID { get; set; }
        public byte[] ProfileImage { get; set; }
    }
} 
