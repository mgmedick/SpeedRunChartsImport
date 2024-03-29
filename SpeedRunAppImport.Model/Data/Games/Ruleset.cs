﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace SpeedRunAppImport.Model.Data
{
    public class Ruleset
    {
        public bool ShowMilliseconds { get; set; }
        public bool RequiresVerification { get; set; }
        public bool RequiresVideo { get; set; }
        public IEnumerable<TimingMethod> TimingMethods { get; set; }
        public TimingMethod DefaultTimingMethod { get; set; }
        public bool EmulatorsAllowed { get; set; }

        //public Ruleset() { }

        /*
        public static Ruleset Parse(SpeedrunComClient client, dynamic rulesetElement)
        {
            var ruleset = new Ruleset();

            var properties = rulesetElement.Properties as IDictionary<string, dynamic>;

            ruleset.ShowMilliseconds = properties["show-milliseconds"];
            ruleset.RequiresVerification = properties["require-verification"];
            ruleset.RequiresVideo = properties["require-video"];

            Func<dynamic, TimingMethod> timingMethodParser = x => TimingMethodHelpers.FromString(x as string);
            ruleset.TimingMethods = client.ParseCollection(properties["run-times"], timingMethodParser);
            ruleset.DefaultTimingMethod = TimingMethodHelpers.FromString(properties["default-time"]);

            ruleset.EmulatorsAllowed = properties["emulators-allowed"];

            return ruleset;
        }
        */

        public override string ToString()
        {
            var list = new List<string>();
            if (ShowMilliseconds)
                list.Add("Show Milliseconds");
            if (RequiresVerification)
                list.Add("Requires Verification");
            if (RequiresVideo)
                list.Add("Requires Video");
            if (EmulatorsAllowed)
                list.Add("Emulators Allowed");
            if (!list.Any())
                list.Add("No Rules");

            return string.Join(",", list);//.Aggregate(", ");
        }
    }
}
