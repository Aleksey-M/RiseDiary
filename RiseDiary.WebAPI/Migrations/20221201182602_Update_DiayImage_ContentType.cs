using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiseDiary.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiayImageContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update Images set ContentType = 'image/jpeg'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
