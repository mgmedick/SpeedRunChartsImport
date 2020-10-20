using System;
using System.Collections.Generic;
using System.Text;
using NPoco.FluentMappings;
using SpeedRunApp.Model.Data;

namespace SpeedRunAppImport.Repository
{
    public class DataMappings : Mappings
    {
        public DataMappings()
        {
            For<Game>().PrimaryKey("ID").TableName("dbo.tbl_Game");
        }
    }
}



