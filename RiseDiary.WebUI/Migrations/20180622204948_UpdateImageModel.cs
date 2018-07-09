using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class UpdateImageModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FullSizeImages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullSizeImages", x => x.Id);
                });

            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
 
              CREATE TABLE Images_temp AS SELECT * FROM Images;
              
              INSERT INTO FullSizeImages (Id, Data)
              SELECT Id, Data FROM Images_temp;

              DROP TABLE Images;
              
              CREATE TABLE Images ( 
                Id INTEGER NOT NULL CONSTRAINT PK_Images PRIMARY KEY AUTOINCREMENT, 
                Name TEXT NULL, 
                CreateDate TEXT NOT NULL, 
                ModifyDate TEXT NOT NULL,
                Thumbnail BLOB NULL, 
                Width INTEGER NOT NULL DEFAULT 0,
                Height INTEGER NOT NULL DEFAULT 0,
                SizeByte INTEGER NOT NULL DEFAULT 0,
                Deleted INTEGER NOT NULL DEFAULT 0);             
              
              INSERT INTO Images 
              (
                  Id,
                  Name,
                  CreateDate,
                  ModifyDate,
                  Thumbnail,
                  Deleted
              )
              SELECT Id,
                     Name,
                     CreateDate,
                     CreateDate,
                     NULL,
                     Deleted
              FROM Images_temp;
              
              DROP TABLE Images_temp;
              
              PRAGMA foreign_keys = 1;                
              ");         
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "FullSizeImages");

            //migrationBuilder.DropColumn(
            //    name: "Height",
            //    table: "Images");

            //migrationBuilder.DropColumn(
            //    name: "ModifyDate",
            //    table: "Images");

            //migrationBuilder.DropColumn(
            //    name: "SizeBit",
            //    table: "Images");

            //migrationBuilder.DropColumn(
            //    name: "Width",
            //    table: "Images");

            //migrationBuilder.RenameColumn(
            //    name: "Thumbnail",
            //    table: "Images",
            //    newName: "Data");
        }
    }
}
