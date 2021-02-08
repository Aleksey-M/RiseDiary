using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebUI.Shared.Dto;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public class SqliteDbController : ControllerBase
    {
        private readonly ISqliteDatabase _sqliteDb;
        public SqliteDbController(ISqliteDatabase sqliteDb)
        {
            _sqliteDb = sqliteDb;
        }

        [HttpGet, Route("api/v1.0/sqlitedb")]
        [ProducesResponseType(typeof(SqliteDbInfoDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SqliteDbInfoDto>> GetDbInfo()
        {
            var db = _sqliteDb.GetSqliteDatabaseInfo();
            var deleted = await _sqliteDb.GetDeletedEntitiesInfo();

            return Ok(new SqliteDbInfoDto
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
            });
        }

        [HttpPost, Route("api/v1.0/sqlitedb/clear")]
        public async Task<IActionResult> ClearDb()
        {
            await _sqliteDb.ClearDatabase();
            await _sqliteDb.Vacuum();

            return Ok();
        }

        [HttpPost, Route("api/v1.0/sqlitedb/file2file")]
        public async Task<IActionResult> File2FileMigration()
        {
            await _sqliteDb.File2FileMigration();

            return Ok();
        }
    }
}
