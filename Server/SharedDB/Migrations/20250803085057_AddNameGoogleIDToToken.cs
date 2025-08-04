using Microsoft.EntityFrameworkCore.Migrations;

namespace SharedDB.Migrations
{
    public partial class AddNameGoogleIDToToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleID",
                table: "Token",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Token",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleID",
                table: "Token");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Token");
        }
    }
}
