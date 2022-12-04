using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiseDiary.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class DeleteScaleSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from AppSettings where Key in ('CropImageMaxScaledWidth', 'CropImageMaxScaledHeight')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
