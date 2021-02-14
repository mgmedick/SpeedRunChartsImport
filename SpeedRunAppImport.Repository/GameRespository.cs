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

                                    DELETE FROM dbo.[tbl_Level] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_Level] ADD CONSTRAINT [FK_tbl_Level_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_Category] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_Category] ADD CONSTRAINT [FK_tbl_Category_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_Variable] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_VariableValue] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_VariableValue] ADD CONSTRAINT [FK_tbl_VariableValue_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_Game_Platform] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_Game_Platform] ADD CONSTRAINT [FK_tbl_Game_Platform_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_Game_Region] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_Game_Region] ADD CONSTRAINT [FK_tbl_Game_Region_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_Game_Moderator] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_Game_Moderator] ADD CONSTRAINT [FK_tbl_Game_Moderator_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_Game_TimingMethod] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
                                    ALTER TABLE [dbo].[tbl_Game_TimingMethod] ADD CONSTRAINT [FK_tbl_Game_TimingMethod_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])

                                    DELETE FROM dbo.[tbl_SpeedRun] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Game g WHERE g.ID = GameID)                           
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

                                    DELETE FROM dbo.[tbl_Variable] WHERE LevelID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Level l WHERE l.ID = LevelID)                           
                                    ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Level] FOREIGN KEY ([LevelID]) REFERENCES [dbo].[tbl_Level] ([ID])

                                    DELETE FROM dbo.[tbl_SpeedRun] WHERE LevelID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Level l WHERE l.ID = LevelID)                           
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

                                    DELETE FROM dbo.[tbl_Variable] WHERE CategoryID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Category c WHERE c.ID = CategoryID)                           
                                    ALTER TABLE [dbo].[tbl_Variable] ADD CONSTRAINT [FK_tbl_Variable_tbl_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Category] ([ID])

                                    DELETE FROM dbo.[tbl_SpeedRun] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Category c WHERE c.ID = CategoryID)                           
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

                                    DELETE FROM dbo.[tbl_VariableValue] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Variable v WHERE v.ID = VariableID)                           
                                    ALTER TABLE [dbo].[tbl_VariableValue] ADD CONSTRAINT [FK_tbl_VariableValue_tbl_Variable] FOREIGN KEY ([VariableID]) REFERENCES [dbo].[tbl_Variable] ([ID])

                                    DELETE FROM dbo.[tbl_SpeedRun_VariableValue] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Variable v WHERE v.ID = VariableID)                           
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

                                    DELETE FROM dbo.[tbl_SpeedRun_VariableValue] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_VariableValue v WHERE v.ID = VariableValueID)                           
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
                var gameSpeedRunComIDs = gamesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var gameLinksBatch = gameLinks.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                var levelsBatch = levels.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                var levelSpeedRunComIDs = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var levelRulesBatch = levelRules.Where(i => levelSpeedRunComIDs.Contains(i.LevelSpeedRunComID)).ToList();
                var categoriesBatch = categories.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                var categorySpeedRunComIDs = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var categoryRulesBatch = categoryRules.Where(i => categorySpeedRunComIDs.Contains(i.CategorySpeedRunComID)).ToList();
                var variablesBatch = variables.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)
                                                        && (string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) || categorySpeedRunComIDs.Contains(i.CategorySpeedRunComID))
                                                        && (string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) || levelSpeedRunComIDs.Contains(i.LevelSpeedRunComID)))
                                              .ToList();
                var variableSpeedRunComIDs = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var variablesValuesBatch = variableValues.Where(i => variableSpeedRunComIDs.Contains(i.VariableSpeedRunComID)).ToList();
                var gamePlatformsBatch = gamePlatforms.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameRegionsBatch = gameRegions.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameModeratorsBatch = gameModerators.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameRulesetsBatch = gameRulesets.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                var gameTimingMethodsBatch = gameTimingMethods.Where(i => gameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        //foreach (var game in gamesBatch)
                        //{
                        //    db.Insert(game);
                        //}
                        db.InsertBulk<GameEntity>(gamesBatch);
                        var gameIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Game_Full ORDER BY ID DESC", gamesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < gamesBatch.Count; i++)
                        {
                            gamesBatch[i].ID = gameIDs[i];
                        }

                        var gameSpeedRunComIDsBatch = gamesBatch.Select(i => new GameSpeedRunComIDEntity { GameID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBulk<GameSpeedRunComIDEntity>(gameSpeedRunComIDsBatch);

                        gameLinksBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<GameLinkEntity>(gameLinksBatch);

                        //foreach (var level in levelsBatch)
                        //{
                        //    level.GameID = gamesBatch.Find(g => g.SpeedRunComID == level.GameSpeedRunComID).ID;
                        //    db.Insert(level);
                        //}
                        levelsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<LevelEntity>(levelsBatch);
                        var levelIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Level_Full ORDER BY ID DESC", levelsBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < levelsBatch.Count; i++)
                        {
                            levelsBatch[i].ID = levelIDs[i];
                        }

                        var levelSpeedRunComIDsBatch = levelsBatch.Select(i => new LevelSpeedRunComIDEntity { LevelID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBulk<LevelSpeedRunComIDEntity>(levelSpeedRunComIDsBatch);

                        levelRulesBatch.ForEach(i => i.LevelID = levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID);
                        db.InsertBulk<LevelRuleEntity>(levelRulesBatch);

                        //foreach (var category in categoriesBatch)
                        //{
                        //    category.GameID = gamesBatch.Find(g => g.SpeedRunComID == category.GameSpeedRunComID).ID;
                        //    db.Insert(category);
                        //}
                        categoriesBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<CategoryEntity>(categoriesBatch);
                        var categoryIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Category_Full ORDER BY ID DESC", categoriesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < categoriesBatch.Count; i++)
                        {
                            categoriesBatch[i].ID = categoryIDs[i];
                        }

                        var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => new CategorySpeedRunComIDEntity { CategoryID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBulk<CategorySpeedRunComIDEntity>(categorySpeedRunComIDsBatch);

                        categoryRulesBatch.ForEach(i => i.CategoryID = categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID);
                        db.InsertBulk<CategoryRuleEntity>(categoryRulesBatch);

                        //foreach (var variable in variablesBatch)
                        //{
                        //    variable.GameID = gamesBatch.Find(g => g.SpeedRunComID == variable.GameSpeedRunComID).ID;
                        //    variable.CategoryID = !string.IsNullOrWhiteSpace(variable.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == variable.CategorySpeedRunComID).ID : (int?)null;
                        //    variable.LevelID = !string.IsNullOrWhiteSpace(variable.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == variable.LevelSpeedRunComID).ID : (int?)null;
                        //    db.Insert(variable);
                        //}
                        variablesBatch.ForEach(i =>
                        {
                            i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID;
                            i.CategoryID = !string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID : (int?)null;
                            i.LevelID = !string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID : (int?)null;
                        });
                        db.InsertBulk<VariableEntity>(variablesBatch);
                        var variableIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Variable_Full ORDER BY ID DESC", variablesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < variablesBatch.Count; i++)
                        {
                            variablesBatch[i].ID = variableIDs[i];
                        }

                        var variableSpeedRunComIDsBatch = variablesBatch.Select(i => new VariableSpeedRunComIDEntity { VariableID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBulk<VariableSpeedRunComIDEntity>(variableSpeedRunComIDsBatch);

                        //foreach (var variableValue in variablesValuesBatch)
                        //{
                        //    variableValue.GameID = gamesBatch.Find(g => g.SpeedRunComID == variableValue.GameSpeedRunComID).ID;
                        //    variableValue.VariableID = variablesBatch.Find(g => g.SpeedRunComID == variableValue.VariableSpeedRunComID).ID;
                        //    db.Insert(variableValue);
                        //}
                        variablesValuesBatch.ForEach(i =>
                        {
                            i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID;
                            i.VariableID = variablesBatch.Find(g => g.SpeedRunComID == i.VariableSpeedRunComID).ID;
                        });
                        db.InsertBulk<VariableValueEntity>(variablesValuesBatch);
                        var variableValueIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_VariableValue_Full ORDER BY ID DESC", variablesValuesBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < variablesValuesBatch.Count; i++)
                        {
                            variablesValuesBatch[i].ID = variableValueIDs[i];
                        }

                        var variableValueSpeedRunComIDsBatch = variablesValuesBatch.Select(i => new VariableValueSpeedRunComIDEntity { VariableValueID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBulk<VariableValueSpeedRunComIDEntity>(variableValueSpeedRunComIDsBatch);

                        gamePlatformsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<GamePlatformEntity>(gamePlatformsBatch);

                        gameRegionsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<GameRegionEntity>(gameRegionsBatch);

                        gameModeratorsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<GameModeratorEntity>(gameModeratorsBatch);

                        gameRulesetsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<GameRulesetEntity>(gameRulesetsBatch);

                        gameTimingMethodsBatch.ForEach(i => i.GameID = gamesBatch.Find(g => g.SpeedRunComID == i.GameSpeedRunComID).ID);
                        db.InsertBulk<GameTimingMethodEntity>(gameTimingMethodsBatch);

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

            foreach (var game in gamesList)
            {
                var gameLink = gameLinks.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                var levelsBatch = levels.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var levelSpeedRunComIDsBatch = levelsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var levelRulesBatch = levelRules.Where(i => levelSpeedRunComIDsBatch.Contains(i.LevelSpeedRunComID)).ToList();
                var categoriesBatch = categories.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var categorySpeedRunComIDsBatch = categoriesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var categoryRulesBatch = categoryRules.Where(i => categorySpeedRunComIDsBatch.Contains(i.CategorySpeedRunComID)).ToList();
                var variablesBatch = variables.Where(i => i.GameSpeedRunComID == game.SpeedRunComID
                                                        && (string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) || categorySpeedRunComIDsBatch.Contains(i.CategorySpeedRunComID))
                                                        && (string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) || levelSpeedRunComIDsBatch.Contains(i.LevelSpeedRunComID)))
                                              .ToList();
                var variableSpeedRunComIDsBatch = variablesBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var variablesValuesBatch = variableValues.Where(i => variableSpeedRunComIDsBatch.Contains(i.VariableSpeedRunComID)).ToList();
                var gamePlatformsBatch = gamePlatforms.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameRegionsBatch = gameRegions.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameModeratorsBatch = gameModerators.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();
                var gameRuleset = gameRulesets.FirstOrDefault(i => i.GameSpeedRunComID == game.SpeedRunComID);
                var gameTimingMethodsBatch = gameTimingMethods.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        if (game.ID != 0)
                        {
                            game.ModifiedDate = DateTime.Now;
                            db.DeleteWhere<GameSpeedRunComIDEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<VariableValueSpeedRunComIDEntity>("VariableValueID IN (SELECT ID FROM dbo.tbl_VariableValue WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = game.ID });
                            //db.DeleteWhere<VariableValueEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<VariableSpeedRunComIDEntity>("VariableID IN (SELECT ID FROM dbo.tbl_Variable WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = game.ID });
                            //db.DeleteWhere<VariableEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<LevelSpeedRunComIDEntity>("LevelID IN (SELECT ID FROM dbo.tbl_Level WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = game.ID });
                            db.DeleteWhere<LevelRuleEntity>("LevelID IN (SELECT ID FROM dbo.tbl_Level WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = game.ID });
                            //db.DeleteWhere<LevelEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<CategorySpeedRunComIDEntity>("CategoryID IN (SELECT ID FROM dbo.tbl_Category WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = game.ID });
                            db.DeleteWhere<CategoryRuleEntity>("CategoryID IN (SELECT ID FROM dbo.tbl_Category WITH (NOLOCK) WHERE GameID = @gameID)", new { gameID = game.ID });
                            //db.DeleteWhere<CategoryEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<GameLinkEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<GameRulesetEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<GamePlatformEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<GameRegionEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<GameModeratorEntity>("GameID = @gameID", new { gameID = game.ID });
                            db.DeleteWhere<GameTimingMethodEntity>("GameID = @gameID", new { gameID = game.ID });
                        }

                        db.Save<GameEntity>(game);

                        var gameSpeedRunComID = new GameSpeedRunComIDEntity { GameID = game.ID, SpeedRunComID = game.SpeedRunComID };
                        db.Insert<GameSpeedRunComIDEntity>(gameSpeedRunComID);

                        foreach (var level in levelsBatch)
                        {
                            level.GameID = game.ID;
                            db.Save<LevelEntity>(level);
                        }
                        db.DeleteWhere<SpeedRunEntity>("GameID = @gameID AND LevelID IS NOT NULL AND LevelID NOT IN (@levelIDs)", new { gameID = game.ID, levelIDs = string.Join("','", levelsBatch.Select(i => i.ID)) });
                        db.DeleteWhere<LevelEntity>("GameID = @gameID AND LevelID NOT IN (@levelIDs)", new { gameID = game.ID, levelIDs = string.Join("','", levelsBatch.Select(i => i.ID)) });

                        var levelIDs = levelsBatch.Select(i => new LevelSpeedRunComIDEntity { LevelID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<LevelSpeedRunComIDEntity>(levelIDs);

                        levelRulesBatch.ForEach(i => i.LevelID = levelsBatch.Find(g => g.SpeedRunComID == i.LevelSpeedRunComID).ID);
                        db.InsertBatch<LevelRuleEntity>(levelRulesBatch);

                        foreach (var category in categoriesBatch)
                        {
                            category.GameID = game.ID;
                            db.Save(category);
                        }
                        db.DeleteWhere<SpeedRunEntity>("GameID = @gameID AND CategoryID NOT IN (@categoryIDs)", new { gameID = game.ID, categoryIDs = string.Join("','", categoriesBatch.Select(i => i.ID)) });
                        db.DeleteWhere<CategoryEntity>("GameID = @gameID AND CategoryID NOT IN (@categoryIDs)", new { gameID = game.ID, categoryIDs = string.Join("','", categoriesBatch.Select(i => i.ID)) });

                        var categoryIDs = categoriesBatch.Select(i => new CategorySpeedRunComIDEntity { CategoryID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<CategorySpeedRunComIDEntity>(categoryIDs);

                        categoryRulesBatch.ForEach(i => i.CategoryID = categoriesBatch.Find(g => g.SpeedRunComID == i.CategorySpeedRunComID).ID);
                        db.InsertBatch<CategoryRuleEntity>(categoryRulesBatch);

                        foreach (var variable in variablesBatch)
                        {
                            variable.GameID = game.ID;
                            variable.CategoryID = !string.IsNullOrWhiteSpace(variable.CategorySpeedRunComID) ? categoriesBatch.Find(g => g.SpeedRunComID == variable.CategorySpeedRunComID).ID : (int?)null;
                            variable.LevelID = !string.IsNullOrWhiteSpace(variable.LevelSpeedRunComID) ? levelsBatch.Find(g => g.SpeedRunComID == variable.LevelSpeedRunComID).ID : (int?)null;
                            db.Save(variable);
                        }
                        db.DeleteWhere<SpeedRunVariableValueEntity>("GameID = @gameID AND VariableID NOT IN (@variableIDs)", new { gameID = game.ID, variableIDs = string.Join("','", variablesBatch.Select(i => i.ID)) });
                        db.DeleteWhere<VariableEntity>("GameID = @gameID AND VariableID NOT IN (@variableIDs)", new { gameID = game.ID, variableIDs = string.Join("','", variablesBatch.Select(i => i.ID)) });

                        var variableIDs = variablesBatch.Select(i => new VariableSpeedRunComIDEntity { VariableID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<VariableSpeedRunComIDEntity>(variableIDs);

                        foreach (var variableValue in variablesValuesBatch)
                        {
                            variableValue.GameID = game.ID;
                            variableValue.VariableID = variablesBatch.Find(g => g.SpeedRunComID == variableValue.VariableSpeedRunComID).ID;
                            db.Save(variableValue);
                        }
                        db.DeleteWhere<SpeedRunVariableValueEntity>("GameID = @gameID AND VariableValueID NOT IN (@variableValueIDs)", new { gameID = game.ID, variableValueIDs = string.Join("','", variablesValuesBatch.Select(i => i.ID)) });
                        db.DeleteWhere<VariableValueEntity>("GameID = @gameID AND VariableValueID NOT IN (@variableValueIDs)", new { gameID = game.ID, variableValueIDs = string.Join("','", variablesValuesBatch.Select(i => i.ID)) });

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
