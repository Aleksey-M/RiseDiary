using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class AddedModelsProperty_Deleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Themes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Scopes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Records",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Images",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Cogitations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Scopes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Records");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Cogitations");
        }
    }
}
