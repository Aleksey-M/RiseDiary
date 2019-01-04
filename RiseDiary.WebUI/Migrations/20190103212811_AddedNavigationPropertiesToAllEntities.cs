using Microsoft.EntityFrameworkCore.Migrations;

namespace RiseDiary.WebUI.Migrations
{
    public partial class AddedNavigationPropertiesToAllEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /** SQLite migration **/

            /** sql при миграции вызывает ошибку. Был выполнен в SQL Browser (отдельной утилите), 
             * потом была выполнена миграция для создания индексов **/
            /*
            migrationBuilder.Sql(
            @"PRAGMA foreign_keys = 0;
            
            CREATE TABLE FullSizeImages_temp AS SELECT * FROM FullSizeImages;    
            DROP TABLE FullSizeImages;        

            CREATE TABLE FullSizeImages ( 
                Id INTEGER NOT NULL CONSTRAINT PK_FullSizeImages PRIMARY KEY AUTOINCREMENT,
                ImageId INTEGER NOT NULL, 
                Data BLOB NULL,
                CONSTRAINT FK_FullSizeImages_Images_Id 
                    FOREIGN KEY (ImageId) 
                    REFERENCES Images(Id) 
                    ON DELETE CASCADE); 

            INSERT INTO FullSizeImages ( ImageId, Data )
            SELECT Id, Data                
            FROM FullSizeImages_temp;

            DROP TABLE FullSizeImages_temp;


            DROP TABLE TempImages;
            CREATE TABLE TempImages ( 
                Id INTEGER NOT NULL CONSTRAINT PK_TempImages PRIMARY KEY AUTOINCREMENT, 
                SourceImageId INTEGER NOT NULL, 
                Modification TEXT NULL, 
                Data BLOB NULL, 
                Width INTEGER NOT NULL, 
                Height INTEGER NOT NULL, 
                SizeByte INTEGER NOT NULL, 
                CONSTRAINT FK_TempImages_Images_SourceImageId 
                    FOREIGN KEY (SourceImageId) 
                    REFERENCES Images(Id) 
                    ON DELETE CASCADE);


            CREATE TABLE Cogitations_temp AS SELECT * FROM Cogitations;
            DROP TABLE Cogitations;

            CREATE TABLE Cogitations ( 
                Id INTEGER NOT NULL CONSTRAINT PK_Cogitations PRIMARY KEY AUTOINCREMENT, 
                RecordId INTEGER NOT NULL, 
                Date TEXT NOT NULL, 
                Text TEXT NULL , 
                Deleted INTEGER NOT NULL DEFAULT 0,
                CONSTRAINT FK_Cogitations_Records_RecordId 
                    FOREIGN KEY (RecordId) 
                    REFERENCES Records(Id) 
                    ON DELETE CASCADE);

            INSERT INTO Cogitations (Id, RecordId, Date, Text, Deleted)
            SELECT Id, RecordId, Date, Text, Deleted
            FROM Cogitations_temp;

            DROP TABLE Cogitations_temp;


            CREATE TABLE Themes_temp AS SELECT * FROM Themes;
            DROP TABLE Themes;

            CREATE TABLE Themes ( 
                Id INTEGER NOT NULL CONSTRAINT PK_Themes PRIMARY KEY AUTOINCREMENT, 
                ScopeId INTEGER NOT NULL, 
                ThemeName TEXT, 
                Deleted INTEGER NOT NULL DEFAULT 0, 
                Actual INTEGER NOT NULL DEFAULT 0,
                CONSTRAINT FK_Themes_Scopes_ScopeId 
                    FOREIGN KEY (ScopeId) 
                    REFERENCES Scopes(Id) 
                    ON DELETE CASCADE);
            
            INSERT INTO Themes (Id, ScopeId, ThemeName, Deleted, Actual)
            SELECT Id, ScopeId, ThemeName, Deleted, Actual
            FROM Themes_temp;

            DROP TABLE Themes_temp;


            CREATE TABLE RecordImages_temp AS SELECT * FROM RecordImages;
            DROP TABLE RecordImages;

            CREATE TABLE RecordImages ( 
                ImageId INTEGER NOT NULL, 
                RecordId INTEGER NOT NULL, 
                Deleted INTEGER NOT NULL DEFAULT 0, 
                CONSTRAINT PK_RecordImages PRIMARY KEY (RecordId, ImageId),
                CONSTRAINT FK_RecordImages_Images_ImageId 
                    FOREIGN KEY (ImageId) 
                    REFERENCES Images(Id) 
                    ON DELETE CASCADE,
                CONSTRAINT FK_RecordImages_Records_RecordId 
                    FOREIGN KEY (RecordId) 
                    REFERENCES Records(Id) 
                    ON DELETE CASCADE);

            INSERT INTO RecordImages (ImageId, RecordId, Deleted)
            SELECT ImageId, RecordId, Deleted
            FROM RecordImages_temp
            WHERE RecordId <> 0;

            DROP TABLE RecordImages_temp;



            CREATE TABLE RecordThemes_temp AS SELECT * FROM RecordThemes;
            DROP TABLE RecordThemes;

            CREATE TABLE RecordThemes ( 
                ThemeId INTEGER NOT NULL, 
                RecordId INTEGER NOT NULL, 
                Deleted INTEGER NOT NULL DEFAULT 0, 
                CONSTRAINT PK_RecordThemes PRIMARY KEY (RecordId, ThemeId),
                CONSTRAINT FK_RecordThemes_Records_RecordId 
                    FOREIGN KEY (RecordId) 
                    REFERENCES Records(Id) 
                    ON DELETE CASCADE,
                CONSTRAINT FK_RecordThemes_Themes_ThemeId 
                    FOREIGN KEY (ThemeId) 
                    REFERENCES Themes(Id) 
                    ON DELETE CASCADE);

            INSERT INTO RecordThemes (ThemeId, RecordId, Deleted)
            SELECT ThemeId, RecordId, Deleted
            FROM RecordThemes_temp;

            DROP TABLE RecordThemes_temp;


              PRAGMA foreign_keys = 1;
            ");
            */
            /*
          + migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FullSizeImages",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("Sqlite:Autoincrement", true);                
            
           + migrationBuilder.AddForeignKey(
                name: "FK_Cogitations_Records_RecordId",
                table: "Cogitations",
                column: "RecordId",
                principalTable: "Records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

           + migrationBuilder.AddForeignKey(
                name: "FK_FullSizeImages_Images_Id",
                table: "FullSizeImages",
                column: "Id",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

          +  migrationBuilder.AddForeignKey(
                name: "FK_RecordImages_Images_ImageId",
                table: "RecordImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

          +  migrationBuilder.AddForeignKey(
                name: "FK_RecordImages_Records_RecordId",
                table: "RecordImages",
                column: "RecordId",
                principalTable: "Records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

          +  migrationBuilder.AddForeignKey(
                name: "FK_RecordThemes_Records_RecordId",
                table: "RecordThemes",
                column: "RecordId",
                principalTable: "Records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

           + migrationBuilder.AddForeignKey(
                name: "FK_RecordThemes_Themes_ThemeId",
                table: "RecordThemes",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

           + migrationBuilder.AddForeignKey(
                name: "FK_TempImages_Images_SourceImageId",
                table: "TempImages",
                column: "SourceImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

           + migrationBuilder.AddForeignKey(
                name: "FK_Themes_Scopes_ScopeId",
                table: "Themes",
                column: "ScopeId",
                principalTable: "Scopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);      
            */

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
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
                */
        }
    }
}
