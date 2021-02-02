using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class PlatformEntity
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public string Name { get; set; }
        public int YearOfRelease { get; set; }
        public DateTime ImportedDate { get; set; }
    }
} 
