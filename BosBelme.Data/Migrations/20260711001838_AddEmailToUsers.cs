using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BosBelme.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discription",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionKey",
                table: "GameHubs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "GameHubs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discription",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ConnectionKey",
                table: "GameHubs");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "GameHubs");
        }
    }
}
