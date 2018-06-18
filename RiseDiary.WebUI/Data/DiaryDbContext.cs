﻿using Microsoft.EntityFrameworkCore;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Data
{
    public class DiaryDbContext : DbContext
    {
        public DiaryDbContext(DbContextOptions<DiaryDbContext> options):base(options) { }

        public DbSet<DiaryScope> Scopes { get; set; }
        public DbSet<DiaryTheme> Themes { get; set; }
        public DbSet<DiaryImage> Images { get; set; }

        public DbSet<DiaryRecord> Records { get; set; }
        public DbSet<Cogitation> Cogitations { get; set; }

        public DbSet<DiaryRecordTheme> RecordThemes { get; set; }
        public DbSet<DiaryRecordImage> RecordImages { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiaryScope>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryTheme>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryImage>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryRecord>().Property(r => r.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Cogitation>().Property(c => c.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<AppSetting>().HasKey(s => s.Key);

            modelBuilder.Entity<DiaryRecordTheme>().HasKey(nameof(DiaryRecordTheme.RecordId), nameof(DiaryRecordTheme.ThemeId));
            modelBuilder.Entity<DiaryRecordImage>().HasKey(nameof(DiaryRecordImage.RecordId), nameof(DiaryRecordImage.ImageId));
        }
    }

    public sealed class RecordsFilter
    {
        public string RecordNameFilter { get; set; }

        private DateTime? _recordDateFrom;
        private DateTime? _recordDateTo;

        public DateTime? RecordDateFrom
        {
            get => _recordDateFrom;
            set => _recordDateFrom = value > _recordDateTo ? null : value?.Date;
        }
        public DateTime? RecordDateTo
        {
            get => _recordDateTo;
            set => _recordDateTo = value < _recordDateFrom ? null : value?.Date;
        }
        public int PageNo { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public static RecordsFilter Empty => new RecordsFilter();
        public static bool IsEmpty(RecordsFilter filter) =>
            string.IsNullOrWhiteSpace(filter.RecordNameFilter) &&
            filter.RecordDateFrom == null &&
            filter.RecordDateTo == null &&
            filter.RecordThemeIds.Count == 0;
        public ReadOnlyCollection<int> RecordThemeIds { get; private set; } = new ReadOnlyCollection<int>(new List<int>());

        public void AddThemeId(int rtid)
        {
            if (!RecordThemeIds.Contains(rtid))
            {
                var list = new List<int>(RecordThemeIds)
                {
                    rtid
                };
                RecordThemeIds = new ReadOnlyCollection<int>(list);
            }
        }
        public void AddThemeId(IEnumerable<int> idsList)
        {
            if (idsList.Any(i => !RecordThemeIds.Contains(i)))
            {
                RecordThemeIds = new ReadOnlyCollection<int>(RecordThemeIds.Union(idsList).ToList());
            }
        }
        public void RemoveThemeId(int id)
        {
            if (RecordThemeIds.Contains(id))
            {
                RecordThemeIds = new ReadOnlyCollection<int>(RecordThemeIds.Where(i => i != id).ToList());
            }
        }
        public void RemoveThemeId(IEnumerable<int> idsList)
        {
            var foundIds = RecordThemeIds.Intersect(idsList);
            if (foundIds.Count() > 0)
            {
                RecordThemeIds = new ReadOnlyCollection<int>(RecordThemeIds.Except(idsList).ToList());
            }
        }
        public bool IsEmptyTypeFilter => RecordThemeIds.Count == 0;
    }

    public static class DiaryDbContextExtensions
    {
        public static async Task<int> AddScope(this DiaryDbContext context, string scopeName)
        {
            if(string.IsNullOrWhiteSpace(scopeName)) throw new ArgumentException($"Parameter {nameof(scopeName)} should not be null or empty");
            var area = new DiaryScope { ScopeName = scopeName };
            context.Scopes.Add(area);
            await context.SaveChangesAsync();
            return area.Id;
        }

        public static async Task<bool> CanDeleteScope(this DiaryDbContext context, int scopeId)
        {
            return !(await context.Themes.AnyAsync(th => th.ScopeId == scopeId && !th.Deleted));             
        }

        public static async Task DeleteScope(this DiaryDbContext context, int scopeId)
        {
            bool canDelete = await context.CanDeleteScope(scopeId);
            if (canDelete)
            {
                var s = context.Scopes.Find(scopeId);
                if (s != null)
                {
                    s.Deleted = true;
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task<int> UpdateScope(this DiaryDbContext context, DiaryScope scope)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            var targetScope = await context.Scopes.FindAsync(scope.Id);
            if (targetScope == null) throw new ArgumentException($"Scope with id = {scope.Id} is not exists");
            if (targetScope.Deleted) throw new ArgumentException($"Scope with id = {scope.Id} is deleted");
            targetScope.ScopeName = scope.ScopeName;
            await context.SaveChangesAsync();
            return targetScope.Id;
        }

        public static Task<DiaryScope> FetchScopeById(this DiaryDbContext context, int scopeId)
        {
            return context.Scopes.FirstOrDefaultAsync(s=> s.Id == scopeId && !s.Deleted);
        }

        public static Task<List<DiaryScope>> FetchAllScopes(this DiaryDbContext context)
        {
            return context.Scopes.Where(s=>!s.Deleted).ToListAsync();
        }

        public static Task<int> GetScopesCount(this DiaryDbContext context)
        {
            return context.Scopes.CountAsync(s => !s.Deleted);
        }

        public static async Task<int> AddTheme(this DiaryDbContext context, int scopeId, string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                throw new ArgumentException($"Parameter {nameof(themeName)} should not be null or empty");
            }
            var theme = new DiaryTheme { ScopeId = scopeId, ThemeName = themeName };
            context.Themes.Add(theme);
            await context.SaveChangesAsync();
            return theme.Id;
        }

        public static async Task<int> UpdateTheme(this DiaryDbContext context, DiaryTheme theme)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            var targetTheme = await context.Themes.FindAsync(theme?.Id);
            if (targetTheme == null) throw new ArgumentException($"Theme with id = {theme.Id} is not exists");
            if (targetTheme.Deleted) throw new ArgumentException($"Theme with id = {theme.Id} is deleted");
            targetTheme.ThemeName = theme.ThemeName;
            await context.SaveChangesAsync();
            return targetTheme.Id;
        }

        public static async Task DeleteTheme(this DiaryDbContext context, int themeId)
        {
            var theme = await context.Themes.FindAsync(themeId);
            if (theme != null)
            {
                await context.RecordThemes.Where(rth => rth.ThemeId == themeId && !rth.Deleted).ForEachAsync(rth => rth.Deleted = true);
                theme.Deleted = true;
                await context.SaveChangesAsync();
            }
        }

        public static Task<DiaryTheme> FetchThemeById(this DiaryDbContext context, int themeId)
        {
            return context.Themes.FirstOrDefaultAsync(t => t.Id == themeId && !t.Deleted);
        }

        public static Task<List<DiaryTheme>> FetchThemesOfScope(this DiaryDbContext context, int? scopeId)
        {
            if(scopeId != null)
            {
                return context.Themes.Where(t => t.ScopeId == scopeId && !t.Deleted).ToListAsync();
            }
            return context.Themes.Where(t=>!t.Deleted).ToListAsync();
        }

        public static Task<List<int>> FetchThemesIds(this DiaryDbContext context, int? scopeId)
        {
            if (scopeId != null)
            {
                return context.Themes
                    .Where(t => t.ScopeId == scopeId && !t.Deleted)
                    .Select(t => t.Id)
                    .ToListAsync();
            }
            return context.Themes.Where(t=>!t.Deleted).Select(t => t.Id).ToListAsync();
        }

        public static Task<List<DiaryThemeJoined>> FetchThemesWithScopes(this DiaryDbContext context)
        {
            return context.Scopes.Where(s=>!s.Deleted).Join(
                context.Themes.Where(t=>!t.Deleted),
                s => s.Id,
                t => t.ScopeId,
                (s, t) => new DiaryThemeJoined
                {
                    Id = t.Id,
                    ScopeId = s.Id,
                    ScopeName = s.ScopeName,
                    ThemeName = t.ThemeName
                })
                .OrderBy(tj => tj.ScopeName)
                .ThenBy(tj => tj.ThemeName)
                .ToListAsync();
        }

        public static Task<int> GetThemesCount(this DiaryDbContext context, int? scopeId)
        {
            if (scopeId != null)
            {
                return context.Themes.CountAsync(t => t.ScopeId == scopeId && !t.Deleted);
            }
            return context.Themes.CountAsync(t => !t.Deleted);
        }

        public static async Task AddRecordTheme(this DiaryDbContext context, int recordId, int themeId)
        {
            var recThem = await context.RecordThemes.FirstOrDefaultAsync(rt=>rt.RecordId == recordId && rt.ThemeId == themeId);
            if (recThem != null)
            {
                if (recThem.Deleted)
                {
                    recThem.Deleted = false;
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                context.RecordThemes.Add(new DiaryRecordTheme { RecordId = recordId, ThemeId = themeId });
                await context.SaveChangesAsync();
            }
        }

        public static async Task RemoveRecordTheme(this DiaryDbContext context, int recordId, int themeId)
        {
            var rt = await context.RecordThemes.FindAsync(recordId, themeId);
            if (rt != null)
            {                
                rt.Deleted = true;
                await context.SaveChangesAsync();
            }           
        }

        public static Task<List<DiaryTheme>> FetchRecordThemes(this DiaryDbContext context, int recordId)
        {
            return context.RecordThemes
                .Where(rt => rt.RecordId == recordId && !rt.Deleted)
                .Join(context.Themes.Where(t => !t.Deleted), rt => rt.ThemeId, t => t.Id, (rt, t) => t)
                .ToListAsync();  
        }

        public static Task<List<string>> FetchRecordThemesList(this DiaryDbContext context, int recordId)
        {
            return context.RecordThemes
                .Where(rt => rt.RecordId == recordId && !rt.Deleted)
                .Join(context.Themes.OrderBy(t=>t.ThemeName), rt => rt.ThemeId, t => t.Id, (rt, t) => new { t.Id, t.ThemeName, t.ScopeId })
                .Join(context.Scopes, t => t.ScopeId, s => s.Id, (t, s) => $"{s.ScopeName} - {t.ThemeName}")
                .ToListAsync();
        }

        public static async Task<int> AddImage(this DiaryDbContext context, DiaryImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (image.Data == null || image.Data.Length == 0)
            {
                throw new ArgumentException(nameof(image));
            }
            context.Images.Add(image);
            await context.SaveChangesAsync();
            return image.Id;
        }

        public static async Task DeleteImage(this DiaryDbContext context, int imageId)
        {
            var img = await context.Images.FindAsync(imageId);
            if(img != null)
            {
                await context.RecordImages.Where(ri => ri.ImageId == img.Id && !ri.Deleted).ForEachAsync(ri=>ri.Deleted = true);
                img.Deleted = true;
                await context.SaveChangesAsync();
            }
        }
        public static async Task UpdateImageName(this DiaryDbContext context, int imageId, string imageNewName)
        {
            var img = await context.Images.FindAsync(imageId);
            if (img != null && !img.Deleted)
            {
                img.Name = imageNewName;
                await context.SaveChangesAsync();
            }
        }
        public static Task<DiaryImage> FetchImageById(this DiaryDbContext context, int imageId)
        {
            return context.Images.FirstOrDefaultAsync(i => i.Id == imageId && !i.Deleted);
        }
        public static Task<int> GetImagesCount(this DiaryDbContext context)
        {
            return context.Images.CountAsync(i => !i.Deleted);
        }
        public static Task<List<DiaryImage>> FetchImageSet(this DiaryDbContext context, int skip, int count)
        {
            return context.Images.Where(i=>!i.Deleted).OrderByDescending(i => i.CreateDate).Skip(skip).Take(count).ToListAsync();
        }
        public static async Task<byte[]> FetchImageDataById(this DiaryDbContext context,int imageId)
        {
            var img = await context.Images.FindAsync(imageId);
            if (img != null && !img.Deleted)
            {
                return img.Data;
            }
            return null;
        }
        public static async Task AddRecordImage(this DiaryDbContext context, int recordId, int imageId)
        {
            var recImg = await context.RecordImages.FirstOrDefaultAsync(ri => ri.RecordId == recordId && ri.ImageId == imageId);
            if(recImg != null)
            {
                if (recImg.Deleted)
                {
                    recImg.Deleted = false;
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                context.RecordImages.Add(new DiaryRecordImage { ImageId = imageId, RecordId = recordId });
                await context.SaveChangesAsync();
            }            
        }
        public static async Task RemoveRecordImage(this DiaryDbContext context, int recordId, int imageId)
        {
            var recordImage = await context.RecordImages.FindAsync(recordId, imageId);
            if(recordImage != null)
            {
                recordImage.Deleted = true;
                await context.SaveChangesAsync();
            }
        }
        public static async Task<List<DiaryImage>> FetchImagesForRecord(this DiaryDbContext context, int recordId)
        {
            var imagesIds = await context.RecordImages.Where(ri => ri.RecordId == recordId && !ri.Deleted).Select(ri => ri.ImageId).ToListAsync();
            return await context.Images.Where(i => imagesIds.Contains(i.Id) && !i.Deleted).ToListAsync();
        }
        public static Task<int> GetLinkedRecordsCount(this DiaryDbContext context, int imageId)
        {
            return context.RecordImages.CountAsync(ri => ri.ImageId == imageId && !ri.Deleted);
        }

        public static async Task DeleteCogitation(this DiaryDbContext context, int cogitationId)
        {
            var c = await context.Cogitations.FindAsync(cogitationId);
            if(c != null)
            {
                c.Deleted = true;
                await context.SaveChangesAsync();
            }
        }

        public static Task<Cogitation> FetchCogitationById(this DiaryDbContext context, int cogitationId)
        {
            return context.Cogitations.FirstOrDefaultAsync(c=>c.Id == cogitationId && !c.Deleted);
        }

        public static Task<List<Cogitation>> FetchAllCogitationsOfRecord(this DiaryDbContext context, int recordId)
        {
            return context.Cogitations.Where(c => c.RecordId == recordId && !c.Deleted).ToListAsync();
        }

        public static async Task<int> AddCogitation(this DiaryDbContext context, Cogitation cogitation)
        {
            if (cogitation == null) throw new ArgumentNullException(nameof(cogitation));
            context.Cogitations.Add(cogitation);
            await context.SaveChangesAsync();
            return cogitation.Id;
        }

        public static async Task<int> UpdateCogitation(this DiaryDbContext context, Cogitation cogitation)
        {
            var c = await context.Cogitations.FindAsync(cogitation.Id);
            if (c == null) throw new ArgumentException($"Cogitation with id = {cogitation.Id} is not exists");
            if (c.Deleted) throw new ArgumentException($"Cogitation with id = {cogitation.Id} is deleted");
            c.Date = cogitation.Date;
            c.Text = cogitation.Text;
            await context.SaveChangesAsync();
            return c.Id;
        }

        public static Task<int> GetCogitationsCount(this DiaryDbContext context, int recordId)
        {
            return context.Cogitations.CountAsync(c => c.RecordId == recordId && !c.Deleted);
        }
        
        public static async Task<int> AddRecord(this DiaryDbContext context, DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            DiaryRecord r;
            if (record.Id != 0)
            {
                r = new DiaryRecord
                {
                    CreateDate = record.CreateDate,
                    Date = record.Date,
                    ModifyDate = record.ModifyDate,
                    Name = record.Name,
                    Text = record.Text
                };
            }
            else
            {
                r = record;
            }
            context.Records.Add(r);
            await context.SaveChangesAsync();
            return r.Id;
        }

        public static async Task DeleteRecord(this DiaryDbContext context, int recordId)
        {            
            var record = await context.Records.FindAsync(recordId);
            if (record != null)
            {
                await context.Cogitations.Where(c=>c.RecordId == record.Id && !c.Deleted).ForEachAsync(c=>c.Deleted = true);
                await context.RecordImages.Where(ri=>ri.RecordId == record.Id && !ri.Deleted).ForEachAsync(ri => ri.Deleted = true);
                await context.RecordThemes.Where(rt => rt.RecordId == record.Id && !rt.Deleted).ForEachAsync(rt => rt.Deleted = true);
                record.Deleted = true;
                await context.SaveChangesAsync();
            }
        }

        public static Task<DiaryRecord> FetchRecordById(this DiaryDbContext context, int recordId)
        {
            return context.Records.FirstOrDefaultAsync(r => r.Id == recordId && !r.Deleted);
        }

        public static async Task<DiaryRecord> GetRecordByCogitation(this DiaryDbContext context, int cogitationId)
        {
            var c = await context.Cogitations.FindAsync(cogitationId);
            return c == null ? null : await context.Records.FindAsync(c.RecordId);
        }
        
        private static IQueryable<DiaryRecord> _FetchRecordsListFiltered(DiaryDbContext context, RecordsFilter filter)
        {
            var result = context.Records.Where(r => !r.Deleted);

            if (!RecordsFilter.IsEmpty(filter))
            {
                if (!string.IsNullOrWhiteSpace(filter.RecordNameFilter))
                {
                    result = result.Where(r => r.Name.IndexOf(filter.RecordNameFilter, StringComparison.CurrentCultureIgnoreCase) >= 0);
                }
                if (filter.RecordDateFrom != null)
                {
                    result = result.Where(r => r.Date >= filter.RecordDateFrom);                    
                }
                if (filter.RecordDateTo != null)
                {
                    result = result.Where(r => r.Date <= filter.RecordDateTo);                    
                }
                if (!filter.IsEmptyTypeFilter)
                {
                    result = result.Where(r => filter.RecordThemeIds.All(id => context.RecordThemes.Where(rt => rt.RecordId == r.Id && !rt.Deleted).Select(rt=>rt.ThemeId).Contains(id)));                   
                }
            }
            return result;
        }

        public static Task<List<DiaryRecord>> FetchRecordsListFiltered(this DiaryDbContext context, RecordsFilter filter) => _FetchRecordsListFiltered(context, filter).OrderByDescending(r => r.Date).Skip(filter.PageNo * filter.PageSize).Take(filter.PageSize).ToListAsync();

        public static Task<int> GetFilteredRecordsCount(this DiaryDbContext context, RecordsFilter filter) => _FetchRecordsListFiltered(context, filter).CountAsync();

        public static Task<List<DiaryRecord>> FetchRecordsByMonth(this DiaryDbContext context, int year, int? month = null)
        {
            if(month == null)
            {
                return context.Records.Where(r => r.Date.Year == year).ToListAsync();
            }
            return context.Records.Where(r => r.Date.Year == year && r.Date.Month == month).ToListAsync();
        }

        public static Task<int> GetMonthRecordsCount(this DiaryDbContext context, int year, int? month = null)
        {
            if (month == null)
            {
                return context.Records.CountAsync(r => r.Date.Year == year && !r.Deleted);
            }
            return context.Records.CountAsync(r => r.Date.Year == year && r.Date.Month == month && !r.Deleted);
        }

        public static async Task<int> UpdateRecord(this DiaryDbContext context, DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            var destRec = await context.Records.FindAsync(record.Id);
            if(destRec == null) throw new ArgumentException($"Record with ID={record.Id} is not found");
            if (destRec.Deleted) throw new ArgumentException($"Record with ID={record.Id} is deleted");
            destRec.Date = record.Date;
            destRec.CreateDate = record.CreateDate;
            destRec.ModifyDate = record.ModifyDate;
            destRec.Name = record.Name;
            destRec.Text = record.Text;

            await context.SaveChangesAsync();
            return destRec.Id;
        }

        public static Task<List<int>> FetchYearsList(this DiaryDbContext context)
        {
            return context.Records.Where(r => !r.Deleted).Select(r => r.Date.Year).Distinct().ToListAsync();
        }

        public static async Task<int> UpdateCogitationText(this DiaryDbContext context, int cogitationId, string text)
        {
            var c = await context.Cogitations.FindAsync(cogitationId);
            if (c == null) throw new ArgumentException($"Cogitation with id = {cogitationId} is not exists");
            if (c.Deleted) throw new ArgumentException($"Cogitation with id = {cogitationId} is deleted");
            c.Text = text;
            await context.SaveChangesAsync();
            return c.Id;
        }

        public static Task<Dictionary<DiaryScope, IEnumerable<DiaryTheme>>> FetchScopesWithThemes(this DiaryDbContext context)
        {
            return context.Scopes
                .Where(s => !s.Deleted)
                .OrderBy(s => s.ScopeName)
                .GroupJoin(context.Themes.Where(t => !t.Deleted), s => s.Id, t => t.ScopeId, (s, res) => new { s, res })
                .ToDictionaryAsync(j => j.s, j => j.res);
        }

        public static async Task<string> GetAppSetting(this DiaryDbContext context, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            return (await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key))?.Value ?? string.Empty;
        }

        public static async Task<int?> GetAppSettingInt(this DiaryDbContext context, string key)
        {
            var str = await context.GetAppSetting(key);
             var style = new NumberStyles();
            var formatProv = CultureInfo.CurrentCulture.NumberFormat;
            bool res = int.TryParse(str, style, formatProv, out int result);

            return res ? new Nullable<int>(result) : null;
        }

        public static async Task UpdateAppSetting(this DiaryDbContext context, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
            var appSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            if(appSetting == null)
            {
                context.AppSettings.Add(new AppSetting
                {
                    Key = key,
                    Value = value,
                    ModifiedDate = DateTime.Now
                });
                await context.SaveChangesAsync();
            }
            else
            {
               if(appSetting.Value != value)
                {
                    appSetting.Value = value;
                    appSetting.ModifiedDate = DateTime.Now;
                    await context.SaveChangesAsync();
                }
            }
        }

        public static Task<List<DateItem>> FetchDateItems(this DiaryDbContext context, int scopeId, DateTime today, int displayRange)
        {
            var from = today.AddDays(-displayRange);
            var to = today.AddDays(displayRange);
            return context.RecordThemes.Where(rt => !rt.Deleted)
                .Join(context.Themes.Where(t => !t.Deleted && t.ScopeId == scopeId), rt => rt.ThemeId, t => t.Id, (rt, t) => new { t.ThemeName, rt.RecordId })
                .Join(
                    context.Records.Where(
                        r => !r.Deleted && 
                        r.Date >= new DateTime(r.Date.Year, from.Month, from.Day) && 
                        r.Date <= new DateTime(r.Date.Year, to.Month, to.Day)), 
                    t => t.RecordId, 
                    r => r.Id, 
                    (t, r) => new DateItem(r.Id, t.ThemeName, r.Date, r.Name, r.Text))
                .OrderByDescending(i => i.ThisYearDate)
                .ToListAsync();
        }

        public static Task<List<DateItem>> FetchAllDateItems(this DiaryDbContext context, int scopeId)
        {
            return context.RecordThemes.Where(rt => !rt.Deleted)
                .Join(context.Themes.Where(t => !t.Deleted && t.ScopeId == scopeId), rt => rt.ThemeId, t => t.Id, (rt, t) => new { t.ThemeName, rt.RecordId })
                .Join(context.Records.Where(r => !r.Deleted), t => t.RecordId, r => r.Id, (t, r) => new DateItem(r.Id, t.ThemeName, r.Date, r.Name, r.Text))
                .OrderByDescending(i => i.ThisYearDate)
                .ToListAsync();
        }

        public static Task<List<DiaryRecord>> SearchRecordsByText(this DiaryDbContext context, string searchText)
        {
            return context.Records                
                .Where(r => !r.Deleted && ( r.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1 ||
                    r.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1))
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();
        }
    }
}
