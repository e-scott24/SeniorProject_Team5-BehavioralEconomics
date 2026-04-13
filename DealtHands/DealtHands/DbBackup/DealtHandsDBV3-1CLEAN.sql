-- =============================================
-- Script: Recreate DealtHandsDBV2 Database (LocalDB Fixed)
-- Purpose: Clean rebuild of the database with current schema
-- =============================================

USE [master]
GO

-- Drop the database if it exists (LocalDB compatible)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DealtHandsDBV2')
BEGIN
    -- Force disconnect all users
    ALTER DATABASE [DealtHandsDBV2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DealtHandsDBV2];
    PRINT 'Database DealtHandsDBV2 dropped successfully';
END
GO

-- Create the database (let SQL Server choose the file location)
CREATE DATABASE [DealtHandsDBV2]
GO

ALTER DATABASE [DealtHandsDBV2] SET COMPATIBILITY_LEVEL = 150
GO

USE [DealtHandsDBV2]
GO

-- =============================================
-- Create Tables
-- =============================================

-- EF Migrations History Table
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
) ON [PRIMARY]
GO

-- Game Table
CREATE TABLE [dbo].[Game](
	[GameId] [bigint] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](80) NOT NULL,
	[Description] [varchar](255) NULL,
	[Mode] [varchar](20) NOT NULL,
	[MaxPlayers] [int] NULL,
	[CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_Game_CreatedAt] DEFAULT (getutcdate()),
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_Game_IsActive] DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([GameId] ASC)
) ON [PRIMARY]
GO

-- User Table
CREATE TABLE [dbo].[User](
	[UserId] [bigint] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](40) NOT NULL,
	[Email] [varchar](120) NOT NULL,
	[PasswordHash] [varchar](255) NULL,
	[CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_User_CreatedAt] DEFAULT (getutcdate()),
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_User_IsActive] DEFAULT ((1)),
	[PasswordResetToken] [varchar](255) NULL,
	[PasswordResetExpires] [datetime] NULL,
	[IsEducator] [bit] NOT NULL CONSTRAINT [DF_User_IsEducator] DEFAULT ((0)),
	[HasStudentLoans] [bit] NOT NULL CONSTRAINT [DF_User_HasStudentLoans] DEFAULT ((0)),
	[HasCar] [bit] NOT NULL CONSTRAINT [DF_User_HasCar] DEFAULT ((0)),
	[HasCarLoan] [bit] NOT NULL CONSTRAINT [DF_User_HasCarLoan] DEFAULT ((0)),
	[OwnsHome] [bit] NOT NULL CONSTRAINT [DF_User_OwnsHome] DEFAULT ((0)),
	[HasApartment] [bit] NOT NULL CONSTRAINT [DF_User_HasApartment] DEFAULT ((0)),
	[HasRoommate] [bit] NOT NULL CONSTRAINT [DF_User_HasRoommate] DEFAULT ((0)),
	[IsMarried] [bit] NOT NULL CONSTRAINT [DF_User_IsMarried] DEFAULT ((0)),
	[HasChildren] [bit] NOT NULL CONSTRAINT [DF_User_HasChildren] DEFAULT ((0)),
	[HasJob] [bit] NOT NULL CONSTRAINT [DF_User_HasJob] DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([UserId] ASC),
UNIQUE NONCLUSTERED ([Username] ASC),
UNIQUE NONCLUSTERED ([Email] ASC)
) ON [PRIMARY]
GO

-- Card Table
CREATE TABLE [dbo].[Card](
	[CardId] [int] IDENTITY(1,1) NOT NULL,
	[RoundType] [varchar](20) NOT NULL,
	[CardType] [varchar](20) NOT NULL CONSTRAINT [DF_Card_CardType] DEFAULT ('RoundCard'),
	[Title] [varchar](80) NOT NULL,
	[Description] [varchar](255) NULL,
	[MonthlyAmount] [decimal](10, 2) NULL,
	[FieldData] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_Card_IsActive] DEFAULT ((1)),
	[DifficultyLevel] [tinyint] NOT NULL CONSTRAINT [DF_Card_DifficultyLevel] DEFAULT ((0)),
	[RequiresStudentLoans] [bit] NULL,
	[RequiresCar] [bit] NULL,
	[RequiresCarLoan] [bit] NULL,
	[RequiresOwnsHome] [bit] NULL,
	[RequiresApartment] [bit] NULL,
	[RequiresRoommate] [bit] NULL,
	[RequiresMarried] [bit] NULL,
	[RequiresChildren] [bit] NULL,
	[SetsStudentLoans] [bit] NULL,
	[SetsCar] [bit] NULL,
	[SetsCarLoan] [bit] NULL,
	[SetsOwnsHome] [bit] NULL,
	[SetsApartment] [bit] NULL,
	[SetsRoommate] [bit] NULL,
	[SetsMarried] [bit] NULL,
	[SetsChildren] [bit] NULL,
PRIMARY KEY CLUSTERED ([CardId] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- GameChanger Table
CREATE TABLE [dbo].[GameChanger](
	[GameChangerId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](80) NOT NULL,
	[Description] [varchar](500) NULL,
	[RoundType] [varchar](20) NOT NULL,
	[DifficultyLevel] [tinyint] NOT NULL CONSTRAINT [DF_GameChanger_DifficultyLevel] DEFAULT ((0)),
	[MonthlyAmount] [decimal](10, 2) NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_GameChanger_IsActive] DEFAULT ((1)),
	[RequiresApartment] [bit] NULL,
	[RequiresCar] [bit] NULL,
	[RequiresCarLoan] [bit] NULL,
	[RequiresOwnsHome] [bit] NULL,
	[RequiresStudentLoans] [bit] NULL,
	[RequiresRoommate] [bit] NULL,
	[RequiresMarried] [bit] NULL,
	[RequiresChildren] [bit] NULL,
	[RequiresJob] [bit] NULL,
	[IncomeEffect] [decimal](10, 2) NULL,
	[IncomeEffectPercent] [decimal](5, 4) NULL,
	[ExpenseEffect] [decimal](10, 2) NULL,
	[SetsApartment] [bit] NULL,
	[SetsCar] [bit] NULL,
	[SetsCarLoan] [bit] NULL,
	[SetsOwnsHome] [bit] NULL,
	[SetsStudentLoans] [bit] NULL,
	[SetsRoommate] [bit] NULL,
	[SetsMarried] [bit] NULL,
	[SetsChildren] [bit] NULL,
	[SetsJob] [bit] NULL,
 CONSTRAINT [PK_GameChanger] PRIMARY KEY CLUSTERED ([GameChangerId] ASC)
) ON [PRIMARY]
GO

-- GameSession Table
CREATE TABLE [dbo].[GameSession](
	[GameSessionId] [bigint] IDENTITY(1,1) NOT NULL,
	[GameId] [bigint] NOT NULL,
	[HostUserId] [bigint] NOT NULL,
	[JoinCode] [varchar](12) NOT NULL,
	[Status] [varchar](20) NOT NULL CONSTRAINT [DF_GameSession_Status] DEFAULT ('Waiting'),
	[CurrentRoundNumber] [tinyint] NOT NULL CONSTRAINT [DF_GameSession_CurrentRoundNumber] DEFAULT ((1)),
	[CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_GameSession_CreatedAt] DEFAULT (getutcdate()),
	[StartedAt] [datetime] NULL,
	[PausedAt] [datetime] NULL,
	[ResumedAt] [datetime] NULL,
	[EndedAt] [datetime] NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_GameSession_IsActive] DEFAULT ((1)),
	[Name] [varchar](100) NULL,
	[Difficulty] [varchar](10) NULL,
PRIMARY KEY CLUSTERED ([GameSessionId] ASC),
UNIQUE NONCLUSTERED ([JoinCode] ASC)
) ON [PRIMARY]
GO

-- GameRound Table
CREATE TABLE [dbo].[GameRound](
	[GameRoundId] [bigint] IDENTITY(1,1) NOT NULL,
	[GameSessionId] [bigint] NOT NULL,
	[RoundNumber] [tinyint] NOT NULL,
	[RoundType] [varchar](20) NOT NULL,
	[Status] [varchar](20) NOT NULL CONSTRAINT [DF_GameRound_Status] DEFAULT ('NotStarted'),
	[OpenedAt] [datetime] NULL,
	[ClosedAt] [datetime] NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_GameRound_IsActive] DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([GameRoundId] ASC),
 CONSTRAINT [uq_gameround_session_roundnum] UNIQUE NONCLUSTERED ([GameSessionId] ASC, [RoundNumber] ASC)
) ON [PRIMARY]
GO

-- UGC Table (User Generated Content)
CREATE TABLE [dbo].[UGC](
	[UGCId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[CardId] [int] NULL,
	[GameChangerId] [int] NULL,
	[GameRoundId] [bigint] NOT NULL,
	[GameSessionId] [bigint] NOT NULL,
	[SubmittedAmount] [decimal](10, 2) NULL,
	[RunningTotal] [decimal](10, 2) NULL,
	[AssignedAt] [datetime] NOT NULL CONSTRAINT [DF_UGC_AssignedAt] DEFAULT (getutcdate()),
	[SubmittedAt] [datetime] NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_UGC_IsActive] DEFAULT ((1)),
PRIMARY KEY CLUSTERED ([UGCId] ASC),
 CONSTRAINT [uq_ugc_user_round_card] UNIQUE NONCLUSTERED ([UserId] ASC, [GameRoundId] ASC, [CardId] ASC)
) ON [PRIMARY]
GO

-- =============================================
-- Create Indexes
-- =============================================

CREATE NONCLUSTERED INDEX [idx_card_cardtype] ON [dbo].[Card]([CardType] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_card_roundtype] ON [dbo].[Card]([RoundType] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_gamechanger_difficulty] ON [dbo].[GameChanger]([DifficultyLevel] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_gamechanger_roundtype] ON [dbo].[GameChanger]([RoundType] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_gameround_session] ON [dbo].[GameRound]([GameSessionId] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_gamesession_game] ON [dbo].[GameSession]([GameId] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_gamesession_host] ON [dbo].[GameSession]([HostUserId] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_gamesession_status] ON [dbo].[GameSession]([Status] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_ugc_card] ON [dbo].[UGC]([CardId] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_ugc_gamechanger] ON [dbo].[UGC]([GameChangerId] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_ugc_round] ON [dbo].[UGC]([GameRoundId] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_ugc_session] ON [dbo].[UGC]([GameSessionId] ASC)
GO

CREATE NONCLUSTERED INDEX [idx_ugc_user] ON [dbo].[UGC]([UserId] ASC)
GO

-- =============================================
-- Create Foreign Key Constraints
-- =============================================

ALTER TABLE [dbo].[GameSession]  WITH CHECK ADD  
CONSTRAINT [fk_gamesession_game] FOREIGN KEY([GameId])
REFERENCES [dbo].[Game] ([GameId])
GO
ALTER TABLE [dbo].[GameSession] CHECK CONSTRAINT [fk_gamesession_game]
GO

ALTER TABLE [dbo].[GameSession]  WITH CHECK ADD  
CONSTRAINT [fk_gamesession_host] FOREIGN KEY([HostUserId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[GameSession] CHECK CONSTRAINT [fk_gamesession_host]
GO

ALTER TABLE [dbo].[GameRound]  WITH CHECK ADD  
CONSTRAINT [fk_gameround_session] FOREIGN KEY([GameSessionId])
REFERENCES [dbo].[GameSession] ([GameSessionId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GameRound] CHECK CONSTRAINT [fk_gameround_session]
GO

ALTER TABLE [dbo].[UGC]  WITH CHECK ADD  
CONSTRAINT [fk_ugc_card] FOREIGN KEY([CardId])
REFERENCES [dbo].[Card] ([CardId])
GO
ALTER TABLE [dbo].[UGC] CHECK CONSTRAINT [fk_ugc_card]
GO

ALTER TABLE [dbo].[UGC]  WITH CHECK ADD  
CONSTRAINT [fk_ugc_gamechanger] FOREIGN KEY([GameChangerId])
REFERENCES [dbo].[GameChanger] ([GameChangerId])
GO
ALTER TABLE [dbo].[UGC] CHECK CONSTRAINT [fk_ugc_gamechanger]
GO

ALTER TABLE [dbo].[UGC]  WITH CHECK ADD  
CONSTRAINT [fk_ugc_round] FOREIGN KEY([GameRoundId])
REFERENCES [dbo].[GameRound] ([GameRoundId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UGC] CHECK CONSTRAINT [fk_ugc_round]
GO

ALTER TABLE [dbo].[UGC]  WITH CHECK ADD  
CONSTRAINT [fk_ugc_session] FOREIGN KEY([GameSessionId])
REFERENCES [dbo].[GameSession] ([GameSessionId])
GO
ALTER TABLE [dbo].[UGC] CHECK CONSTRAINT [fk_ugc_session]
GO

ALTER TABLE [dbo].[UGC]  WITH CHECK ADD  
CONSTRAINT [fk_ugc_user] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[UGC] CHECK CONSTRAINT [fk_ugc_user]
GO

-- =============================================
-- Record Migrations in EF History
-- =============================================

INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES 
	('20260326005009_InitialCreateV2', '9.0.14'),
	('20260326005654_AddIsEducatorToUser', '9.0.14'),
	('20260408200759_GameChanger_Modifications', '9.0.14')
GO

PRINT 'Database DealtHandsDBV2 created successfully with all migrations applied'
GO

USE [master]
GO