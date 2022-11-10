using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebAPI.Shared.Dto;

namespace RiseDiary.Api;

[ApiController]
[Route("api/sqlitedb")]
public sealed class SqliteDbController : ControllerBase
{
    private readonly ISqliteDatabase _sqliteDb;

    public SqliteDbController(ISqliteDatabase sqliteDb)
    {
        _sqliteDb = sqliteDb;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SqliteDbInfoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SqliteDbInfoDto>> GetDbInfo(CancellationToken cancellationToken)
    {
        var db = _sqliteDb.GetSqliteDatabaseInfo();
        var deleted = await _sqliteDb.GetDeletedEntitiesCount(cancellationToken);

        return new SqliteDbInfoDto
        {
            FileName = db.FileName,
            FileSize = db.FileSize,
            DeletedCogitations = deleted.Cogitations,
            DeletedImages = deleted.Images,
            DeletedRecords = deleted.Records,
            DeletedRecordImages = deleted.RecordImages,
            DeletedRecordThemes = deleted.RecordThemes,
            DeletedScopes = deleted.Scopes,
            DeletedThemes = deleted.Themes
        };
    }

    [HttpPost, Route("clear")]
    public async Task<IActionResult> ClearDb()
    {
        await _sqliteDb.ClearDatabase();
        await _sqliteDb.Vacuum();

        return Ok();
    }

    [HttpPost, Route("file2file")]
    public async Task<IActionResult> File2FileMigration()
    {
        await _sqliteDb.File2FileMigration();

        return Ok();
    }
}
