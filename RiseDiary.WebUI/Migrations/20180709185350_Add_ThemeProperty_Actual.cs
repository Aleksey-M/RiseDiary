using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class Add_ThemeProperty_Actual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Actual",
                table: "Themes",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actual",
                table: "Themes");
        }
    }
}
