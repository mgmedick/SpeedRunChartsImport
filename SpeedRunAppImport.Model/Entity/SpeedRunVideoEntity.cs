using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunVideoEntity
    {
        public int ID { get; set; }
        public int SpeedRunID { get; set; }
        public string SpeedRunSpeedRunComID { get; set; }
        public string VideoLinkUrl { get; set; }
        public string EmbeddedVideoLinkUrl { get; set; }
        public string ThumbnailLinkUrl { get; set; }
        public int? ViewCount { get; set; }
        public Uri VideoLinkUri { get; set; }
        public string VideoID { get; set; }
    }
} 
