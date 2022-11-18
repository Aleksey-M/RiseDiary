using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Database;

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

    [HttpGet("info")]
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

    [HttpPost("cleanup")]
    public async Task<IActionResult> ClearDb()
    {
        await _sqliteDb.ClearDatabase();
        await _sqliteDb.Vacuum();

        return Ok();
    }

    [HttpGet("deleted-data")]
    public async Task<ActionResult<DeletedDataInfo>> GetDeletedData(CancellationToken cancellationToken) =>
        await _sqliteDb.GetDeletedEntitiesData(cancellationToken);
}
