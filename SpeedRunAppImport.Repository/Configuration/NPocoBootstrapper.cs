using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
using NPoco.FluentMappings;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace SpeedRunAppImport.Repository.Configuration
{
    public static class NPocoBootstrapper
    {
        public static void Configure(string connectionString, int maxBulkRows, bool isBulkReload, bool isMySQL)
        {
            var fluentConfig = FluentMappingConfiguration.Configure(new Repository.DataMappings(isBulkReload));

            BaseRepository.DBFactory = DatabaseFactory.Config(i =>
            {
                i.UsingDatabase(() => isMySQL ? new Database(connectionString, DatabaseType.MySQL, MySqlClientFactory.Instance, System.Data.IsolationLevel.ReadUncommitted) :
                                                new Database(connectionString, DatabaseType.SqlServer2012, SqlClientFactory.Instance, System.Data.IsolationLevel.ReadUncommitted));
                i.WithFluentConfig(fluentConfig);                
            });

            BaseRepository.MaxBulkRows = maxBulkRows;
            BaseRepository.IsMySQL = isMySQL;
        }
    }
}
