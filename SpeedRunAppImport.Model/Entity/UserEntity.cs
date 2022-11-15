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
        public int UserRoleID { get; set; }
        public string Abbr { get; set; }
        public DateTime? SignUpDate { get; set; }
        public DateTime ImportedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsChanged { get; set; }
    }
} 
