using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunStatusEntity
    {
        public int SpeedRunID { get; set; }
        public string SpeedRunSpeedRunComID { get; set; }
        public int StatusTypeID { get; set; }
        public int? ExaminerUserID { get; set; }
        public DateTime? VerifyDate { get; set; }
    }
} 
