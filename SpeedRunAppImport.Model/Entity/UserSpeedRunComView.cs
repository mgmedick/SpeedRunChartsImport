using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunAppImport.Model.Entity
{
    public class UserSpeedRunComView
    {
        public int ID { get; set; }
        public string SpeedRunComID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string SpeedRunComUrl { get; set; }
        public string ProfileImageUrl { get; set; }
        public string TwitchProfileUrl { get; set; }
        public string HitboxProfileUrl { get; set; }
        public string YoutubeProfileUrl { get; set; }
        public string TwitterProfileUrl { get; set; }
        public bool? IsChanged { get; set; }
    }
} 
