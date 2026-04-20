using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealtHands.Migrations
{
    /// <inheritdoc />
    public partial class GameChanger_Modifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ugc_session",
                table: "UGC");

            migrationBuilder.DropPrimaryKey(
                name: "PK__UGC__DC0AF6F268FF4BFD",
                table: "UGC");

            migrationBuilder.DropIndex(
                name: "uq_ugc_user_round_card",
                table: "UGC");

            migrationBuilder.AddColumn<bool>(
                name: "HasApartment",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCar",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCarLoan",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasChildren",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasJob",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasRoommate",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStudentLoans",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarried",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OwnsHome",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "CardId",
                table: "UGC",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "GameChangerId",
                table: "UGC",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DifficultyLevel",
                table: "Card",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresApartment",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCar",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCarLoan",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresChildren",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresMarried",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresOwnsHome",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresRoommate",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresStudentLoans",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsApartment",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsCar",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsCarLoan",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsChildren",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsMarried",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsOwnsHome",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsRoommate",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetsStudentLoans",
                table: "Card",
                type: "bit",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__UGC__DC0AF6F2A9F2DF4B",
                table: "UGC",
                column: "UGCId");

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

            migrationBuilder.CreateIndex(
                name: "idx_ugc_gamechanger",
                table: "UGC",
                column: "GameChangerId");

            migrationBuilder.CreateIndex(
                name: "uq_ugc_user_round_card",
                table: "UGC",
                columns: new[] { "UserId", "GameRoundId", "CardId" },
                unique: true,
                filter: "[CardId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_gamechanger_difficulty",
                table: "GameChanger",
                column: "DifficultyLevel");

            migrationBuilder.CreateIndex(
                name: "idx_gamechanger_roundtype",
                table: "GameChanger",
                column: "RoundType");

            migrationBuilder.AddForeignKey(
                name: "FK_UGC_GameSession_GameSessionId",
                table: "UGC",
                column: "GameSessionId",
                principalTable: "GameSession",
                principalColumn: "GameSessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_ugc_gamechanger",
                table: "UGC",
                column: "GameChangerId",
                principalTable: "GameChanger",
                principalColumn: "GameChangerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UGC_GameSession_GameSessionId",
                table: "UGC");

            migrationBuilder.DropForeignKey(
                name: "fk_ugc_gamechanger",
                table: "UGC");

            migrationBuilder.DropTable(
                name: "GameChanger");

            migrationBuilder.DropPrimaryKey(
                name: "PK__UGC__DC0AF6F2A9F2DF4B",
                table: "UGC");

            migrationBuilder.DropIndex(
                name: "idx_ugc_gamechanger",
                table: "UGC");

            migrationBuilder.DropIndex(
                name: "uq_ugc_user_round_card",
                table: "UGC");

            migrationBuilder.DropColumn(
                name: "HasApartment",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HasCar",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HasCarLoan",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HasChildren",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HasJob",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HasRoommate",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HasStudentLoans",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsMarried",
                table: "User");

            migrationBuilder.DropColumn(
                name: "OwnsHome",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GameChangerId",
                table: "UGC");

            migrationBuilder.DropColumn(
                name: "DifficultyLevel",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresApartment",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresCar",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresCarLoan",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresChildren",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresMarried",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresOwnsHome",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresRoommate",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "RequiresStudentLoans",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsApartment",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsCar",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsCarLoan",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsChildren",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsMarried",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsOwnsHome",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsRoommate",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "SetsStudentLoans",
                table: "Card");

            migrationBuilder.AlterColumn<int>(
                name: "CardId",
                table: "UGC",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__UGC__DC0AF6F268FF4BFD",
                table: "UGC",
                column: "UGCId");

            migrationBuilder.CreateIndex(
                name: "uq_ugc_user_round_card",
                table: "UGC",
                columns: new[] { "UserId", "GameRoundId", "CardId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_ugc_session",
                table: "UGC",
                column: "GameSessionId",
                principalTable: "GameSession",
                principalColumn: "GameSessionId");
        }
    }
}
