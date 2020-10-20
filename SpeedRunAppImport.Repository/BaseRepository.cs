using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace SpeedRunAppImport.Repository
{
    public abstract class BaseRepository
    {
        //private readonly IConfiguration _config = null;
        //private readonly string connectionString = null;

        //public BaseRepository(IConfiguration Configuration)
        //{
        //    _config = Configuration;
        //    connectionString = _config.GetSection("ConnectionStrings").GetSection("DBConnectionString").Value;
        //}

        //public IDatabase Connection
        //{
        //    get
        //    {
        //        return new Database(connectionString, DatabaseType.SqlServer2008, SqlClientFactory.Instance);
        //    }
        //}

        public static DatabaseFactory DBFactory { get; set; }
    }
}


