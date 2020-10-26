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
            For<GameEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game");
            For<LevelEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Level");
            For<CategoryEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Category");
            For<VariableEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Variable");
            For<GamePlatformEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Platform");
            For<GameRegionEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Region");
            For<SettingEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Setting");
        } 
    }
}



