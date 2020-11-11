using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    public class RecordsController : ControllerBase
    {
        private readonly IRecordsService _recordService;
        private readonly IHostAndPortService _hostAndPortService;
        private readonly IRecordsThemesService _recordsThemesService;

        public RecordsController(IRecordsService recordService, IHostAndPortService hostAndPortService, IRecordsThemesService recordsThemesService)
        {
            _recordService = recordService;
            _hostAndPortService = hostAndPortService;
            _recordsThemesService = recordsThemesService;
        }

        [HttpPost, Route("api/v1.0/records")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateRecord(CreateRecordDto createDto)
        {
            try
            {
                var recId = await _recordService.AddRecord(createDto.Date, createDto.RecordName, createDto.RecordText);
                if (createDto.ThemesIds.Length > 0) await _recordsThemesService.AddRecordTheme(recId, createDto.ThemesIds);
                var newRecordUri = $@"{_hostAndPortService.GetHostAndPort()}/api/v1.0/records/{recId}";
                return Created(newRecordUri, recId);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpGet, Route("api/v1.0/records/{recordId}")]
        [ProducesResponseType(typeof(RecordDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RecordDto>> GetRecord(Guid recordId)
        {
            try
            {
                var record = await _recordService.FetchRecordById(recordId);
                var dto = new RecordDto
                {
                    Id = record.Id,
                    Date = record.Date,
                    CreatedDate = record.CreateDate,
                    ModifiedDate = record.ModifyDate,
                    Name = record.Name,
                    Text = record.Text,
                    Cogitations = record.Cogitations
                        .Select(c => new CogitationDto
                        {
                            Id = c.Id,
                            CreateDate = c.Date,
                            Text = c.Text
                        })
                        .ToArray(),
                    Themes = record.ThemesRefs
                        .Select(rt => rt.Theme)
                        .Select(t => new ThemeDto
                        {
                            ThemeId = t!.Id,
                            ThemeName = t.ThemeName,
                            Actual = t.Actual
                        })
                        .ToArray(),
                    Images = record.ImagesRefs
                        .Select(ri => ri.Image)
                        .Select(i => new ImageListItemDto
                        {
                            Id = i!.Id,
                            Name = i.Name,
                            Width = i.Width,
                            Height = i.Height,
                            SizeKb = i.GetSizeKbString(),
                            Base64Thumbnail = i.GetBase64Thumbnail()
                        })
                        .ToArray()
                };

                return Ok(dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpDelete, Route("api/v1.0/records/{recordId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteRecord(Guid recordId)
        {
            await _recordService.DeleteRecord(recordId);
            return NoContent();
        }

        [HttpPut, Route("api/v1.0/records/{recordId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRecord(Guid recordId, UpdateRecordDto updateRecordDto)
        {
            if (recordId != updateRecordDto.Id) return BadRequest("Not consistent request");

            try
            {
                await _recordService.UpdateRecord(recordId, updateRecordDto.NewDate, updateRecordDto.NewName, updateRecordDto.NewText);
                return NoContent();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPost, Route("api/v1.0/records/{recordId}/cogitations")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddCogitation(Guid recordId, CreateCogitationDto createCogitationDto)
        {
            if (recordId != createCogitationDto.RecordId) return BadRequest("Not consistent request");

            try
            {
                var cogId = await _recordService.AddCogitation(recordId, createCogitationDto.Text);
                var recordUri = $@"{_hostAndPortService.GetHostAndPort()}/api/v1.0/records/{recordId}";
                return Created(recordUri, cogId);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPut, Route("api/v1.0/records/{recordId}/cogitations/{cogitationId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateCogitation(Guid recordId, Guid cogitationId, UpdateCogitationDto updateCogitationDto)
        {
            if (recordId != updateCogitationDto.RecordId || cogitationId != updateCogitationDto.CogitationId) return BadRequest("Not consistent request");

            try
            {
                await _recordService.UpdateCogitationText(cogitationId, updateCogitationDto.NewText);
                return NoContent();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpDelete, Route("api/v1.0/records/{recordId}/cogitations/{cogitationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> DeleteCogitation(Guid recordId, Guid cogitationId)
        {
            await _recordService.DeleteCogitation(cogitationId);
            return NoContent();
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
