using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            migrationBuilder.CreateTable(
#pragma warning restore CA1062 // Validate arguments of public methods
                name: "AppSettings",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    Thumbnail = table.Column<byte[]>(nullable: true),
                    Width = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    SizeByte = table.Column<int>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ScopeName = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FullSizeImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ImageId = table.Column<Guid>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullSizeImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FullSizeImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TempImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SourceImageId = table.Column<Guid>(nullable: false),
                    Modification = table.Column<string>(nullable: true),
                    Data = table.Column<byte[]>(nullable: true),
                    Width = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    SizeByte = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TempImages_Images_SourceImageId",
                        column: x => x.SourceImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cogitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RecordId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cogitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cogitations_Records_RecordId",
                        column: x => x.RecordId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecordImages",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(nullable: false),
                    RecordId = table.Column<Guid>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordImages", x => new { x.RecordId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_RecordImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecordImages_Records_RecordId",
                        column: x => x.RecordId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ScopeId = table.Column<Guid>(nullable: false),
                    ThemeName = table.Column<string>(nullable: true),
                    Actual = table.Column<bool>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Themes_Scopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "Scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecordThemes",
                columns: table => new
                {
                    ThemeId = table.Column<Guid>(nullable: false),
                    RecordId = table.Column<Guid>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordThemes", x => new { x.RecordId, x.ThemeId });
                    table.ForeignKey(
                        name: "FK_RecordThemes_Records_RecordId",
                        column: x => x.RecordId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecordThemes_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cogitations_RecordId",
                table: "Cogitations",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_FullSizeImages_ImageId",
                table: "FullSizeImages",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordImages_ImageId",
                table: "RecordImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordImages_RecordId_ImageId",
                table: "RecordImages",
                columns: new[] { "RecordId", "ImageId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecordThemes_ThemeId",
                table: "RecordThemes",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordThemes_RecordId_ThemeId",
                table: "RecordThemes",
                columns: new[] { "RecordId", "ThemeId" });

            migrationBuilder.CreateIndex(
                name: "IX_TempImages_SourceImageId",
                table: "TempImages",
                column: "SourceImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_ScopeId",
                table: "Themes",
                column: "ScopeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            migrationBuilder.DropTable(
#pragma warning restore CA1062 // Validate arguments of public methods
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "Cogitations");

            migrationBuilder.DropTable(
                name: "FullSizeImages");

            migrationBuilder.DropTable(
                name: "RecordImages");

            migrationBuilder.DropTable(
                name: "RecordThemes");

            migrationBuilder.DropTable(
                name: "TempImages");

            migrationBuilder.DropTable(
                name: "Records");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Scopes");
        }
    }
}
