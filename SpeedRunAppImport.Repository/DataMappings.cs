using System;
using System.Collections.Generic;
using System.Text;
using NPoco.FluentMappings;
using SpeedRunAppImport.Model.Entity;

namespace SpeedRunAppImport.Repository
{
    public class DataMappings : Mappings
    {
        public DataMappings(bool isBulkReload)
        {
            List<IMap> blahs = new List<IMap>();
            For<SettingEntity>().PrimaryKey("ID").TableName("dbo.tbl_Setting");
            For<SettingEntity>().PrimaryKey("ID").TableName("dbo.tbl_Setting");
            For<GameView>().PrimaryKey("ID").TableName("dbo.vw_Game");
            
            //region
            For<RegionSpeedRunComIDEntity>().PrimaryKey("RegionID", false).TableName("dbo.tbl_Region_SpeedRunComID");

            string tblEnd = (isBulkReload ? "_Full" : string.Empty);
            //user
            For<UserEntity>().PrimaryKey("ID").TableName("dbo.tbl_User" + tblEnd).Columns(i =>
            {
                i.Column(g => g.ImportedDate).Ignore();
                i.Column(g => g.SpeedRunComID).Ignore();
            });
            For<UserSpeedRunComIDEntity>().PrimaryKey("UserID", false).TableName("dbo.tbl_User_SpeedRunComID" + tblEnd);
            For<UserLocationEntity>().PrimaryKey("UserID", false).TableName("dbo.tbl_User_Location" + tblEnd).Columns(i =>
            {
                i.Column(g => g.UserSpeedRunComID).Ignore();
            });
            For<UserLinkEntity>().PrimaryKey("UserID", false).TableName("dbo.tbl_User_Link" + tblEnd).Columns(i =>
            {
                i.Column(g => g.UserSpeedRunComID).Ignore();
            });

            //platform
            For<PlatformEntity>().PrimaryKey("ID").TableName("dbo.tbl_Platform" + tblEnd).Columns(i =>
            {
                i.Column(g => g.ImportedDate).Ignore();
                i.Column(g => g.SpeedRunComID).Ignore();
            });
            For<PlatformSpeedRunComIDEntity>().PrimaryKey("PlatformID", false).TableName("dbo.tbl_Platform_SpeedRunComID" + tblEnd);

            //game
            For<GameEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game" + tblEnd).Columns(i =>
            {
                i.Column(g => g.ImportedDate).Ignore();
                i.Column(g => g.SpeedRunComID).Ignore();
            });
            For<GameSpeedRunComIDEntity>().PrimaryKey("GameID", false).TableName("dbo.tbl_Game_SpeedRunComID" + tblEnd);
            For<GameLinkEntity>().PrimaryKey("GameID", false).TableName("dbo.tbl_Game_Link" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<LevelEntity>().PrimaryKey("ID").TableName("dbo.tbl_Level" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<LevelSpeedRunComIDEntity>().PrimaryKey("LevelID", false).TableName("dbo.tbl_Level_SpeedRunComID" + tblEnd);
            For<LevelRuleEntity>().PrimaryKey("LevelID", false).TableName("dbo.tbl_Level_Rule" + tblEnd).Columns(i =>
            {
                i.Column(g => g.LevelSpeedRunComID).Ignore();
            });
            For<CategoryEntity>().PrimaryKey("ID").TableName("dbo.tbl_Category" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<CategorySpeedRunComIDEntity>().PrimaryKey("CategoryID", false).TableName("dbo.tbl_Category_SpeedRunComID" + tblEnd);
            For<CategoryRuleEntity>().PrimaryKey("CategoryID", false).TableName("dbo.tbl_Category_Rule" + tblEnd).Columns(i =>
            {
                i.Column(g => g.CategorySpeedRunComID).Ignore();
            });
            For<VariableEntity>().PrimaryKey("ID").TableName("dbo.tbl_Variable" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
                i.Column(g => g.CategorySpeedRunComID).Ignore();
                i.Column(g => g.LevelSpeedRunComID).Ignore();
            });
            For<VariableSpeedRunComIDEntity>().PrimaryKey("VariableID", false).TableName("dbo.tbl_Variable_SpeedRunComID" + tblEnd);
            For<VariableValueEntity>().PrimaryKey("ID").TableName("dbo.tbl_VariableValue" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
                i.Column(g => g.VariableSpeedRunComID).Ignore();
            });
            For<VariableValueSpeedRunComIDEntity>().PrimaryKey("VariableValueID", false).TableName("dbo.tbl_VariableValue_SpeedRunComID" + tblEnd);
            For<GamePlatformEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game_Platform" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameRegionEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game_Region" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameModeratorEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game_Moderator" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameRulesetEntity>().PrimaryKey("GameID", false).TableName("dbo.tbl_Game_Ruleset" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameTimingMethodEntity>().PrimaryKey("ID").TableName("dbo.tbl_Game_TimingMethod" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });

            //speedrun
            For<SpeedRunEntity>().PrimaryKey("ID").TableName("dbo.tbl_SpeedRun" + tblEnd).Columns(i =>
            {
                i.Column(g => g.ImportedDate).Ignore();
                i.Column(g => g.SpeedRunComID).Ignore();
            });
            For<SpeedRunSpeedRunComIDEntity>().PrimaryKey("SpeedRunID", false).TableName("dbo.tbl_SpeedRun_SpeedRunComID" + tblEnd);
            For<SpeedRunLinkEntity>().PrimaryKey("SpeedRunID", false).TableName("dbo.tbl_SpeedRun_Link" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
            For<SpeedRunSystemEntity>().PrimaryKey("SpeedRunID", false).TableName("dbo.tbl_SpeedRun_System" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
            For<SpeedRunTimeEntity>().PrimaryKey("SpeedRunID", false).TableName("dbo.tbl_SpeedRun_Time" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
            For<SpeedRunCommentEntity>().PrimaryKey("SpeedRunID", false).TableName("dbo.tbl_SpeedRun_Comment" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
            For<SpeedRunVariableValueEntity>().PrimaryKey("ID").TableName("dbo.tbl_SpeedRun_VariableValue" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
            For<SpeedRunPlayerEntity>().PrimaryKey("ID").TableName("dbo.tbl_SpeedRun_Player" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
            For<SpeedRunVideoEntity>().PrimaryKey("ID").TableName("dbo.tbl_SpeedRun_Video" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
        }
    }
}



