using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class SpeedRunVideoView
    {
        public int SpeedRunID { get; set; }
        public int SpeedRunVideoID { get; set; }
        public string VideoLinkUrl { get; set; }
        public string ThumbnailLinkUrl { get; set; }
        public string EmbeddedVideoLinkUrl { get; set; }
        public DateTime? VerifyDate { get; set; }
        public long? ViewCount { get; set; }
        public string ChannelCode { get; set; }
        public bool HasDetails { get; set; }
    }
} 
