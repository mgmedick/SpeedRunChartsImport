using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class UserNameStyleEntity
    {
        public int UserID { get; set; }
        public string UserSpeedRunComID { get; set; }
        public bool IsGradient { get; set; }
        public string ColorLight { get; set; }
        public string ColorDark { get; set; }
        public string ColorToLight { get; set; }
        public string ColorToDark { get; set; }
    }
} 
