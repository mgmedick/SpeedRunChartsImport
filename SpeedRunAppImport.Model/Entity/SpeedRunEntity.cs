using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunEntity
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public int GameID { get; set; }
        public int CategoryID { get; set; }
        public int? LevelID { get; set; }
        //public string VariableValues { get; set; }
        //public string SubCategoryVariableValues { get; set; }
        //public string PlayerIDs { get; set; }
        //public string PlatformID { get; set; }
        //public string RegionID { get; set; }
        //public bool IsEmulated { get; set; }
        public int? Rank { get; set; }
        public long? PrimaryTime { get; set; }
        //public long? RealTime { get; set; }
        //public long? RealTimeWithoutLoads { get; set; }
        //public long? GameTime { get; set; }
        //public string Comment { get; set; }
        //public string RejectReason { get; set; }
        //public string PrimaryVideoLinkUrl { get; set; }
        //public string SpeedRunComUrl { get; set; }
        //public string SplitsUrl { get; set; }
        public DateTime? RunDate { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime ImportedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
} 
