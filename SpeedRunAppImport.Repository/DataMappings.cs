using System;
using System.Collections.Generic;
using System.Text;
using NPoco.FluentMappings;
using SpeedRunApp.Model.Entity;

namespace SpeedRunAppImport.Repository
{
    public class DataMappings : Mappings
    {
        public DataMappings(bool isFullImport)
        {
            For<SettingEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Setting");

            if (isFullImport)
            {
                For<GameEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Full");
                For<LevelEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Level_Full");
                For<CategoryEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Category_Full");
                For<VariableEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Variable_Full");
                For<GamePlatformEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Platform_Full");
                For<GameRegionEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Region_Full");
                For<UserEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_User_Full");
            }
            else
            {
                For<GameEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game");
                For<LevelEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Level");
                For<CategoryEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Category");
                For<VariableEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Variable");
                For<GamePlatformEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Platform");
                For<GameRegionEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Region");
                For<UserEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_User");
            }
        }  
    }
}



