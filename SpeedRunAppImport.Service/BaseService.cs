using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunApp.Client;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace SpeedRunAppImport.Service
{
    public abstract class BaseService
    {
        public static int MaxElementsPerPage { get; set; }
        public static int MaxRetryCount { get; set; }
        public static int PullDelayMS { get; set; }
        public static bool IsFullImport { get; set; }
    }
}


