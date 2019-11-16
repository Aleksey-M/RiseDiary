﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.WebUI.Data;
using RiseDiary.Model;

namespace RiseDiary.WebUI.Pages
{
    public class RecordEditModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public RecordEditModel(DiaryDbContext context)
        {
            _context = context;
        }

        public Guid? RecordId { get; set; }
        public string RecordCode { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? RecordCreateDate { get; set; }
        public DateTime? RecordModifyDate { get; set; }
        public string RecordName { get; set; }
        public string RecordText { get; set; }
        public List<Guid> RecordThemesIds { get; set; }
        public List<DiaryThemeJoined> Themes { get; set; }

        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        public IEnumerable<DiaryThemeJoined> ActualOrSelectedThemes => Themes
            .Where(t => RecordThemesIds.Contains(t.Id) || t.Actual)
            .OrderBy(t => t.ScopeName)
            .ThenBy(t => t.ThemeName);

        public IEnumerable<DiaryThemeJoined> NotActualThemes => Themes
            .Where(t => !RecordThemesIds.Contains(t.Id) && !t.Actual)
            .OrderBy(t => t.ScopeName)
            .ThenBy(t => t.ThemeName);

        private async Task UpdatePageState()
        {
            Themes = await _context.FetchThemesWithScopes();
            RecordDate = DateTime.Now.Date;
            RecordThemesIds = new List<Guid>();
            if (RecordId != null)
            {
                var rec = await _context.FetchRecordById(RecordId.Value, LocalHostAndPort);
                if (rec != null)
                {
                    RecordDate = rec.Date;
                    RecordCreateDate = rec.CreateDate;
                    RecordModifyDate = rec.ModifyDate;
                    RecordName = rec.Name;
                    RecordText = rec.Text;
                    RecordThemesIds = (await _context.FetchRecordThemes(rec.Id)).Select(rt => rt.Id).ToList();
                }
            }
        }

        public async Task OnGetAsync(Guid? recordId)
        {
            RecordId = recordId;
            await UpdatePageState();
        }

        public async Task OnPostSaveRecordAsync(Guid? recordId, DateTime recordDate, string recordName, string recordText, Guid[] themeId)
        {
            if (recordId == null)
            {
                var newRecord = new DiaryRecord
                {
                    CreateDate = DateTime.Now,
                    ModifyDate = DateTime.Now,
                    Date = recordDate,
                    Name = recordName?.Trim() ?? string.Empty,
                    Text = recordText?.Trim() ?? string.Empty
                };
                Guid newRecordId = await _context.AddRecord(newRecord, LocalHostAndPort);
                foreach (Guid tid in themeId)
                {
                    await _context.AddRecordTheme(newRecordId, tid);
                }
                RecordId = newRecordId;
            }
            else
            {
                var record = await _context.FetchRecordById(recordId.Value, LocalHostAndPort);
                if (record != null)
                {
                    record.Date = recordDate;
                    record.ModifyDate = DateTime.Now;
                    record.Name = recordName?.Trim() ?? string.Empty;
                    record.Text = recordText?.Trim() ?? string.Empty;
                    await _context.UpdateRecord(record, LocalHostAndPort);

                    var recThemesIds = (await _context.FetchRecordThemes(record.Id)).Select(rth => rth.Id);
                    foreach (Guid id in recThemesIds.Except(themeId))
                    {
                        await _context.RemoveRecordTheme(record.Id, id);
                    }
                    foreach (Guid id in themeId.Except(recThemesIds))
                    {
                        await _context.AddRecordTheme(record.Id, id);
                    }
                    RecordId = record.Id;
                }
            }
            await UpdatePageState();
        }

        public async Task<IActionResult> OnPostDeleteRecordAsync(Guid recordId)
        {
            await _context.DeleteRecord(recordId);
            return Redirect("~/Records");
        }        
    }
}