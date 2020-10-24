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
            For<LevelEntity>().PrimaryKey("ID").TableName("dbo.tbl_Level");
            For<CategoryEntity>().PrimaryKey("ID").TableName("dbo.tbl_Category");
            For<VariableEntity>().PrimaryKey("ID").TableName("dbo.tbl_Variable");
            For<GamePlatformEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game_Platform");
            For<GameRegionEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game_Region");
            For<SettingEntity>().PrimaryKey("ID").TableName("dbo.tbl_Setting");
        }
    }
}



