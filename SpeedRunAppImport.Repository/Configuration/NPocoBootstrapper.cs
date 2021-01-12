using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
using NPoco.FluentMappings;
//using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace SpeedRunAppImport.Repository.Configuration
{
    public static class NPocoBootstrapper
    {
        public static void Configure(string connectionString, int maxBulkRows, bool isFullImport)
        {
            var fluentConfig = FluentMappingConfiguration.Configure(new Repository.DataMappings(isFullImport));

            BaseRepository.DBFactory = DatabaseFactory.Config(i =>
            {
                //var db = new Database(connectionString, DatabaseType.SqlServer2012, SqlClientFactory.Instance);
                //db.CommandTimeout = 0;
                i.UsingDatabase(() => new Database(connectionString, DatabaseType.SqlServer2012, SqlClientFactory.Instance));
                i.WithFluentConfig(fluentConfig);
                
            });

            BaseRepository.MaxBulkRows = maxBulkRows;
        }
    }
}
