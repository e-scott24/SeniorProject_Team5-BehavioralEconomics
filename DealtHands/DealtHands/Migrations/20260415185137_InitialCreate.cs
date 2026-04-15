using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealtHands.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CardType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "RoundCard"),
                    Title = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FieldData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DifficultyLevel = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    RequiresStudentLoans = table.Column<bool>(type: "bit", nullable: true),
                    RequiresCar = table.Column<bool>(type: "bit", nullable: true),
                    RequiresCarLoan = table.Column<bool>(type: "bit", nullable: true),
                    RequiresOwnsHome = table.Column<bool>(type: "bit", nullable: true),
                    RequiresApartment = table.Column<bool>(type: "bit", nullable: true),
                    RequiresRoommate = table.Column<bool>(type: "bit", nullable: true),
                    RequiresMarried = table.Column<bool>(type: "bit", nullable: true),
                    RequiresChildren = table.Column<bool>(type: "bit", nullable: true),
                    SetsStudentLoans = table.Column<bool>(type: "bit", nullable: true),
                    SetsCar = table.Column<bool>(type: "bit", nullable: true),
                    SetsCarLoan = table.Column<bool>(type: "bit", nullable: true),
                    SetsOwnsHome = table.Column<bool>(type: "bit", nullable: true),
                    SetsApartment = table.Column<bool>(type: "bit", nullable: true),
                    SetsRoommate = table.Column<bool>(type: "bit", nullable: true),
                    SetsMarried = table.Column<bool>(type: "bit", nullable: true),
                    SetsChildren = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Card__55FECDAE04C2E5A4", x => x.CardId);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    GameId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Mode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getutcdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Game__2AB897FDEF6AACC0", x => x.GameId);
                });

            migrationBuilder.CreateTable(
                name: "GameChanger",
                columns: table => new
                {
                    GameChangerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    RoundType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DifficultyLevel = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequiresApartment = table.Column<bool>(type: "bit", nullable: true),
                    RequiresCar = table.Column<bool>(type: "bit", nullable: true),
                    RequiresCarLoan = table.Column<bool>(type: "bit", nullable: true),
                    RequiresOwnsHome = table.Column<bool>(type: "bit", nullable: true),
                    RequiresStudentLoans = table.Column<bool>(type: "bit", nullable: true),
                    RequiresRoommate = table.Column<bool>(type: "bit", nullable: true),
                    RequiresMarried = table.Column<bool>(type: "bit", nullable: true),
                    RequiresChildren = table.Column<bool>(type: "bit", nullable: true),
                    RequiresJob = table.Column<bool>(type: "bit", nullable: true),
                    IncomeEffect = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IncomeEffectPercent = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    ExpenseEffect = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SetsApartment = table.Column<bool>(type: "bit", nullable: true),
                    SetsCar = table.Column<bool>(type: "bit", nullable: true),
                    SetsCarLoan = table.Column<bool>(type: "bit", nullable: true),
                    SetsOwnsHome = table.Column<bool>(type: "bit", nullable: true),
                    SetsStudentLoans = table.Column<bool>(type: "bit", nullable: true),
                    SetsRoommate = table.Column<bool>(type: "bit", nullable: true),
                    SetsMarried = table.Column<bool>(type: "bit", nullable: true),
                    SetsChildren = table.Column<bool>(type: "bit", nullable: true),
                    SetsJob = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameChanger", x => x.GameChangerId);
                });

            migrationBuilder.CreateTable(
                name: "LifeSituation",
                columns: table => new
                {
                    LifeSituationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeSituation", x => x.LifeSituationId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getutcdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsEducator = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PasswordResetToken = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    PasswordResetExpires = table.Column<DateTime>(type: "datetime", nullable: true),
                    HasStudentLoans = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasCar = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasCarLoan = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OwnsHome = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasApartment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasRoommate = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsMarried = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasChildren = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasJob = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__1788CC4C7489493E", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CardLifeSituation",
                columns: table => new
                {
                    CardLifeSituationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    LifeSituationId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardLifeSituation", x => x.CardLifeSituationId);
                    table.ForeignKey(
                        name: "FK_CardLifeSituation_Card",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardLifeSituation_LifeSituation",
                        column: x => x.LifeSituationId,
                        principalTable: "LifeSituation",
                        principalColumn: "LifeSituationId");
                });

            migrationBuilder.CreateTable(
                name: "GameChangerLifeSituation",
                columns: table => new
                {
                    GameChangerLifeSituationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameChangerId = table.Column<int>(type: "int", nullable: false),
                    LifeSituationId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameChangerLifeSituation", x => x.GameChangerLifeSituationId);
                    table.ForeignKey(
                        name: "FK_GameChangerLifeSituation_GameChanger",
                        column: x => x.GameChangerId,
                        principalTable: "GameChanger",
                        principalColumn: "GameChangerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameChangerLifeSituation_LifeSituation",
                        column: x => x.LifeSituationId,
                        principalTable: "LifeSituation",
                        principalColumn: "LifeSituationId");
                });

            migrationBuilder.CreateTable(
                name: "GameSession",
                columns: table => new
                {
                    GameSessionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    HostUserId = table.Column<long>(type: "bigint", nullable: false),
                    JoinCode = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Difficulty = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Waiting"),
                    CurrentRoundNumber = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getutcdate())"),
                    StartedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    PausedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ResumedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GameSess__E588370D2F5B90D0", x => x.GameSessionId);
                    table.ForeignKey(
                        name: "fk_gamesession_game",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "GameId");
                    table.ForeignKey(
                        name: "fk_gamesession_host",
                        column: x => x.HostUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "GameRound",
                columns: table => new
                {
                    GameRoundId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameSessionId = table.Column<long>(type: "bigint", nullable: false),
                    RoundNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    RoundType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "NotStarted"),
                    OpenedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GameRoun__003F8F05D8CB7DE5", x => x.GameRoundId);
                    table.ForeignKey(
                        name: "fk_gameround_session",
                        column: x => x.GameSessionId,
                        principalTable: "GameSession",
                        principalColumn: "GameSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UGC",
                columns: table => new
                {
                    UGCId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    GameChangerId = table.Column<int>(type: "int", nullable: true),
                    GameRoundId = table.Column<long>(type: "bigint", nullable: false),
                    GameSessionId = table.Column<long>(type: "bigint", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getutcdate())"),
                    SubmittedAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    RunningTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UGC__DC0AF6F2A9F2DF4B", x => x.UGCId);
                    table.ForeignKey(
                        name: "FK_UGC_GameSession_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSession",
                        principalColumn: "GameSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ugc_card",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "CardId");
                    table.ForeignKey(
                        name: "fk_ugc_gamechanger",
                        column: x => x.GameChangerId,
                        principalTable: "GameChanger",
                        principalColumn: "GameChangerId");
                    table.ForeignKey(
                        name: "fk_ugc_round",
                        column: x => x.GameRoundId,
                        principalTable: "GameRound",
                        principalColumn: "GameRoundId");
                    table.ForeignKey(
                        name: "fk_ugc_user",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "idx_card_cardtype",
                table: "Card",
                column: "CardType");

            migrationBuilder.CreateIndex(
                name: "idx_card_roundtype",
                table: "Card",
                column: "RoundType");

            migrationBuilder.CreateIndex(
                name: "idx_cardlifesituation_card",
                table: "CardLifeSituation",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "idx_cardlifesituation_lifesituation",
                table: "CardLifeSituation",
                column: "LifeSituationId");

            migrationBuilder.CreateIndex(
                name: "idx_cardlifesituation_relation",
                table: "CardLifeSituation",
                column: "RelationType");

            migrationBuilder.CreateIndex(
                name: "UQ_CardLifeSituation",
                table: "CardLifeSituation",
                columns: new[] { "CardId", "LifeSituationId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_gamechanger_difficulty",
                table: "GameChanger",
                column: "DifficultyLevel");

            migrationBuilder.CreateIndex(
                name: "idx_gamechanger_roundtype",
                table: "GameChanger",
                column: "RoundType");

            migrationBuilder.CreateIndex(
                name: "idx_gamechangerlifesituation_gamechanger",
                table: "GameChangerLifeSituation",
                column: "GameChangerId");

            migrationBuilder.CreateIndex(
                name: "idx_gamechangerlifesituation_lifesituation",
                table: "GameChangerLifeSituation",
                column: "LifeSituationId");

            migrationBuilder.CreateIndex(
                name: "idx_gamechangerlifesituation_relation",
                table: "GameChangerLifeSituation",
                column: "RelationType");

            migrationBuilder.CreateIndex(
                name: "UQ_GameChangerLifeSituation",
                table: "GameChangerLifeSituation",
                columns: new[] { "GameChangerId", "LifeSituationId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_gameround_session",
                table: "GameRound",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "uq_gameround_session_roundnum",
                table: "GameRound",
                columns: new[] { "GameSessionId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_gamesession_game",
                table: "GameSession",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "idx_gamesession_host",
                table: "GameSession",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "idx_gamesession_status",
                table: "GameSession",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UQ__GameSess__FF7C6BA0FE398DA1",
                table: "GameSession",
                column: "JoinCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_LifeSituation_Name",
                table: "LifeSituation",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ugc_card",
                table: "UGC",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "idx_ugc_gamechanger",
                table: "UGC",
                column: "GameChangerId");

            migrationBuilder.CreateIndex(
                name: "idx_ugc_round",
                table: "UGC",
                column: "GameRoundId");

            migrationBuilder.CreateIndex(
                name: "idx_ugc_session",
                table: "UGC",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "idx_ugc_user",
                table: "UGC",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "uq_ugc_user_round_card",
                table: "UGC",
                columns: new[] { "UserId", "GameRoundId", "CardId" },
                unique: true,
                filter: "[CardId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__User__536C85E469253162",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__User__A9D105341A37EEF6",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardLifeSituation");

            migrationBuilder.DropTable(
                name: "GameChangerLifeSituation");

            migrationBuilder.DropTable(
                name: "UGC");

            migrationBuilder.DropTable(
                name: "LifeSituation");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropTable(
                name: "GameChanger");

            migrationBuilder.DropTable(
                name: "GameRound");

            migrationBuilder.DropTable(
                name: "GameSession");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
