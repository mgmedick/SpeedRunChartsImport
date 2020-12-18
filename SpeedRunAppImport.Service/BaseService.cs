using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Threading;

namespace SpeedRunAppImport.Service
{
    public abstract class BaseService
    {
        public static int MaxElementsPerPage { get; set; }
        public static int MaxRetryCount { get; set; }
        public static long MaxMemorySizeBytes { get; set; }
        public static int PullDelayMS { get; set; }
        public static int ErrorPullDelayMS { get; set; }
        public static int RejectedDaysBack { get; set; }
        public static bool IsFullImport { get; set; }
        public static DateTime SqlMinDateTime { get; set; }
    }
}


