using Microsoft.EntityFrameworkCore.Migrations;

namespace AccountServer.Migrations
{
    public partial class AddNameGoogleIDToAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Account_AccountName",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "Account");

            migrationBuilder.AddColumn<string>(
                name: "GoogleID",
                table: "Account",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Account",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Name",
                table: "Account",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Account_Name",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "GoogleID",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Account");

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "Account",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountName",
                table: "Account",
                column: "AccountName",
                unique: true,
                filter: "[AccountName] IS NOT NULL");
        }
    }
}
