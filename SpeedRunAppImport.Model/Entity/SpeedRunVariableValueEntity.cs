﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunVariableValueEntity
    {
        public int ID { get; set; }
        public int SpeedRunID { get; set; }
        public string SpeedRunSpeedRunComID { get; set; }
        public int VariableID { get; set; }
        public int VariableValueID { get; set; }
    }
} 
