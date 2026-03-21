using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealtHands.Migrations
{
    /// <inheritdoc />
    public partial class AddEducatorAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducatorId",
                table: "Sessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Educators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educators", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_EducatorId",
                table: "Sessions",
                column: "EducatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Educators_EducatorId",
                table: "Sessions",
                column: "EducatorId",
                principalTable: "Educators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Educators_EducatorId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Educators");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_EducatorId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "EducatorId",
                table: "Sessions");
        }
    }
}
