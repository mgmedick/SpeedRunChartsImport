using System;
using System.Collections.Generic;
using System.Text;
using NPoco.FluentMappings;
using SpeedRunApp.Model.Entity;

namespace SpeedRunAppImport.Repository
{
    public class DataMappings : Mappings
    {
        public DataMappings()
        {
            For<GameEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game");
        }
    }
}



