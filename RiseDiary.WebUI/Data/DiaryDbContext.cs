using Microsoft.EntityFrameworkCore;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiaryScope>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryTheme>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryImage>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryRecord>().Property(r => r.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Cogitation>().Property(c => c.Id).ValueGeneratedOnAdd();

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

        public void AddRecordTypeId(int rtid)
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
        public void AddRecordTypeId(IEnumerable<int> idsList)
        {
            if (idsList.Any(i => !RecordThemeIds.Contains(i)))
            {
                RecordThemeIds = new ReadOnlyCollection<int>(RecordThemeIds.Union(idsList).ToList());
            }
        }
        public void RemoveRecordTypeId(int id)
        {
            if (RecordThemeIds.Contains(id))
            {
                RecordThemeIds = new ReadOnlyCollection<int>(RecordThemeIds.Where(i => i != id).ToList());
            }
        }
        public void RemoveRecordTypeId(IEnumerable<int> idsList)
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
            return !(await context.Themes.AnyAsync(th => th.DiaryScopeId == scopeId));             
        }

        public static async Task DeleteScope(this DiaryDbContext context, int scopeId)
        {
            bool canDelete = await context.CanDeleteScope(scopeId);
            if (canDelete)
            {
                var s = context.Scopes.Find(scopeId);
                if(s != null) context.Scopes.Remove(s);
                await context.SaveChangesAsync();
            }
        }

        public static async Task<int> UpdateScope(this DiaryDbContext context, DiaryScope scope)
        {
            var targetScope = await context.Scopes.FindAsync(scope?.Id);
            if(targetScope == null) throw new ArgumentException($"Scope with id = {scope.Id} is not exists");

            targetScope.ScopeName = scope.ScopeName;
            await context.SaveChangesAsync();
            return targetScope.Id;
        }

        public static Task<DiaryScope> FetchScopeById(this DiaryDbContext context, int scopeId)
        {
            return context.Scopes.FindAsync(scopeId);
        }

        public static Task<List<DiaryScope>> FetchAllScopes(this DiaryDbContext context)
        {
            return context.Scopes.ToListAsync();
        }

        public static Task<int> GetScopesCount(this DiaryDbContext context)
        {
            return context.Scopes.CountAsync();
        }

        public static async Task<int> AddTheme(this DiaryDbContext context, int scopeId, string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                throw new ArgumentException($"Parameter {nameof(themeName)} should not be null or empty");
            }
            var theme = new DiaryTheme { DiaryScopeId = scopeId, ThemeName = themeName };
            context.Themes.Add(theme);
            await context.SaveChangesAsync();
            return theme.Id;
        }

        public static async Task<int> UpdateTheme(this DiaryDbContext context, DiaryTheme theme)
        {
            var targetTheme = await context.Themes.FindAsync(theme?.Id);
            if(targetTheme != null)
            {
                targetTheme.ThemeName = theme.ThemeName;
                await context.SaveChangesAsync();
                return targetTheme.Id;
            }
            return 0;
        }

        public static async Task DeleteTheme(this DiaryDbContext context, int themeId)
        {
            var theme = await context.Themes.FindAsync(themeId);
            if (theme != null)
            {
                context.RecordThemes.RemoveRange(await context.RecordThemes.Where(rth => rth.ThemeId == themeId).ToListAsync());
                context.Themes.Remove(theme);
                await context.SaveChangesAsync();
            }
        }

        public static Task<DiaryTheme> FetchThemeById(this DiaryDbContext context, int themeId)
        {
            return context.Themes.FindAsync(themeId);
        }

        public static Task<List<DiaryTheme>> FetchThemesOfScope(this DiaryDbContext context, int? scopeId)
        {
            if(scopeId != null)
            {
                return context.Themes.Where(t => t.DiaryScopeId == scopeId).ToListAsync();
            }
            return context.Themes.ToListAsync();
        }

        public static Task<List<int>> FetchThemesIds(this DiaryDbContext context, int? scopeId)
        {
            if (scopeId != null)
            {
                return context.Themes
                    .Where(t => t.DiaryScopeId == scopeId)
                    .Select(t => t.Id)
                    .ToListAsync();
            }
            return context.Themes.Select(t => t.Id).ToListAsync();
        }

        public static async Task<List<DiaryThemeJoined>> FetchThemesWithScopes(this DiaryDbContext context)
        {
            var scopes = await context.Scopes.ToListAsync();
            var res = await context.Themes
                .Select(t => new DiaryThemeJoined {
                    Id = t.Id,
                    ScopeId = t.DiaryScopeId,
                    ThemeName = t.ThemeName})
                .OrderBy(tj => tj.ScopeName)
                .ThenBy(tj => tj.ThemeName)
                .ToListAsync();
            res.ForEach(tj => tj.ScopeName = scopes.FirstOrDefault(s => s.Id == tj.ScopeId)?.ScopeName ?? string.Empty);
            return res;
        }

        public static Task<int> GetThemesCount(this DiaryDbContext context, int? scopeId)
        {
            if (scopeId != null)
            {
                return context.Themes
                    .Where(t => t.DiaryScopeId == scopeId)
                    .CountAsync();
            }
            return context.Themes.CountAsync();
        }

        public static Task AddRecordTheme(this DiaryDbContext context, int recordId, int themeId)
        {
            context.RecordThemes.Add(new DiaryRecordTheme { RecordId = recordId, ThemeId = themeId });
            return context.SaveChangesAsync();
        }

        public static async Task RemoveRecordTheme(this DiaryDbContext context, int recordId, int themeId)
        {
            var rt = await context.RecordThemes.FindAsync(themeId, recordId);
            if (rt != null) context.RecordThemes.Remove(rt);
            await context.SaveChangesAsync();
        }

        public static async Task<List<DiaryTheme>> FetchRecordThemes(this DiaryDbContext context, int recordId)
        {
            var themesIds = await context.RecordThemes.Where(t => t.RecordId == recordId).Select(t => t.ThemeId).ToListAsync();
            return await context.Themes.Where(t => themesIds.Contains(t.Id)).ToListAsync();
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
                context.Images.Remove(img);
                await context.SaveChangesAsync();
            }
        }
        public static async Task UpdateImageName(this DiaryDbContext context, int imageId, string imageNewName)
        {
            var img = await context.Images.FindAsync(imageId);
            if (img != null)
            {
                img.Name = imageNewName;
                await context.SaveChangesAsync();
            }
        }
        public static Task<DiaryImage> FetchImageById(this DiaryDbContext context, int imageId)
        {
            return context.Images.FindAsync(imageId);
        }
        public static Task<int> GetImagesCount(this DiaryDbContext context)
        {
            return context.Images.CountAsync();
        }
        public static Task<List<DiaryImage>> FetchImageSet(this DiaryDbContext context, int skip, int count)
        {
            return context.Images.OrderByDescending(i => i.CreateDate).Skip(skip).Take(count).ToListAsync();
        }
        public static async Task<byte[]> FetchImageDataById(this DiaryDbContext context,int imageId)
        {
            var img = await context.Images.FindAsync(imageId);
            if (img != null)
            {
                return img.Data;
            }
            return null;
        }
        public static Task AddImageForRecord(this DiaryDbContext context, int recordId, int imageId)
        {
            context.RecordImages.Add(new DiaryRecordImage { ImageId = imageId, RecordId = recordId });
            return context.SaveChangesAsync();
        }
        public static async Task RemoveImageForRecord(this DiaryDbContext context, int recordId, int imageId)
        {
            var recordImage = await context.RecordImages.FindAsync(imageId, recordId);
            if(recordImage != null)
            {
                context.RecordImages.Remove(recordImage);
                await context.SaveChangesAsync();
            }
        }
        public static async Task<List<DiaryImage>> FetchImagesForRecord(this DiaryDbContext context, int recordId)
        {
            var imagesIds = await context.RecordImages.Where(ri => ri.RecordId == recordId).Select(ri => ri.ImageId).ToListAsync();
            return await context.Images.Where(i => imagesIds.Contains(i.Id)).ToListAsync();
        }
        public static Task<int> GetLinkedRecordsCount(this DiaryDbContext context, int imageId)
        {
            return context.RecordImages.Where(ri => ri.ImageId == imageId).CountAsync();
        }

        public static async Task DeleteCogitation(this DiaryDbContext context, int cogitationId)
        {
            var c = await context.Cogitations.FindAsync(cogitationId);
            if(c != null)
            {
                context.Cogitations.Remove(c);
                await context.SaveChangesAsync();
            }
        }

        public static Task<Cogitation> FetchCogitationById(this DiaryDbContext context, int cogitationId)
        {
            return context.Cogitations.FindAsync(cogitationId);
        }

        public static Task<List<Cogitation>> FetchAllCogitationsOfRecord(this DiaryDbContext context, int recordId)
        {
            return context.Cogitations.Where(c => c.RecordId == recordId).ToListAsync();
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
            if (c != null)
            {
                c.Date = cogitation.Date;
                c.Text = cogitation.Text;
                await context.SaveChangesAsync();
                return c.Id;
            }
            return 0;
        }

        public static Task<int> GetCogitationsCount(this DiaryDbContext context, int recordId)
        {
            return context.Cogitations.Where(c => c.RecordId == recordId).CountAsync();
        }
        
        public static async Task<int> AddDiaryRecord(this DiaryDbContext context, DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            context.Records.Add(record);
            await context.SaveChangesAsync();
            return record.Id;
        }

        public static async Task DeleteRecord(this DiaryDbContext context, int recordId)
        {
            context.Cogitations.RemoveRange(await context.Cogitations.Where(c => c.RecordId == recordId).ToListAsync());
            context.RecordImages.RemoveRange(await context.RecordImages.Where(i => i.RecordId == recordId).ToListAsync());
            context.RecordThemes.RemoveRange(await context.RecordThemes.Where(t => t.RecordId == recordId).ToListAsync());
            context.Records.Remove(await context.Records.FindAsync(recordId));
            await context.SaveChangesAsync();
        }

        public static Task<DiaryRecord> FetchRecordById(this DiaryDbContext context, int recordId)
        {
            return context.Records.FindAsync(recordId);
        }

        public static async Task<DiaryRecord> GetRecordByCogitation(this DiaryDbContext context, int cogitationId)
        {
            var c = await context.Cogitations.FindAsync(cogitationId);
            return c == null ? null : await context.Records.FindAsync(c.RecordId);
        }
        
        private static IQueryable<DiaryRecord> _FetchRecordsListFiltered(DiaryDbContext context, RecordsFilter filter)
        {
            var result = context.Records.AsQueryable();

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
                    result = result.Where(r => context.RecordThemes.Any(rt => rt.RecordId == r.Id && filter.RecordThemeIds.Contains(rt.ThemeId)));
                }
            }
            return result.OrderByDescending(r => r.Date).Skip(filter.PageNo * filter.PageSize).Take(filter.PageSize);
        }

        public static Task<List<DiaryRecord>> FetchRecordsListFiltered(this DiaryDbContext context, RecordsFilter filter)
        {
            return _FetchRecordsListFiltered(context, filter).ToListAsync();
        }

        public static Task<int> GetFilteredRecordsCount(this DiaryDbContext context, RecordsFilter filter)
        {
            return _FetchRecordsListFiltered(context, filter).CountAsync();
        }

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
                return context.Records.Where(r => r.Date.Year == year).CountAsync();
            }
            return context.Records.Where(r => r.Date.Year == year && r.Date.Month == month).CountAsync();
        }

        public static async Task<int> UpdateRecord(this DiaryDbContext context, DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            var destRec = await context.Records.FindAsync(record.Id);
            if(destRec == null) throw new ArgumentException($"Record with ID={record.Id} not found");

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
            return context.Records.Select(r => r.Date.Year).Distinct().ToListAsync();
        }
    }
}
