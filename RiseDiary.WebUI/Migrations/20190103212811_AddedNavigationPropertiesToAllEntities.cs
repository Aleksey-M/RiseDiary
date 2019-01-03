using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class AddedNavigationPropertiesToAllEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {/*
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FullSizeImages",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_ScopeId",
                table: "Themes",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_TempImages_SourceImageId",
                table: "TempImages",
                column: "SourceImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordThemes_ThemeId",
                table: "RecordThemes",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordThemes_RecordId_ThemeId",
                table: "RecordThemes",
                columns: new[] { "RecordId", "ThemeId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecordImages_ImageId",
                table: "RecordImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordImages_RecordId_ImageId",
                table: "RecordImages",
                columns: new[] { "RecordId", "ImageId" });

            migrationBuilder.CreateIndex(
                name: "IX_Cogitations_RecordId",
                table: "Cogitations",
                column: "RecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cogitations_Records_RecordId",
                table: "Cogitations",
                column: "RecordId",
                principalTable: "Records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FullSizeImages_Images_Id",
                table: "FullSizeImages",
                column: "Id",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordImages_Images_ImageId",
                table: "RecordImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordImages_Records_RecordId",
                table: "RecordImages",
                column: "RecordId",
                principalTable: "Records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordThemes_Records_RecordId",
                table: "RecordThemes",
                column: "RecordId",
                principalTable: "Records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordThemes_Themes_ThemeId",
                table: "RecordThemes",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TempImages_Images_SourceImageId",
                table: "TempImages",
                column: "SourceImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_Scopes_ScopeId",
                table: "Themes",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
                */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cogitations_Records_RecordId",
                table: "Cogitations");

            migrationBuilder.DropForeignKey(
                name: "FK_FullSizeImages_Images_Id",
                table: "FullSizeImages");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordImages_Images_ImageId",
                table: "RecordImages");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordImages_Records_RecordId",
                table: "RecordImages");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordThemes_Records_RecordId",
                table: "RecordThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordThemes_Themes_ThemeId",
                table: "RecordThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_TempImages_Images_SourceImageId",
                table: "TempImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Scopes_ScopeId",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Themes_ScopeId",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_TempImages_SourceImageId",
                table: "TempImages");

            migrationBuilder.DropIndex(
                name: "IX_RecordThemes_ThemeId",
                table: "RecordThemes");

            migrationBuilder.DropIndex(
                name: "IX_RecordThemes_RecordId_ThemeId",
                table: "RecordThemes");

            migrationBuilder.DropIndex(
                name: "IX_RecordImages_ImageId",
                table: "RecordImages");

            migrationBuilder.DropIndex(
                name: "IX_RecordImages_RecordId_ImageId",
                table: "RecordImages");

            migrationBuilder.DropIndex(
                name: "IX_Cogitations_RecordId",
                table: "Cogitations");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FullSizeImages",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
