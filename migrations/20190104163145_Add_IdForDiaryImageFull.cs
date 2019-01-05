using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class Add_IdForDiaryImageFull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /** This was fixed in previous migration by SQL **/
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_FullSizeImages_Images_Id",
                table: "FullSizeImages");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FullSizeImages",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "FullSizeImages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FullSizeImages_ImageId",
                table: "FullSizeImages",
                column: "ImageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FullSizeImages_Images_ImageId",
                table: "FullSizeImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
                */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_FullSizeImages_Images_ImageId",
                table: "FullSizeImages");

            migrationBuilder.DropIndex(
                name: "IX_FullSizeImages_ImageId",
                table: "FullSizeImages");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "FullSizeImages");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FullSizeImages",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddForeignKey(
                name: "FK_FullSizeImages_Images_Id",
                table: "FullSizeImages",
                column: "Id",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
                */
        }
    }
}
