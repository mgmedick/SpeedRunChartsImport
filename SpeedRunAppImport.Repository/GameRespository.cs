using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Repository
{
    public class GameRespository : BaseRepository, IGameRepository
    {
        private readonly ILogger _logger;

        public GameRespository(ILogger logger)
        {
            _logger = logger;
        }
        
        public void CopyGameTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"--tbl_Game_Full
                                IF OBJECT_ID('dbo.tbl_Game_Full') IS NOT NULL
                                BEGIN
                                    DROP TABLE dbo.tbl_Game_Full
                                END

                                CREATE TABLE [dbo].[tbl_Game_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [Name] [varchar] (100) NOT NULL,
                                    [IsRomHack] [bit] NOT NULL,
                                    [YearOfRelease] [int] NULL,
                                    [CreatedDate] [datetime] NULL,  
                                    [ImportedDate] [datetime] NOT NULL CONSTRAINT [DF_tbl_Game_Full_ImportedDate] DEFAULT(GETDATE()),
                                    [ModifiedDate] [datetime] NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Full] ADD CONSTRAINT [PK_tbl_Game_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_Game_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Game_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Game_SpeedRunComID_Full] 
                                (
	                                [GameID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_Game_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_Game_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_Game_Link_Full
                                IF OBJECT_ID('dbo.tbl_Game_Link_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Game_Link_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Game_Link_Full] 
                                (
                                    [GameID] [int] NOT NULL,
                                    [SpeedRunComUrl] [varchar] (1000) NOT NULL,
                                    [CoverImageUrl] [varchar] (1000) NULL
                                )
                                ALTER TABLE [dbo].[tbl_Game_Link_Full] ADD CONSTRAINT [PK_tbl_Game_Link_Full] PRIMARY KEY CLUSTERED ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                 --tbl_Level_Full
                                IF OBJECT_ID('dbo.tbl_Level_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Level_Full
                                END

                                CREATE TABLE [dbo].[tbl_Level_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [Name] [varchar] (100) NOT NULL,
                                    [GameID] [int] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Level_Full] ADD CONSTRAINT [PK_tbl_Level_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_Level_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_Level_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Level_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Level_SpeedRunComID_Full] 
                                (
	                                [LevelID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_Level_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_Level_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([LevelID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_Level_Rule_Full
                                IF OBJECT_ID('dbo.tbl_Level_Rule_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Level_Rule_Full 
                                END 

                                CREATE TABLE [dbo].[tbl_Level_Rule_Full] 
                                ( 
                                    [LevelID] [int] NOT NULL, 
                                    [Rules] [varchar] (MAX) NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Level_Rule_Full] ADD CONSTRAINT [PK_tbl_Level_Rule_Full] PRIMARY KEY CLUSTERED ([LevelID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_Category_Full
                                IF OBJECT_ID('dbo.tbl_Category_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Category_Full
                                END

                                CREATE TABLE [dbo].[tbl_Category_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [Name] [varchar] (100) NOT NULL,
                                    [GameID] [int] NOT NULL,
                                    [CategoryTypeID] [int] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Category_Full] ADD CONSTRAINT [PK_tbl_Category_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Category_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_Category_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Category_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Category_SpeedRunComID_Full] 
                                (
	                                [CategoryID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_Category_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_Category_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([CategoryID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Category_Rule_Full
                                IF OBJECT_ID('dbo.tbl_Category_Rule_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Category_Rule_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Category_Rule_Full] 
                                ( 
                                    [CategoryID] [int] NOT NULL, 
                                    [Rules] [varchar] (MAX) NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Category_Rule_Full] ADD CONSTRAINT [PK_tbl_Category_Rule_Full] PRIMARY KEY CLUSTERED ([CategoryID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Variable_Full
                                IF OBJECT_ID('dbo.tbl_Variable_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Variable_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Variable_Full]
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [Name] [varchar] (100) NOT NULL,
                                    [GameID] [int] NOT NULL,
                                    [VariableScopeTypeID] [int] NOT NULL,
                                    [CategoryID] [int] NULL,
                                    [LevelID] [int] NULL,
                                    [IsSubCategory] [bit] NOT NULL   
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Variable_Full] ADD CONSTRAINT [PK_tbl_Variable_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Variable_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_Variable_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Variable_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Variable_SpeedRunComID_Full] 
                                (
	                                [VariableID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_Variable_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_Variable_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([VariableID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_VariableValue_Full
                                IF OBJECT_ID('dbo.tbl_VariableValue_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_VariableValue_Full
                                END 

                                CREATE TABLE [dbo].[tbl_VariableValue_Full]
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [GameID] [int] NOT NULL,   
                                    [VariableID] [int] NOT NULL, 
                                    [Value] [varchar] (100) NOT NULL, 
                                    [IsCustomValue] [bit] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_VariableValue_Full] ADD CONSTRAINT [PK_tbl_VariableValue_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_VariableValue_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_VariableValue_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_VariableValue_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_VariableValue_SpeedRunComID_Full] 
                                (
	                                [VariableValueID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_VariableValue_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_VariableValue_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([VariableValueID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_Game_Platform_Full
                                IF OBJECT_ID('dbo.tbl_Game_Platform_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_Game_Platform_Full
                                END

                                CREATE TABLE [dbo].[tbl_Game_Platform_Full]
                                (     
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [GameID] [int] NOT NULL,
                                    [PlatformID] [int] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Platform_Full] ADD CONSTRAINT [PK_tbl_Game_Platform_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_Region_Full
                                IF OBJECT_ID('dbo.tbl_Game_Region_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_Game_Region_Full
                                END

                                CREATE TABLE [dbo].[tbl_Game_Region_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1),
                                    [GameID] [int] NOT NULL,
                                    [RegionID] [int] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Region_Full] ADD CONSTRAINT [PK_tbl_Game_Region_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_Moderator_Full
                                IF OBJECT_ID('dbo.tbl_Game_Moderator_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_Game_Moderator_Full
                                END

                                CREATE TABLE [dbo].[tbl_Game_Moderator_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1),
                                    [GameID] [int] NOT NULL,
                                    [UserID] [int] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Moderator_Full] ADD CONSTRAINT [PK_tbl_Game_Moderator_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_TimingMethod_Full
                                IF OBJECT_ID('dbo.tbl_Game_TimingMethod_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_Game_TimingMethod_Full
                                END

                                CREATE TABLE [dbo].[tbl_Game_TimingMethod_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1),
                                    [GameID] [int] NOT NULL,
                                    [TimingMethodID] [int] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_TimingMethod_Full] ADD CONSTRAINT [PK_tbl_Game_TimingMethod_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_Ruleset_Full
                                IF OBJECT_ID('dbo.tbl_Game_Ruleset_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_Game_Ruleset_Full
                                END

                                CREATE TABLE [dbo].[tbl_Game_Ruleset_Full] 
                                ( 
                                    [GameID] [int] NOT NULL,
                                    [ShowMilliseconds] [bit] NOT NULL,
                                    [RequiresVerification] [bit] NOT NULL,
                                    [RequiresVideo] [bit] NOT NULL,
                                    [DefaultTimingMethodID] [int] NOT NULL,
                                    [EmulatorsAllowed] [bit] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Ruleset_Full] ADD CONSTRAINT [PK_tbl_Game_Ruleset_Full] PRIMARY KEY CLUSTERED ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }

        public void RenameAndDropGameTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.OneTimeCommandTimeout = 32767;
                    _ = db.Execute(@"--tbl_Game
		                        ALTER TABLE [dbo].[tbl_Level] DROP CONSTRAINT [FK_tbl_Level_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_Category] DROP CONSTRAINT [FK_tbl_Category_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_Variable] DROP CONSTRAINT [FK_tbl_Variable_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_VariableValue] DROP CONSTRAINT [FK_tbl_VariableValue_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_Game_Platform] DROP CONSTRAINT [FK_tbl_Game_Platform_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_Game_Region] DROP CONSTRAINT [FK_tbl_Game_Region_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_Game_Moderator] DROP CONSTRAINT [FK_tbl_Game_Moderator_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_Game_TimingMethod] DROP CONSTRAINT [FK_tbl_Game_TimingMethod_tbl_Game]
		                        ALTER TABLE [dbo].[tbl_SpeedRun] DROP CONSTRAINT [FK_tbl_SpeedRun_tbl_Game]
                                DROP TABLE dbo.tbl_Game

                                EXEC sp_rename 'dbo.PK_tbl_Game_Full', 'PK_tbl_Game'                                
                                EXEC sp_rename 'dbo.DF_tbl_Game_Full_ImportedDate', 'DF_tbl_Game_ImportedDate'
                                EXEC sp_rename 'dbo.tbl_Game_Full', 'tbl_Game'

                                ALTER TABLE [dbo].[tbl_Level] ADD CONSTRAINT [FK_tbl_Level_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_Category] ADD CONSTRAINT [FK_tbl_Category_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_VariableValue] ADD CONSTRAINT [FK_tbl_VariableValue_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_Game_Platform] ADD CONSTRAINT [FK_tbl_Game_Platform_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_Game_Region] ADD CONSTRAINT [FK_tbl_Game_Region_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_Game_Moderator] ADD CONSTRAINT [FK_tbl_Game_Moderator_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_Game_TimingMethod] ADD CONSTRAINT [FK_tbl_Game_TimingMethod_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [FK_tbl_SpeedRun_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                --tbl_Game_SpeedRunComID
                                DROP TABLE dbo.tbl_Game_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_Game_SpeedRunComID_Full', 'PK_tbl_Game_SpeedRunComID'                                
                                EXEC sp_rename 'dbo.tbl_Game_SpeedRunComID_Full', 'tbl_Game_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_Game_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_Link
                                DROP TABLE dbo.tbl_Game_Link
                                
                                EXEC sp_rename 'dbo.PK_tbl_Game_Link_Full', 'PK_tbl_Game_Link'                                
                                EXEC sp_rename 'dbo.tbl_Game_Link_Full', 'tbl_Game_Link'

                                --tbl_Level
		                        ALTER TABLE [dbo].[tbl_Variable] DROP CONSTRAINT [FK_tbl_Variable_tbl_Level]
		                        ALTER TABLE [dbo].[tbl_SpeedRun] DROP CONSTRAINT [FK_tbl_SpeedRun_tbl_Level]

                                DROP TABLE dbo.tbl_Level

                                EXEC sp_rename 'dbo.PK_tbl_Level_Full', 'PK_tbl_Level'                                
                                EXEC sp_rename 'dbo.tbl_Level_Full', 'tbl_Level'

                                ALTER TABLE [dbo].[tbl_Level] ADD CONSTRAINT [FK_tbl_Level_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Level_tbl_Game] ON [dbo].[tbl_Level] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Level] FOREIGN KEY ([LevelID]) REFERENCES [dbo].[tbl_Level] ([ID])
                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [FK_tbl_SpeedRun_tbl_Level] FOREIGN KEY ([LevelID]) REFERENCES [dbo].[tbl_Level] ([ID])

                                --tbl_Level_SpeedRunComID
                                DROP TABLE dbo.tbl_Level_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_Level_SpeedRunComID_Full', 'PK_tbl_Level_SpeedRunComID'  
                                EXEC sp_rename 'dbo.tbl_Level_SpeedRunComID_Full', 'tbl_Level_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_Level_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_Level_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Level_Rule
                                DROP TABLE dbo.tbl_Level_Rule
                                
                                EXEC sp_rename 'dbo.PK_tbl_Level_Rule_Full', 'PK_tbl_Level_Rule'  
                                EXEC sp_rename 'dbo.tbl_Level_Rule_Full', 'tbl_Level_Rule'

                                --tbl_Category
		                        ALTER TABLE [dbo].[tbl_Variable] DROP CONSTRAINT [FK_tbl_Variable_tbl_Category]
		                        ALTER TABLE [dbo].[tbl_SpeedRun] DROP CONSTRAINT [FK_tbl_SpeedRun_tbl_Category]
                                DROP TABLE dbo.tbl_Category

                                EXEC sp_rename 'dbo.PK_tbl_Category_Full', 'PK_tbl_Category'  
                                EXEC sp_rename 'dbo.tbl_Category_Full', 'tbl_Category'

                                ALTER TABLE [dbo].[tbl_Category] ADD CONSTRAINT [FK_tbl_Category_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Category_tbl_Game] ON [dbo].[tbl_Category] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Category] ADD CONSTRAINT [FK_tbl_Category_tbl_CategoryType] FOREIGN KEY ([CategoryTypeID]) REFERENCES [dbo].[tbl_CategoryType] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Category_tbl_CategoryType] ON [dbo].[tbl_Category] ([CategoryTypeID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Category] ([ID])
                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [FK_tbl_SpeedRun_tbl_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Category] ([ID])

                                --tbl_Category_SpeedRunComID
                                DROP TABLE dbo.tbl_Category_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_Category_SpeedRunComID_Full', 'PK_tbl_Category_SpeedRunComID'  
                                EXEC sp_rename 'dbo.tbl_Category_SpeedRunComID_Full', 'tbl_Category_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_Category_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_Category_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Category_Rule
                                DROP TABLE dbo.tbl_Category_Rule
                                
                                EXEC sp_rename 'dbo.PK_tbl_Category_Rule_Full', 'PK_tbl_Category_Rule'  
                                EXEC sp_rename 'dbo.tbl_Category_Rule_Full', 'tbl_Category_Rule'

                                --tbl_Variable
		                        ALTER TABLE [dbo].[tbl_VariableValue] DROP CONSTRAINT [FK_tbl_VariableValue_tbl_Variable]
		                        ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] DROP CONSTRAINT [FK_tbl_SpeedRun_VariableValue_tbl_Variable]
                                DROP TABLE dbo.tbl_Variable

                                EXEC sp_rename 'dbo.PK_tbl_Variable_Full', 'PK_tbl_Variable'  
                                EXEC sp_rename 'dbo.tbl_Variable_Full', 'tbl_Variable'

                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Variable_tbl_Game] ON [dbo].[tbl_Variable] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Category] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Variable_tbl_Category] ON [dbo].[tbl_Variable] ([CategoryID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Level] FOREIGN KEY ([LevelID]) REFERENCES [dbo].[tbl_Level] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Variable_tbl_Level] ON [dbo].[tbl_Variable] ([LevelID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_VariableScopeType] FOREIGN KEY ([VariableScopeTypeID]) REFERENCES [dbo].[tbl_VariableScopeType] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Variable_tbl_VariableScopeType] ON [dbo].[tbl_Variable] ([VariableScopeTypeID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_VariableValue] ADD CONSTRAINT [FK_tbl_VariableValue_tbl_Variable] FOREIGN KEY ([VariableID]) REFERENCES [dbo].[tbl_Variable] ([ID])
                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] ADD CONSTRAINT [FK_tbl_SpeedRun_VariableValue_tbl_Variable] FOREIGN KEY ([VariableID]) REFERENCES [dbo].[tbl_Variable] ([ID])

                                --tbl_Variable_SpeedRunComID
                                DROP TABLE dbo.tbl_Variable_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_Variable_SpeedRunComID_Full', 'PK_tbl_Variable_SpeedRunComID'  
                                EXEC sp_rename 'dbo.tbl_Variable_SpeedRunComID_Full', 'tbl_Variable_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_Variable_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_Variable_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                
                                --tbl_VariableValue
		                        ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] DROP CONSTRAINT [FK_tbl_SpeedRun_VariableValue_tbl_VariableValue]
                                DROP TABLE dbo.tbl_VariableValue

                                EXEC sp_rename 'dbo.PK_tbl_VariableValue_Full', 'PK_tbl_VariableValue'  
                                EXEC sp_rename 'dbo.tbl_VariableValue_Full', 'tbl_VariableValue'

                                ALTER TABLE [dbo].[tbl_VariableValue] ADD CONSTRAINT [FK_tbl_VariableValue_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_VariableValue_tbl_Game] ON [dbo].[tbl_VariableValue] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_VariableValue] ADD CONSTRAINT [FK_tbl_VariableValue_tbl_Variable] FOREIGN KEY ([VariableID]) REFERENCES [dbo].[tbl_Variable] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_VariableValue_tbl_Variable] ON [dbo].[tbl_VariableValue] ([VariableID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] ADD CONSTRAINT [FK_tbl_SpeedRun_VariableValue_tbl_VariableValue] FOREIGN KEY ([VariableValueID]) REFERENCES [dbo].[tbl_VariableValue] ([ID])

                                --tbl_VariableValue_SpeedRunComID
                                DROP TABLE dbo.tbl_VariableValue_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_VariableValue_SpeedRunComID_Full', 'PK_tbl_VariableValue_SpeedRunComID'  
                                EXEC sp_rename 'dbo.tbl_VariableValue_SpeedRunComID_Full', 'tbl_VariableValue_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_VariableValue_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_VariableValue_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                
                                --tbl_Game_Platform
                                DROP TABLE dbo.tbl_Game_Platform

                                EXEC sp_rename 'dbo.PK_tbl_Game_Platform_Full', 'PK_tbl_Game_Platform'  
                                EXEC sp_rename 'dbo.tbl_Game_Platform_Full', 'tbl_Game_Platform'

                                ALTER TABLE [dbo].[tbl_Game_Platform] ADD CONSTRAINT [FK_tbl_Game_Platform_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Platform_tbl_Game] ON [dbo].[tbl_Game_Platform] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Platform] ADD CONSTRAINT [FK_tbl_Game_Platform_tbl_Platform] FOREIGN KEY ([PlatformID]) REFERENCES [dbo].[tbl_Platform] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Platform_tbl_Platform] ON [dbo].[tbl_Game_Platform] ([PlatformID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_Region
                                DROP TABLE dbo.tbl_Game_Region

                                EXEC sp_rename 'dbo.PK_tbl_Game_Region_Full', 'PK_tbl_Game_Region'  
                                EXEC sp_rename 'dbo.tbl_Game_Region_Full', 'tbl_Game_Region'

                                ALTER TABLE [dbo].[tbl_Game_Region] ADD CONSTRAINT [FK_tbl_Game_Region_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Region_tbl_Game] ON [dbo].[tbl_Game_Region] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Region] ADD CONSTRAINT [FK_tbl_Game_Region_tbl_Region] FOREIGN KEY ([RegionID]) REFERENCES [dbo].[tbl_Region] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Region_tbl_Region] ON [dbo].[tbl_Game_Region] ([RegionID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_Moderator
                                DROP TABLE dbo.tbl_Game_Moderator

                                EXEC sp_rename 'dbo.PK_tbl_Game_Moderator_Full', 'PK_tbl_Game_Moderator'  
                                EXEC sp_rename 'dbo.tbl_Game_Moderator_Full', 'tbl_Game_Moderator'

                                ALTER TABLE [dbo].[tbl_Game_Moderator] ADD CONSTRAINT [FK_tbl_Game_Moderator_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Moderator_tbl_Game] ON [dbo].[tbl_Game_Moderator] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_Moderator] ADD CONSTRAINT [FK_tbl_Game_Moderator_tbl_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[tbl_User] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_Moderator_tbl_User] ON [dbo].[tbl_Game_Moderator] ([UserID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_Game_TimingMethod
                                DROP TABLE dbo.tbl_Game_TimingMethod

                                EXEC sp_rename 'dbo.PK_tbl_Game_TimingMethod_Full', 'PK_tbl_Game_TimingMethod'  
                                EXEC sp_rename 'dbo.tbl_Game_TimingMethod_Full', 'tbl_Game_TimingMethod'

                                ALTER TABLE [dbo].[tbl_Game_TimingMethod] ADD CONSTRAINT [FK_tbl_Game_TimingMethod_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_TimingMethod_tbl_Game] ON [dbo].[tbl_Game_TimingMethod] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Game_TimingMethod] ADD CONSTRAINT [FK_tbl_Game_TimingMethod_tbl_TimingMethod] FOREIGN KEY ([TimingMethodID]) REFERENCES [dbo].[tbl_TimingMethod] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Game_TimingMethod_tbl_TimingMethod] ON [dbo].[tbl_Game_TimingMethod] ([TimingMethodID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_Game_Ruleset
                                DROP TABLE dbo.tbl_Game_Ruleset

                                EXEC sp_rename 'dbo.PK_tbl_Game_Ruleset_Full', 'PK_tbl_Game_Ruleset'  
                                EXEC sp_rename 'dbo.tbl_Game_Ruleset_Full', 'tbl_Game_Ruleset'");
                    tran.Complete();
                }
            }
        }

        public void InsertGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods)
        {
            _logger.Information("Started InsertGames");
            int batchCount = 0;
            var gamesList = games.ToList();

            while (batchCount < gamesList.Count)
            {
                var gamesBatch = gamesList.Skip(batchCount).Take(MaxBulkRows).ToList();
                var gameIDs = gamesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var gameLinksBatch = gameLinks.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var levelsBatch = levels.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var levelIDs = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var levelRulesBatch = levelRules.Where(i => levelIDs.Contains(i.LevelSpeedRunComID)).ToList();
                var categoriesBatch = categories.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var categoryIDs = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var categoryRulesBatch = categoryRules.Where(i => categoryIDs.Contains(i.CategorySpeedRunComID)).ToList();
                var variablesBatch = variables.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var variableIDs = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var variablesValuesBatch = variableValues.Where(i => variableIDs.Contains(i.VariableSpeedRunComID)).ToList();
                var gamePlatformsBatch = gamePlatforms.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameRegionsBatch = gameRegions.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameModeratorsBatch = gameModerators.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameRulesetsBatch = gameRulesets.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameTimingMethodsBatch = gameTimingMethods.Where(i => gameIDs.Contains(i.GameSpeedRunComID)).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        foreach (var game in gamesBatch)
                        {
                            db.Insert(game);
                        }

                        var gameSpeedRunComIDsBatch = gamesBatch.Select(i => new GameSpeedRunComIDEntity { GameID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<GameSpeedRunComIDEntity>(gameSpeedRunComIDsBatch);

                        gameLinksBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameLinkEntity>(gameLinksBatch);

                        foreach (var level in levelsBatch)
                        {
                            level.GameID = gamesBatch.Find(g => g.SpeedRunComID == level.GameSpeedRunComID).ID;
                            db.Insert(level);
                        }

                        var levelSpeedRunComIDsBatch = levelsBatch.Select(i => new LevelSpeedRunComIDEntity { LevelID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<LevelSpeedRunComIDEntity>(levelSpeedRunComIDsBatch);

                        levelRulesBatch.ForEach(i => i.LevelID = levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID);
                        db.InsertBatch<LevelRuleEntity>(levelRulesBatch);

                        foreach (var category in categoriesBatch)
                        {
                            category.GameID = gamesBatch.Find(g => g.SpeedRunComID == category.GameSpeedRunComID).ID;
                            db.Insert(category);
                        }

                        var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => new CategorySpeedRunComIDEntity { CategoryID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<CategorySpeedRunComIDEntity>(categorySpeedRunComIDsBatch);

                        categoryRulesBatch.ForEach(i => i.CategoryID = categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID);
                        db.InsertBatch<CategoryRuleEntity>(categoryRulesBatch);

                        foreach (var variable in variablesBatch)
                        {
                            variable.GameID = gamesBatch.Find(g => g.SpeedRunComID == variable.GameSpeedRunComID).ID;
                            variable.CategoryID = !string.IsNullOrWhiteSpace(variable.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == variable.CategorySpeedRunComID).ID : (int?)null;
                            variable.LevelID = !string.IsNullOrWhiteSpace(variable.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == variable.LevelSpeedRunComID).ID : (int?)null;
                            db.Insert(variable);
                        }

                        var variableSpeedRunComIDsBatch = variablesBatch.Select(i => new VariableSpeedRunComIDEntity { VariableID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableSpeedRunComIDEntity>(variableSpeedRunComIDsBatch);

                        foreach (var variableValue in variablesValuesBatch)
                        {
                            variableValue.GameID = gamesBatch.Find(g => g.SpeedRunComID == variableValue.GameSpeedRunComID).ID;
                            variableValue.VariableID = variablesBatch.Find(g => g.SpeedRunComID == variableValue.VariableSpeedRunComID).ID;
                            db.Insert(variableValue);
                        }

                        var variableValueSpeedRunComIDsBatch = variablesValuesBatch.Select(i => new VariableValueSpeedRunComIDEntity { VariableValueID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableValueSpeedRunComIDEntity>(variableValueSpeedRunComIDsBatch);

                        gamePlatformsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GamePlatformEntity>(gamePlatformsBatch);

                        gameRegionsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameRegionEntity>(gameRegionsBatch);

                        gameModeratorsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameModeratorEntity>(gameModeratorsBatch);

                        gameRulesetsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameRulesetEntity>(gameRulesetsBatch);

                        gameTimingMethodsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBatch<GameTimingMethodEntity>(gameTimingMethodsBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved games {@Count} / {@Total}", gamesBatch.Count, gamesList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertGames");
        }

        public void SaveGames(IEnumerable<GameEntity> games, IEnumerable<GameLinkEntity> gameLinks, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> categoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods)
        {
            int count = 1;
            var gamesList = games.ToList();
            var gameSpeedRunComIDs = GetGameSpeedRunComIDs();

            foreach (var game in gamesList)
            {
                var gameLink = gameLinks.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                var levelsBatch = levels.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var levelIDs = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var levelRulesBatch = levelRules.Where(i => levelIDs.Contains(i.LevelSpeedRunComID)).ToList();
                var categoriesBatch = categories.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var categoryIDs = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var categoryRulesBatch = categoryRules.Where(i => categoryIDs.Contains(i.CategorySpeedRunComID)).ToList();
                var variablesBatch = variables.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var variableIDs = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var variablesValuesBatch = variableValues.Where(i => variableIDs.Contains(i.VariableSpeedRunComID)).ToList();
                var gamePlatformsBatch = gamePlatforms.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameRegionsBatch = gameRegions.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameModeratorsBatch = gameModerators.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameRuleset = gameRulesets.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                var gameTimingMethodsBatch = gameTimingMethods.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        var gameSpeedRunCom = gameSpeedRunComIDs.FirstOrDefault(i => i.SpeedRunComID == game.SpeedRunComID);
                        if (gameSpeedRunCom != null)
                        {
                            game.ModifiedDate = DateTime.Now;
                            db.DeleteWhere<GameSpeedRunComIDEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableValueSpeedRunComIDEntity>("VariableValueID IN (SELECT ID FROM dbo.tbl_VariableValue WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableValueEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableSpeedRunComIDEntity>("VariableID IN (SELECT ID FROM dbo.tbl_Variable WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<VariableEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<LevelSpeedRunComIDEntity>("LevelID IN (SELECT ID FROM dbo.tbl_Level WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<LevelRuleEntity>("LevelID IN (SELECT ID FROM dbo.tbl_Level WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<LevelEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<CategorySpeedRunComIDEntity>("CategoryID IN (SELECT ID FROM dbo.tbl_Category WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<CategoryRuleEntity>("CategoryID IN (SELECT ID FROM dbo.tbl_Category WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<CategoryEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameLinkEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameRulesetEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GamePlatformEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameRegionEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameModeratorEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                            db.DeleteWhere<GameTimingMethodEntity>("GameID = @gameID", new { gameID = gameSpeedRunCom.GameID });
                        }

                        db.Save<GameEntity>(game);

                        var gameSpeedRunComID = new GameSpeedRunComIDEntity { GameID = game.ID, SpeedRunComID = game.SpeedRunComID };
                        db.Insert<GameSpeedRunComIDEntity>(gameSpeedRunComID);

                        foreach (var level in levelsBatch)
                        {
                            level.GameID = game.ID;
                            db.Insert(level);
                        }

                        var levelSpeedRunComIDsBatch = levelsBatch.Select(i => new LevelSpeedRunComIDEntity { LevelID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<LevelSpeedRunComIDEntity>(levelSpeedRunComIDsBatch);

                        levelRulesBatch.ForEach(i => i.LevelID = levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID);
                        db.InsertBatch<LevelRuleEntity>(levelRulesBatch);

                        foreach (var category in categoriesBatch)
                        {
                            category.GameID = game.ID;
                            db.Insert(category);
                        }

                        var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => new CategorySpeedRunComIDEntity { CategoryID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<CategorySpeedRunComIDEntity>(categorySpeedRunComIDsBatch);

                        categoryRulesBatch.ForEach(i => i.CategoryID = categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID);
                        db.InsertBatch<CategoryRuleEntity>(categoryRulesBatch);

                        foreach (var variable in variablesBatch)
                        {
                            variable.GameID = game.ID;
                            variable.CategoryID = !string.IsNullOrWhiteSpace(variable.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == variable.CategorySpeedRunComID).ID : (int?)null;
                            variable.LevelID = !string.IsNullOrWhiteSpace(variable.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == variable.LevelSpeedRunComID).ID : (int?)null;
                            db.Insert(variable);
                        }

                        var variableSpeedRunComIDsBatch = variablesBatch.Select(i => new VariableSpeedRunComIDEntity { VariableID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableSpeedRunComIDEntity>(variableSpeedRunComIDsBatch);

                        foreach (var variableValue in variablesValuesBatch)
                        {
                            variableValue.GameID = game.ID;
                            variableValue.VariableID = variablesBatch.Find(g => g.SpeedRunComID == variableValue.VariableSpeedRunComID).ID;
                            db.Insert(variableValue);
                        }

                        var variableValueSpeedRunComIDsBatch = variablesValuesBatch.Select(i => new VariableValueSpeedRunComIDEntity { VariableValueID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableValueSpeedRunComIDEntity>(variableValueSpeedRunComIDsBatch);

                        if (gameLink != null)
                        {
                            gameLink.GameID = game.ID;
                            db.Insert<GameLinkEntity>(gameLink);
                        }

                        if (gameRuleset != null)
                        {
                            gameRuleset.GameID = game.ID;
                            db.Insert<GameRulesetEntity>(gameRuleset);
                        }

                        gamePlatformsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GamePlatformEntity>(gamePlatformsBatch);

                        gameRegionsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GameRegionEntity>(gameRegionsBatch);

                        gameModeratorsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GameModeratorEntity>(gameModeratorsBatch);

                        gameTimingMethodsBatch.ForEach(i => i.GameID = game.ID);
                        db.InsertBatch<GameTimingMethodEntity>(gameTimingMethodsBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved games {@Count} / {@Total}", count, gamesList.Count);
                count++;
            }
        }

        //public IEnumerable<VariableEntity> GetVariables()
        //{
        //    using (IDatabase db = DBFactory.GetDatabase())
        //    {
        //        return db.Query<VariableEntity>("SELECT OrderValue, ID, [Name], GameID, VariableScopeTypeID, CategoryID, LevelID, IsSubCategory FROM dbo.tbl_Variable WITH (NOLOCK) ORDER BY OrderValue").ToList();
        //    }
        //}

        //public IEnumerable<GameEntity> GetGames()
        //{
        //    using (IDatabase db = DBFactory.GetDatabase())
        //    {
        //        return db.Query<GameEntity>("SELECT [OrderValue], [ID], [Name], [JapaneseName], [Abbreviation], [IsRomHack], [YearOfRelease], [SpeedRunComUrl], [CoverImageUrl], [CreatedDate], [ImportedDate], [ModifiedDate] FROM [dbo].[tbl_Game] WITH (NOLOCK) ORDER BY OrderValue").ToList();
        //    }
        //}

        public IEnumerable<GameSpeedRunComIDEntity> GetGameSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<GameSpeedRunComIDEntity>("SELECT GameID, SpeedRunComID FROM dbo.tbl_Game_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }

        public IEnumerable<CategorySpeedRunComIDEntity> GetCategorySpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<CategorySpeedRunComIDEntity>("SELECT CategoryID, SpeedRunComID FROM dbo.tbl_Category_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }

        public IEnumerable<LevelSpeedRunComIDEntity> GetLevelSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<LevelSpeedRunComIDEntity>("SELECT LevelID, SpeedRunComID FROM dbo.tbl_Level_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }

        public IEnumerable<VariableSpeedRunComIDEntity> GetVariableSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<VariableSpeedRunComIDEntity>("SELECT VariableID, SpeedRunComID FROM dbo.tbl_Variable_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }

        public IEnumerable<VariableValueSpeedRunComIDEntity> GetVariableValueSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<VariableValueSpeedRunComIDEntity>("SELECT VariableValueID, SpeedRunComID FROM dbo.tbl_VariableValue_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }

        public IEnumerable<RegionSpeedRunComIDEntity> GetRegionSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<RegionSpeedRunComIDEntity>("SELECT RegionID, SpeedRunComID FROM dbo.tbl_Region_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }
    }
}
