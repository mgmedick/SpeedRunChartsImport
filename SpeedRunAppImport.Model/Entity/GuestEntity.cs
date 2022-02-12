using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class GuestEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Abbr { get; set; }
        public DateTime ImportedDate { get; set; }
    }
} 
