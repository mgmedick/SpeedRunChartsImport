using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class UserLinkEntity
    {
        public int UserID { get; set; }
        public string UserSpeedRunComID { get; set; }
        public string SpeedRunComUrl { get; set; }
        public string ProfileImageUrl { get; set; }
        public string TwitchProfileUrl { get; set; }
        public string HitboxProfileUrl { get; set; }
        public string YoutubeProfileUrl { get; set; }
        public string TwitterProfileUrl { get; set; }
        //TempProfileImagePath
        public string TempProfileImagePath { get; set; }
        public string LocalProfileImagePath { get; set; }
    }
} 
