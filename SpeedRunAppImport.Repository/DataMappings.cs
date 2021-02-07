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
            List<IMap> blahs = new List<IMap>();
            For<SettingEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Setting");
            For<SettingEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Setting");
            For<GameView>().PrimaryKey("ID", false).TableName("dbo.vw_Game");

            string tblEnd = (isFullImport ? "_Full" : string.Empty);
            //user
            For<UserEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_User" + tblEnd).Columns(i =>
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
            For<PlatformEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Platform" + tblEnd).Columns(i =>
            {
                i.Column(g => g.ImportedDate).Ignore();
                i.Column(g => g.SpeedRunComID).Ignore();
            });
            For<PlatformSpeedRunComIDEntity>().PrimaryKey("PlatformID", false).TableName("dbo.tbl_Platform_SpeedRunComID" + tblEnd);

            //game
            For<GameEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Game" + tblEnd).Columns(i =>
            {
                i.Column(g => g.ImportedDate).Ignore();
                i.Column(g => g.SpeedRunComID).Ignore();
            });
            For<GameSpeedRunComIDEntity>().PrimaryKey("GameID", false).TableName("dbo.tbl_Game_SpeedRunComID" + tblEnd);
            For<GameLinkEntity>().PrimaryKey("GameID", false).TableName("dbo.tbl_Game_Link" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<LevelEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Level" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<CategoryEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Category" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<VariableEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_Variable" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
                i.Column(g => g.CategorySpeedRunComID).Ignore();
                i.Column(g => g.LevelSpeedRunComID).Ignore();
            });
            For<VariableValueEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_VariableValue" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
                i.Column(g => g.VariableSpeedRunComID).Ignore();
            });
            For<GamePlatformEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Platform" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameRegionEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Region" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameModeratorEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_Moderator" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameRulesetEntity>().PrimaryKey("GameID", true).TableName("dbo.tbl_Game_Ruleset" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });
            For<GameTimingMethodEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_Game_TimingMethod" + tblEnd).Columns(i =>
            {
                i.Column(g => g.GameSpeedRunComID).Ignore();
            });

            //speedrun
            For<SpeedRunEntity>().PrimaryKey("ID", false).TableName("dbo.tbl_SpeedRun" + tblEnd).Columns(i =>
            {
                i.Column(g => g.ImportedDate).Ignore();
                i.Column(g => g.SpeedRunComID).Ignore();
                i.Column(g => g.GameSpeedRunComID).Ignore();
                i.Column(g => g.CategorySpeedRunComID).Ignore();
                i.Column(g => g.LevelSpeedRunComID).Ignore();
                i.Column(g => g.ExaminerSpeedRunComID).Ignore();
            });
            For<SpeedRunSpeedRunComIDEntity>().PrimaryKey("SpeedRunID", false).TableName("dbo.tbl_SpeedRun_SpeedRunComID" + tblEnd);
            For<SpeedRunVariableValueEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_VariableValue" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
                i.Column(g => g.VariableSpeedRunComID).Ignore();
                i.Column(g => g.VariableValueSpeedRunComID).Ignore();
            });
            For<SpeedRunPlayerEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_Player" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
                i.Column(g => g.UserSpeedRunComID).Ignore();
            });
            For<SpeedRunVideoEntity>().PrimaryKey("ID", true).TableName("dbo.tbl_SpeedRun_Video" + tblEnd).Columns(i =>
            {
                i.Column(g => g.SpeedRunSpeedRunComID).Ignore();
            });
        }
    }
}



