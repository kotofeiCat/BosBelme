using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BosBelme.Data.Migrations
{
    /// <inheritdoc />
    public partial class KakNazvat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayersCount_Games_GameId",
                table: "PlayersCount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayersCount",
                table: "PlayersCount");

            migrationBuilder.RenameTable(
                name: "PlayersCount",
                newName: "PlayersCounts");

            migrationBuilder.RenameIndex(
                name: "IX_PlayersCount_GameId",
                table: "PlayersCounts",
                newName: "IX_PlayersCounts_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayersCounts",
                table: "PlayersCounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayersCounts_Games_GameId",
                table: "PlayersCounts",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayersCounts_Games_GameId",
                table: "PlayersCounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayersCounts",
                table: "PlayersCounts");

            migrationBuilder.RenameTable(
                name: "PlayersCounts",
                newName: "PlayersCount");

            migrationBuilder.RenameIndex(
                name: "IX_PlayersCounts_GameId",
                table: "PlayersCount",
                newName: "IX_PlayersCount_GameId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayersCount",
                table: "PlayersCount",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayersCount_Games_GameId",
                table: "PlayersCount",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
