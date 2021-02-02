using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class UserEntity
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public string Name { get; set; }
        public DateTime? SignUpDate { get; set; }
        public DateTime ImportedDate { get; set; }
    }
} 
