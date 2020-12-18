using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
//using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace SpeedRunAppImport.Repository
{
    public abstract class BaseRepository
    {
        public static DatabaseFactory DBFactory { get; set; }
        public static int MaxBulkRows { get; set; }
        public static bool FullImport { get; set; }
    }
} 


