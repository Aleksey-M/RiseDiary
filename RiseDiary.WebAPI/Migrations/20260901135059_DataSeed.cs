using Microsoft.EntityFrameworkCore.Migrations;
using RiseDiary.WebAPI.Settings.Model;

#nullable disable

namespace RiseDiary.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class DataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                INSERT INTO AppSettings (Key, Value, ModifiedDate) VALUES
                ('{(int)SettingsKey.AvailableImagesPageSize}', '5', CURRENT_TIMESTAMP),
                ('{(int)SettingsKey.RecordsPageSize}', '5', CURRENT_TIMESTAMP),
                ('{(int)SettingsKey.ImagesPageSize}', '10', CURRENT_TIMESTAMP),
                ('{(int)SettingsKey.ThumbnailSize}', '140', CURRENT_TIMESTAMP),
                ('{(int)SettingsKey.ImageQuality}', '80', CURRENT_TIMESTAMP),
                ('{(int)SettingsKey.ImportantDaysDisplayRange}', '10', CURRENT_TIMESTAMP);

                INSERT INTO Scopes (Id, ScopeName, Description, CreateDate, ModifyDate, Deleted)
                VALUES (
                    '98366e10-a24b-46f4-be0b-d11f96ff78d6',
                    'Важные даты',
                    'Группа записей, даты которых будут выводиться в разделе "Даты"',
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP,
                    0
                );

                INSERT INTO Themes (Id, ScopeId, ThemeName, Description, Actual, Deleted, CreateDate, ModifyDate)
                VALUES
                    ('98366e10-a24b-46f4-be0b-d11f96ff78d4', '98366e10-a24b-46f4-be0b-d11f96ff78d6',
                        'День рожденья', 'Тема для дней рождения', 1, 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),

                    ('98366e10-a24b-46f4-be0b-d11f96ff78d5', '98366e10-a24b-46f4-be0b-d11f96ff78d6',
                        'Календарный праздник', 'Тема для календарных праздников', 1, 0, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

                INSERT INTO AppSettings (Key, Value, ModifiedDate)
                VALUES ('{(int)SettingsKey.ImportantDaysScopeId}', '98366e10-a24b-46f4-be0b-d11f96ff78d6', CURRENT_TIMESTAMP);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
