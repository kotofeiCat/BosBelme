using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BosBelme.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Users_IdPlayer",
                table: "GameSessions");

            migrationBuilder.RenameColumn(
                name: "IdPlayer",
                table: "GameSessions",
                newName: "PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_GameSessions_IdPlayer",
                table: "GameSessions",
                newName: "IX_GameSessions_PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Users_PlayerId",
                table: "GameSessions",
                column: "PlayerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_Users_PlayerId",
                table: "GameSessions");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "GameSessions",
                newName: "IdPlayer");

            migrationBuilder.RenameIndex(
                name: "IX_GameSessions_PlayerId",
                table: "GameSessions",
                newName: "IX_GameSessions_IdPlayer");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_Users_IdPlayer",
                table: "GameSessions",
                column: "IdPlayer",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
