using System;
using System.Collections.Generic;
using System.Text;
using NPoco.FluentMappings;
using SpeedRunAppImport.Model.Entity;

namespace SpeedRunAppImport.Repository
{
    public class DataMappings : Mappings
    {
        public DataMappings(bool isFullImport)
        {
            For<SettingEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Setting");
            For<GameView>().PrimaryKey("ID", false).TableName("dbo.vw_Game");

            if (isFullImport)
            {
                For<GameEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<LevelEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Level_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<CategoryEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Category_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<VariableEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Variable_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<VariableValueEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_VariableValue_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<GamePlatformEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Platform_Full");
                For<GameRegionEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Region_Full");
                For<GameModeratorEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Moderator_Full");
                For<GameRulesetEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Ruleset_Full");
                For<GameTimingMethodEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_TimingMethod_Full");
                For<UserEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_User_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<SpeedRunEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_SpeedRun_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<SpeedRunVariableValueEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_VariableValue_Full");
                For<SpeedRunPlayerEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_Player_Full");
                For<SpeedRunVideoEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_Video_Full");
                For<LeaderboardEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Leaderboard_Full").Columns(i => {
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<PlatformEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Platform_Full").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
            }
            else
            {
                For<GameEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<LevelEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Level").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<CategoryEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Category").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<VariableEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Variable").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<VariableValueEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_VariableValue").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                });
                For<GamePlatformEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Platform");
                For<GameRegionEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Region");
                For<GameModeratorEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Moderator");
                For<GameRulesetEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Ruleset");
                For<GameTimingMethodEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_TimingMethod");
                For<UserEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_User").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<SpeedRunEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_SpeedRun").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<SpeedRunVariableValueEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_VariableValue");
                For<SpeedRunPlayerEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_Player");
                For<SpeedRunVideoEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_Video");
                For<LeaderboardEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Leaderboard").Columns(i => {
                    i.Column(g => g.ImportedDate).Ignore();
                });
                For<PlatformEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Platform").Columns(i => {
                    i.Column(g => g.OrderValue).Ignore();
                    i.Column(g => g.ImportedDate).Ignore();
                });
            }
        }  
    }
}



