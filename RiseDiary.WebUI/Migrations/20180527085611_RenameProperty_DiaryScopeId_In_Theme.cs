using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class RenameProperty_DiaryScopeId_In_Theme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
 
              CREATE TABLE Themes_temp AS SELECT *
                                            FROM Themes;
              
              DROP TABLE Themes;
              
              CREATE TABLE Themes (
	            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	            ScopeId INTEGER NOT NULL,
	            ThemeName TEXT);
              
              INSERT INTO Themes 
              (
                  Id,
                  ScopeId,
                  ThemeName
              )
              SELECT Id,
                     DiaryScopeId,
                     ThemeName
              FROM Themes_temp;
              
              DROP TABLE Themes_temp;
              
              PRAGMA foreign_keys = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"PRAGMA foreign_keys = 0;
 
              CREATE TABLE Themes_temp AS SELECT *
                                            FROM Themes;
              
              DROP TABLE Themes;
              
              CREATE TABLE Themes (
	            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	            DiaryScopeId INTEGER NOT NULL,
	            ThemeName TEXT);
              
              INSERT INTO Themes 
              (
                  Id,
                  DiaryScopeId,
                  ThemeName
              )
              SELECT Id,
                     ScopeId,
                     ThemeName
              FROM Themes_temp;
              
              DROP TABLE Themes_temp;
              
              PRAGMA foreign_keys = 1;");
        }
    }
}
