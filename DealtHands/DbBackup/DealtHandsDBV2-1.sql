-- ============================================================
-- BehaviorGameDB v6  (6-table reduced schema)
--
-- Tables:
--   User           - registered players and hosts
--   Game           - game definition / template
--   GameSession    - a single playthrough instance of a Game
--   GameRound      - one round within a GameSession
--   Card           - static card library
--   UGC            - User-Game-Card: one row per user per card
--                    interaction; the central activity log
--
-- Relationship summary:
--   GameSession  → Game         (many sessions per game)
--   GameSession  → User         (host FK)
--   GameRound    → GameSession  (many rounds per session)
--   UGC          → User         (which player)
--   UGC          → Card         (which card they interacted with)
--   UGC          → GameRound    (which round it occurred in)
--   UGC          → GameSession  (which session it occurred in)
--   Card         → UGC only     (Card has no other FK relationships)
--
-- SQL Server cascade note:
--   Deleting a GameSession can reach UGC via two paths:
--     GameSession → GameRound → UGC
--     GameSession → UGC (direct)
--   SQL Server forbids multiple cascade paths to the same table.
--   Resolution: GameRound → UGC owns CASCADE DELETE.
--               GameSession → UGC (direct) is NO ACTION.
--   Application layer must delete UGC rows before deleting a
--   GameSession directly (GameRound cascade handles the rest).
-- ============================================================

--USE master;
--GO
--ALTER DATABASE BehaviorGameDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
--GO
--DROP DATABASE IF EXISTS BehaviorGameDB;
--GO
--CREATE DATABASE BehaviorGameDB;
--GO
--USE BehaviorGameDB;
--GO


USE DealtHandsDBV2;
GO

-- ============================================================
-- 1. User
-- ============================================================
CREATE TABLE [User] (
  UserId       BIGINT       IDENTITY(1,1) PRIMARY KEY,
  Username     VARCHAR(40)  NOT NULL UNIQUE,
  Email        VARCHAR(120) NOT NULL UNIQUE,
  PasswordHash VARCHAR(255) NULL,
  CreatedAt    DATETIME     NOT NULL DEFAULT GETUTCDATE(),
  IsActive     BIT          NOT NULL DEFAULT 1
);

-- ============================================================
-- 2. Game
--    The definition/template of a game type. A Game can have
--    many GameSessions (playthroughs) run against it.
-- ============================================================
CREATE TABLE Game (
  GameId      BIGINT      IDENTITY(1,1) PRIMARY KEY,
  Title       VARCHAR(80) NOT NULL,
  Description VARCHAR(255) NULL,
  Mode        VARCHAR(20) NOT NULL,  -- e.g. RandomAssigned, ChooseFromFour, PlayerEntered
  MaxPlayers  INT         NULL,
  CreatedAt   DATETIME    NOT NULL DEFAULT GETUTCDATE(),
  IsActive    BIT         NOT NULL DEFAULT 1
);

-- ============================================================
-- 3. GameSession
--    One playthrough/instance of a Game. Tracks full lifecycle
--    of a session (waiting → in progress → completed).
--    HostUserId records who is running this session.
-- ============================================================
CREATE TABLE GameSession (
  GameSessionId      BIGINT      IDENTITY(1,1) PRIMARY KEY,
  GameId             BIGINT      NOT NULL,       -- which game definition this runs
  HostUserId         BIGINT      NOT NULL,       -- user hosting this session
  JoinCode           VARCHAR(12) NOT NULL UNIQUE,
  Status             VARCHAR(20) NOT NULL DEFAULT 'Waiting',  -- Waiting, InProgress, Paused, Completed, Abandoned
  CurrentRoundNumber TINYINT     NOT NULL DEFAULT 1,
  CreatedAt          DATETIME    NOT NULL DEFAULT GETUTCDATE(),
  StartedAt          DATETIME    NULL,
  PausedAt           DATETIME    NULL,
  ResumedAt          DATETIME    NULL,
  EndedAt            DATETIME    NULL,
  IsActive           BIT         NOT NULL DEFAULT 1,
  CONSTRAINT fk_gamesession_game FOREIGN KEY (GameId)
    REFERENCES Game(GameId)
    ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT fk_gamesession_host FOREIGN KEY (HostUserId)
    REFERENCES [User](UserId)
    ON DELETE NO ACTION ON UPDATE NO ACTION
);

-- ============================================================
-- 4. GameRound
--    One round within a GameSession. RoundType determines which
--    category of cards are used. The host controls its lifecycle.
-- ============================================================
CREATE TABLE GameRound (
  GameRoundId   BIGINT      IDENTITY(1,1) PRIMARY KEY,
  GameSessionId BIGINT      NOT NULL,
  RoundNumber   TINYINT     NOT NULL,
  RoundType     VARCHAR(20) NOT NULL,  -- StudentLoan, CarPayment, Housing, Investment, Insurance, Misc
  Status        VARCHAR(20) NOT NULL DEFAULT 'NotStarted',  -- NotStarted, Open, Paused, Closed
  OpenedAt      DATETIME    NULL,
  ClosedAt      DATETIME    NULL,
  IsActive      BIT         NOT NULL DEFAULT 1,
  CONSTRAINT uq_gameround_session_roundnum UNIQUE (GameSessionId, RoundNumber),
  CONSTRAINT fk_gameround_session FOREIGN KEY (GameSessionId)
    REFERENCES GameSession(GameSessionId)
    ON DELETE CASCADE ON UPDATE CASCADE
);

-- ============================================================
-- 5. Card
--    Static card library. Each card is a reusable financial
--    scenario (e.g. a 2022 Toyota Camry car payment card).
--    FieldData stores card-type-specific fields as JSON.
--    Card has no outbound FKs — it is referenced only by UGC.
-- ============================================================
CREATE TABLE Card (
  CardId        INT           IDENTITY(1,1) PRIMARY KEY,
  RoundType     VARCHAR(20)   NOT NULL,   -- matches GameRound.RoundType
  CardType      VARCHAR(20)   NOT NULL DEFAULT 'RoundCard',  -- RoundCard, GameChangerCard
  Title         VARCHAR(80)   NOT NULL,
  Description   VARCHAR(255)  NULL,
  MonthlyAmount DECIMAL(10,2) NULL,       -- headline amount for display/sorting
  FieldData     NVARCHAR(MAX) NULL,       -- JSON: card-type-specific fields
  IsActive      BIT           NOT NULL DEFAULT 1
);

-- ============================================================
-- 6. UGC  (User-Game-Card)
--    One row per user per card interaction within a round.
--    This is the central activity log of the game.
--    Tracks which card a user was shown, their submitted
--    answer, and timestamps for assignment and submission.
--
--    Cascade note: GameRound → UGC is CASCADE so that closing
--    out a round's data cascades automatically. GameSession →
--    UGC is NO ACTION to avoid SQL Server's multiple cascade
--    path error (see file header note).
-- ============================================================
CREATE TABLE UGC (
  UGCId           BIGINT        IDENTITY(1,1) PRIMARY KEY,

  -- Who, what, where
  UserId          BIGINT        NOT NULL,  -- which player
  CardId          INT           NOT NULL,  -- which card they were given
  GameRoundId     BIGINT        NOT NULL,  -- which round it happened in
  GameSessionId   BIGINT        NOT NULL,  -- which session it happened in

  -- Interaction data
  SubmittedAmount DECIMAL(10,2) NULL,      -- player's answer; NULL until submitted
  RunningTotal    DECIMAL(10,2) NULL,      -- player's cumulative score after this
                                           -- submission across all prior rounds in
                                           -- the session; NULL until submitted.
                                           -- Set by the application at submit time.

  -- Timestamps
  AssignedAt      DATETIME      NOT NULL DEFAULT GETUTCDATE(),
  SubmittedAt     DATETIME      NULL,      -- NULL until the player submits

  IsActive        BIT           NOT NULL DEFAULT 1,

  -- One card interaction per user per round (one card, one submission, done)
  CONSTRAINT uq_ugc_user_round_card UNIQUE (UserId, GameRoundId, CardId),

  CONSTRAINT fk_ugc_user    FOREIGN KEY (UserId)
    REFERENCES [User](UserId)
    ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT fk_ugc_card    FOREIGN KEY (CardId)
    REFERENCES Card(CardId)
    ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT fk_ugc_round   FOREIGN KEY (GameRoundId)
    REFERENCES GameRound(GameRoundId)
    ON DELETE CASCADE  ON UPDATE CASCADE,   -- owns the cascade from GameRound
  CONSTRAINT fk_ugc_session FOREIGN KEY (GameSessionId)
    REFERENCES GameSession(GameSessionId)
    ON DELETE NO ACTION ON UPDATE NO ACTION -- NO ACTION: avoids multiple cascade path error
);

-- ============================================================
-- Indexes
-- ============================================================
CREATE INDEX idx_gamesession_game    ON GameSession (GameId);
CREATE INDEX idx_gamesession_host    ON GameSession (HostUserId);
CREATE INDEX idx_gamesession_status  ON GameSession (Status);
CREATE INDEX idx_gameround_session   ON GameRound   (GameSessionId);
CREATE INDEX idx_card_roundtype      ON Card        (RoundType);
CREATE INDEX idx_card_cardtype       ON Card        (CardType);
CREATE INDEX idx_ugc_user            ON UGC         (UserId);
CREATE INDEX idx_ugc_round           ON UGC         (GameRoundId);
CREATE INDEX idx_ugc_session         ON UGC         (GameSessionId);
CREATE INDEX idx_ugc_card            ON UGC         (CardId);
