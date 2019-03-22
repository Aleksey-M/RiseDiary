using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class AddedProperties_Code : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Themes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Scopes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Records",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Images",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Cogitations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_Code",
                table: "Themes",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Scopes_Code",
                table: "Scopes",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Records_Code",
                table: "Records",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Images_Code",
                table: "Images",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Cogitations_Code",
                table: "Cogitations",
                column: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Themes_Code",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Scopes_Code",
                table: "Scopes");

            migrationBuilder.DropIndex(
                name: "IX_Records_Code",
                table: "Records");

            migrationBuilder.DropIndex(
                name: "IX_Images_Code",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Cogitations_Code",
                table: "Cogitations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Scopes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Cogitations");
        }
    }
}
