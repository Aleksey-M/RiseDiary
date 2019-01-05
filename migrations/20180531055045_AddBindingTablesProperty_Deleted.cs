using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class AddBindingTablesProperty_Deleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "RecordThemes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "RecordImages",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "RecordThemes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "RecordImages");
        }
    }
}
