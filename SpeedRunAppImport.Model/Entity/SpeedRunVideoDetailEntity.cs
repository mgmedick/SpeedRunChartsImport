using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunVideoDetailEntity
    {
        public int SpeedRunVideoID { get; set; }
        public int SpeedRunVideoLocalID { get; set; }
        public int SpeedRunID { get; set; }
        public string ChannelCode { get; set; }
        public long? ViewCount { get; set; }
        public string ThumbnailLinkUrl { get; set; }
    }
} 
