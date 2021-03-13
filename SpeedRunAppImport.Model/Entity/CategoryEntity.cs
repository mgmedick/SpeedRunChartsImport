using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class CategoryEntity
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public string Name { get; set; }
        public int GameID { get; set; }
        public string GameSpeedRunComID { get; set; }
        public int CategoryTypeID { get; set; }
    }
}

