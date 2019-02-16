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
        public DbSet<DiaryImageFull> FullSizeImages { get; set; }
        public DbSet<DiaryRecord> Records { get; set; }
        public DbSet<Cogitation> Cogitations { get; set; }

        public DbSet<DiaryRecordTheme> RecordThemes { get; set; }
        public DbSet<DiaryRecordImage> RecordImages { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<TempImage> TempImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiaryRecord>().Property(r => r.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryRecord>().HasMany(r => r.Cogitations)
                .WithOne(c => c.Record)
                .HasForeignKey(c => c.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryRecord>().HasMany(r => r.ThemesRefs)
                .WithOne(tr => tr.Record)
                .HasForeignKey(tr => tr.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryRecord>().HasMany(r => r.ImagesRefs)
                .WithOne(ir => ir.DiaryRecord)
                .HasForeignKey(ir => ir.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cogitation>().Property(c => c.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<DiaryScope>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryScope>()
                .HasMany(s => s.Themes)
                .WithOne(t => t.Scope)
                .HasForeignKey(t => t.ScopeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiaryTheme>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryTheme>()
                .HasMany(t => t.RecordsRefs)
                .WithOne(rt => rt.Theme)
                .HasForeignKey(rt => rt.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiaryRecordTheme>().HasKey(nameof(DiaryRecordTheme.RecordId), nameof(DiaryRecordTheme.ThemeId));
            modelBuilder.Entity<DiaryRecordTheme>().HasIndex(nameof(DiaryRecordTheme.RecordId), nameof(DiaryRecordTheme.ThemeId));

            modelBuilder.Entity<DiaryImage>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryImage>()
                .HasOne(i => i.FullImage)
                .WithOne(fi => fi.DiaryImage)
                .HasForeignKey<DiaryImageFull>(fi => fi.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryImage>()
               .HasOne(i => i.TempImage)
               .WithOne(ti => ti.DiaryImage)
               .HasForeignKey<TempImage>(ti => ti.SourceImageId)
               .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryImage>()
                .HasMany(i => i.RecordsRefs)
                .WithOne(rr => rr.DiaryImage)
                .HasForeignKey(rr => rr.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiaryImageFull>().Property(dif => dif.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<DiaryRecordImage>().HasKey(nameof(DiaryRecordImage.RecordId), nameof(DiaryRecordImage.ImageId));
            modelBuilder.Entity<DiaryRecordImage>().HasIndex(nameof(DiaryRecordImage.RecordId), nameof(DiaryRecordImage.ImageId));

            modelBuilder.Entity<AppSetting>().HasKey(s => s.Key);
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
            await context.Scopes.AddAsync(area);
            await context.SaveChangesAsync();
            return area.Id;
        }

        public static async Task<bool> CanDeleteScope(this DiaryDbContext context, int scopeId) => !(await context.Themes.AnyAsync(th => th.ScopeId == scopeId && !th.Deleted));

        public static async Task DeleteScope(this DiaryDbContext context, int scopeId)
        {
            bool canDelete = await context.CanDeleteScope(scopeId);
            if (canDelete)
            {
                var s = await context.Scopes.FindAsync(scopeId);
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

        public static Task<DiaryScope> FetchScopeById(this DiaryDbContext context, int scopeId) => context.Scopes.FirstOrDefaultAsync(s => s.Id == scopeId && !s.Deleted);
        public static Task<List<DiaryScope>> FetchAllScopes(this DiaryDbContext context) => context.Scopes.Where(s => !s.Deleted).ToListAsync();
        public static Task<int> GetScopesCount(this DiaryDbContext context) => context.Scopes.CountAsync(s => !s.Deleted);

        public static async Task<int> AddTheme(this DiaryDbContext context, int scopeId, string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                throw new ArgumentException($"Parameter {nameof(themeName)} should not be null or empty");
            }
            var theme = new DiaryTheme { ScopeId = scopeId, ThemeName = themeName };
            await context.Themes.AddAsync(theme);
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

        public static Task<DiaryTheme> FetchThemeById(this DiaryDbContext context, int themeId) => context.Themes.FirstOrDefaultAsync(t => t.Id == themeId && !t.Deleted);

        public static Task<List<DiaryTheme>> FetchThemesOfScope(this DiaryDbContext context, int? scopeId)
        {
            return scopeId != null
                ? context.Themes.Where(t => t.ScopeId == scopeId && !t.Deleted).ToListAsync()
                : context.Themes.Where(t=>!t.Deleted).ToListAsync();
        }

        public static Task<List<int>> FetchThemesIds(this DiaryDbContext context, int? scopeId)
        {
            return scopeId != null
                ? context.Themes
                    .Where(t => t.ScopeId == scopeId && !t.Deleted)
                    .Select(t => t.Id)
                    .ToListAsync()
                : context.Themes.Where(t=>!t.Deleted).Select(t => t.Id).ToListAsync();
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
                    ThemeName = t.ThemeName,
                    Actual = t.Actual
                })
                .OrderBy(tj => tj.ScopeName)
                .ThenBy(tj => tj.ThemeName)
                .ToListAsync();
        }

        public static Task<int> GetThemesCount(this DiaryDbContext context, int? scopeId)
        {
            return scopeId != null
                ? context.Themes.CountAsync(t => t.ScopeId == scopeId && !t.Deleted)
                : context.Themes.CountAsync(t => !t.Deleted);
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
                await context.RecordThemes.AddAsync(new DiaryRecordTheme { RecordId = recordId, ThemeId = themeId });
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

        public static async Task<int> AddImage(this DiaryDbContext context, string imageName, byte[] fullSizeImageData)
        {
            if (imageName == null)
            {
                throw new ArgumentNullException(nameof(imageName));
            }
            if (fullSizeImageData == null || fullSizeImageData.Length == 0)
            {
                throw new ArgumentException(nameof(fullSizeImageData));
            }
            var image = new DiaryImage
            {
                Name = imageName,
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                SizeByte = fullSizeImageData.Length,
                Thumbnail = ImageHelper.ScaleImage(fullSizeImageData)
            };
            (image.Width, image.Height) = ImageHelper.ImageSize(fullSizeImageData);
            await context.Images.AddAsync(image);
            await context.SaveChangesAsync();

            var fullImage = new DiaryImageFull
            {
                Data = fullSizeImageData,
                ImageId = image.Id
            };
            await context.FullSizeImages.AddAsync(fullImage);
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
                img.ModifyDate = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }
        public static async Task UpdateFullSizeImage(this DiaryDbContext context, int imageId, byte[] newImage)
        {
            var img = await context.Images.FindAsync(imageId);
            if (img != null && !img.Deleted)
            {
                img.Thumbnail = ImageHelper.ScaleImage(newImage);
                img.ModifyDate = DateTime.Now;
                img.SizeByte = newImage.Length;
                (img.Width, img.Height) = ImageHelper.ImageSize(newImage);
                var fullImg = await context.FullSizeImages.FindAsync(img.Id);
                fullImg.Data = newImage;
                await context.SaveChangesAsync();
            }
        }
        public static Task<DiaryImage> FetchImageById(this DiaryDbContext context, int imageId) => context.Images.FirstOrDefaultAsync(i => i.Id == imageId && !i.Deleted);
        public static async Task<byte[]> FetchFullImageById(this DiaryDbContext context, int imageId)
        {
            bool imageExists = await context.Images.FirstOrDefaultAsync(i => i.Id == imageId && !i.Deleted) != null;
            return imageExists ? (await context.FullSizeImages.FirstOrDefaultAsync(i => i.ImageId == imageId))?.Data : null;
        }
        public static Task<int> GetImagesCount(this DiaryDbContext context) => context.Images.CountAsync(i => !i.Deleted);
        public static Task<List<DiaryImage>> FetchImageSet(this DiaryDbContext context, int skip, int count) => context.Images.Where(i => !i.Deleted).OrderByDescending(i => i.CreateDate).Skip(skip).Take(count).ToListAsync();
        public static Task<List<DiaryImage>> FetchAllImages(this DiaryDbContext context) => context.Images.Where(i => !i.Deleted).OrderByDescending(i => i.CreateDate).ToListAsync();
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
                await context.RecordImages.AddAsync(new DiaryRecordImage { ImageId = imageId, RecordId = recordId });
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
        public static Task<int> GetLinkedRecordsCount(this DiaryDbContext context, int imageId) => context.RecordImages.CountAsync(ri => ri.ImageId == imageId && !ri.Deleted);

        public static async Task DeleteCogitation(this DiaryDbContext context, int cogitationId)
        {
            var c = await context.Cogitations.FindAsync(cogitationId);
            if(c != null)
            {
                c.Deleted = true;
                await context.SaveChangesAsync();
            }
        }

        public static Task<Cogitation> FetchCogitationById(this DiaryDbContext context, int cogitationId) => context.Cogitations.FirstOrDefaultAsync(c => c.Id == cogitationId && !c.Deleted);
        public static Task<List<Cogitation>> FetchAllCogitationsOfRecord(this DiaryDbContext context, int recordId) => context.Cogitations.Where(c => c.RecordId == recordId && !c.Deleted).ToListAsync();

        public static async Task<int> AddCogitation(this DiaryDbContext context, Cogitation cogitation)
        {
            if (cogitation == null) throw new ArgumentNullException(nameof(cogitation));
            await context.Cogitations.AddAsync(cogitation);
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

        public static Task<int> GetCogitationsCount(this DiaryDbContext context, int recordId) => context.Cogitations.CountAsync(c => c.RecordId == recordId && !c.Deleted);

        public static async Task<int> AddRecord(this DiaryDbContext context, DiaryRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            DiaryRecord r = record.Id != 0
                ? new DiaryRecord
                {
                    CreateDate = record.CreateDate,
                    Date = record.Date,
                    ModifyDate = record.ModifyDate,
                    Name = record.Name,
                    Text = record.Text
                }
                : record;
            await context.Records.AddAsync(r);
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

        public static Task<DiaryRecord> FetchRecordById(this DiaryDbContext context, int recordId) => context.Records.FirstOrDefaultAsync(r => r.Id == recordId && !r.Deleted);

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
                    result = result.Where(r => filter.RecordThemeIds.All(id => context.RecordThemes.Where(rt => rt.RecordId == r.Id && !rt.Deleted).Select(rt => rt.ThemeId).Contains(id)));                                      
                }
            }
            return result;
        }

        public static Task<List<DiaryRecord>> FetchRecordsListFiltered(this DiaryDbContext context, RecordsFilter filter) => _FetchRecordsListFiltered(context, filter).OrderByDescending(r => r.Date).Skip(filter.PageNo * filter.PageSize).Take(filter.PageSize).ToListAsync();

        public static Task<int> GetFilteredRecordsCount(this DiaryDbContext context, RecordsFilter filter) => _FetchRecordsListFiltered(context, filter).CountAsync();

        public static Task<List<DiaryRecord>> FetchRecordsByMonth(this DiaryDbContext context, int year, int? month = null)
        {
            return month == null
                ? context.Records.Where(r => r.Date.Year == year).ToListAsync()
                : context.Records.Where(r => r.Date.Year == year && r.Date.Month == month).ToListAsync();
        }

        public static Task<int> GetMonthRecordsCount(this DiaryDbContext context, int year, int? month = null)
        {
            return month == null
                ? context.Records.CountAsync(r => r.Date.Year == year && !r.Deleted)
                : context.Records.CountAsync(r => r.Date.Year == year && r.Date.Month == month && !r.Deleted);
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

        public static Task<List<int>> FetchYearsList(this DiaryDbContext context) => context.Records.Where(r => !r.Deleted).Select(r => r.Date.Year).Distinct().ToListAsync();

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
            if(int.TryParse(str, NumberStyles.None, CultureInfo.CurrentCulture.NumberFormat, out int result))
            {
                return result;
            }
            return null;
        }

        public static async Task UpdateAppSetting(this DiaryDbContext context, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));
            var appSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            if(appSetting == null)
            {
                await context.AppSettings.AddAsync(new AppSetting
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

        public static Task<List<DateItem>> FetchDateItems(this DiaryDbContext context, int scopeId, DatesRange datesRange)
        {
            return context.RecordThemes.Where(rt => !rt.Deleted)
                .Join(context.Themes.Where(t => !t.Deleted && t.ScopeId == scopeId), rt => rt.ThemeId, t => t.Id, (rt, t) => new { t.ThemeName, rt.RecordId })
                .Join(
                    context.Records.Where(r => !r.Deleted && datesRange.IsDateInRange(r.Date)), 
                    t => t.RecordId, 
                    r => r.Id, 
                    (t, r) => new DateItem(datesRange, r.Id, t.ThemeName, r.Date, r.Name, r.Text))
                .OrderByDescending(i => i.TransferredDate)
                .ToListAsync();
        }

        public static Task<List<DateItem>> FetchAllDateItems(this DiaryDbContext context, int scopeId)
        {
            var datesRange = DatesRange.ForAllYear();
            return context.RecordThemes.Where(rt => !rt.Deleted)
                .Join(context.Themes.Where(t => !t.Deleted && t.ScopeId == scopeId), rt => rt.ThemeId, t => t.Id, (rt, t) => new { t.ThemeName, rt.RecordId })
                .Join(context.Records.Where(r => !r.Deleted), t => t.RecordId, r => r.Id, (t, r) => new DateItem(datesRange, r.Id, t.ThemeName, r.Date, r.Name, r.Text))
                .OrderByDescending(i => i.TransferredDate)
                .ToListAsync();
        }
       
        public static Task<List<DiaryRecord>> SearchRecordsByText(this DiaryDbContext context, string searchText, int skip, int count = 20)
        {
            return context.Records
                .Where(r => !r.Deleted && (r.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1 ||
                    r.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1) ||
                    context.Cogitations.Any(c => !c.Deleted && 
                        c.RecordId == r.Id && 
                        c.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1))
                .OrderByDescending(r => r.Date)
                .Skip(skip)
                .Take(count)
                .ToListAsync();          
        }

        public static Task<int> SearchRecordsByTextCount(this DiaryDbContext context, string searchText)
        {           
            return context.Records
                .Where(r => !r.Deleted && (r.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1 ||
                    r.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1) ||
                    context.Cogitations.Any(c => !c.Deleted &&
                        c.RecordId == r.Id &&
                        c.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1))
                .CountAsync();                   
        }

        public static async Task ClearDbFromDeletedRecords(this DiaryDbContext context)
        {
            var recImages = context.RecordImages.Where(ri => ri.Deleted);
            context.RecordImages.RemoveRange(recImages);
            await context.SaveChangesAsync();

            var images = context.Images.Where(i => i.Deleted);
            context.Images.RemoveRange(images);
            await context.SaveChangesAsync();

            var recThemes = context.RecordThemes.Where(rt => rt.Deleted);
            context.RecordThemes.RemoveRange(recThemes);
            await context.SaveChangesAsync();

            var themes = context.Themes.Where(t => t.Deleted);
            context.Themes.RemoveRange(themes);
            await context.SaveChangesAsync();

            var scopes = context.Scopes.Where(s => s.Deleted);
            context.Scopes.RemoveRange(scopes);
            await context.SaveChangesAsync();

            var records = context.Records.Where(r => r.Deleted);
            var cogitations = records.SelectMany(r => context.Cogitations.Where(c => c.RecordId == r.Id));
            context.Cogitations.RemoveRange(cogitations);
            context.Records.RemoveRange(records);
            await context.SaveChangesAsync();
        }

        public static Task Vacuum(this DiaryDbContext context) => context.Database.ExecuteSqlCommandAsync("vacuum;");

        public static Task<Dictionary<int, string>> FetchRecordsForImage(this DiaryDbContext context, int imageId)
        {
            return context.RecordImages.Where(ri => !ri.Deleted && ri.ImageId == imageId)
                .Join(context.Records.Where(r => !r.Deleted), ri => ri.RecordId, r => r.Id, (ri, r) => new { r.Id, r.Name })
                .ToDictionaryAsync(r => r.Id, r => r.Name);
        }

        public static Task<TempImage> FetchTempImage(this DiaryDbContext context, int imageId) => context.TempImages.FirstOrDefaultAsync(t => t.SourceImageId == imageId);

        public static async Task AddUnsavedTempImage(this DiaryDbContext context, TempImage image)
        {
            var oldImages = context.TempImages.Where(t => t.SourceImageId == image.SourceImageId);
            if(oldImages.Count() > 0)
            {
                context.TempImages.RemoveRange(oldImages);
                await context.SaveChangesAsync();
            }
            await context.TempImages.AddAsync(image);
            await context.SaveChangesAsync();
        }

        public static async Task ApplyChangesFromTempImage(this DiaryDbContext context, TempImage tempImage)
        {
            var image = await context.Images.FindAsync(tempImage.SourceImageId) ?? throw new ArgumentException($"Image with Id ={tempImage.SourceImageId} is not found");
            var fullImage = await context.FullSizeImages.FindAsync(tempImage.SourceImageId) ?? throw new ArgumentException($"Full image with Id ={tempImage.SourceImageId} is not found! Database is inconsist");

            if (image.Deleted) throw new ArgumentException("Image is deleted. Can't update deleted image");

            image.Thumbnail = ImageHelper.ScaleImage(tempImage.Data);
            image.ModifyDate = DateTime.Now;
            image.SizeByte = tempImage.Data.Length;
            (image.Width, image.Height) = ImageHelper.ImageSize(tempImage.Data);

            fullImage.Data = tempImage.Data;

            context.TempImages.Remove(await context.TempImages.FindAsync(tempImage.Id) );

            await context.SaveChangesAsync();
        }

        public static async Task DeleteTempImage(this DiaryDbContext context, int imageId)
        {
            var oldImages = context.TempImages.Where(t => t.SourceImageId == imageId);
            if (oldImages.Count() > 0)
            {
                context.TempImages.RemoveRange(oldImages);
                await context.SaveChangesAsync();
            }
        }

        public static async Task ChangeThemeActuality(this DiaryDbContext context, int themeId, bool actual)
        {
            var theme = await context.Themes.FindAsync(themeId);
            if(theme.Actual != actual)
            {
                theme.Actual = actual;
                await context.SaveChangesAsync();
            }
        }

        public static async Task<List<CalendarRecordItem>> FetchCalendarDates(this DiaryDbContext context, int year, int[] themes)
        {
            var firstYearDay = new DateTime(year, 01, 01);
            var lastYearDay = new DateTime(year, 12, 31);

            var calendarItems = await context.Records
                .Where(r => !r.Deleted && r.Date >= firstYearDay && r.Date <= lastYearDay)
                .Select(r => new CalendarRecordItem { Id = r.Id, Date = r.Date, Name = r.Name })
                .ToListAsync();

            if(themes != null && themes.Length > 0)
            {
                var recordThemes = await context.RecordThemes
                    .Where(rt => !rt.Deleted)
                    .ToListAsync();

                return calendarItems
                    .Where(ci =>
                        themes.All(id => recordThemes
                            .Where(rt => rt.RecordId == ci.Id)
                            .Select(rt => rt.ThemeId).Contains(id)))
                    .ToList();                
            }
            return calendarItems;
        }

        public static async Task<List<int>> FetchYearsListFiltered(this DiaryDbContext context, int[] themes)
        {
            themes = themes ?? new int[0];
            if(themes.Length == 0)
                return await context.Records
                    .Where(r => !r.Deleted)
                    .Select(r => r.Date.Year)
                    .Distinct()
                    .OrderBy(y => y)
                    .ToListAsync();

            var recordDates = await context.Records
                .Where(r => !r.Deleted)
                .Select(r => new KeyValuePair<int, DateTime>(r.Id, r.Date))
                .ToListAsync();

            var recordThemes = await context.RecordThemes
                    .Where(rt => !rt.Deleted)
                    .ToListAsync();

            return recordDates
                .Where(r => themes.All(id => recordThemes
                    .Where(rt => rt.RecordId == r.Key)
                    .Select(rt => rt.ThemeId)
                    .Contains(id)))
                .Select(r => r.Value.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();              
        }
            
    }
}
