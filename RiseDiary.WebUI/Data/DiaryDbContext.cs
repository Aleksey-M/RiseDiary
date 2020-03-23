using Microsoft.EntityFrameworkCore;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Data
{
#pragma warning disable CA1303 // Do not pass literals as localized parameters
    public class DiaryDbContext : DbContext
    {
        public DiaryDbContext(DbContextOptions<DiaryDbContext> options) : base(options) { }

        public DbSet<DiaryScope> Scopes { get; set; } = null!;
        public DbSet<DiaryTheme> Themes { get; set; } = null!;
        public DbSet<DiaryImage> Images { get; set; } = null!;
        public DbSet<DiaryImageFull> FullSizeImages { get; set; } = null!;
        public DbSet<DiaryRecord> Records { get; set; } = null!;
        public DbSet<Cogitation> Cogitations { get; set; } = null!;

        public DbSet<DiaryRecordTheme> RecordThemes { get; set; } = null!;
        public DbSet<DiaryRecordImage> RecordImages { get; set; } = null!;

        public DbSet<AppSetting> AppSettings { get; set; } = null!;
        public DbSet<TempImage> TempImages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<DiaryRecord>().Property(r => r.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryRecord>().HasMany(r => r.Cogitations)
                .WithOne(c => c.Record!)
                .HasForeignKey(c => c.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryRecord>().HasMany(r => r.ThemesRefs)
                .WithOne(tr => tr.Record!)
                .HasForeignKey(tr => tr.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryRecord>().HasMany(r => r.ImagesRefs)
                .WithOne(ir => ir.Record!)
                .HasForeignKey(ir => ir.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cogitation>().Property(c => c.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<DiaryScope>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryScope>()
                .HasMany(s => s.Themes)
                .WithOne(t => t.Scope!)
                .HasForeignKey(t => t.ScopeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiaryTheme>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryTheme>()
                .HasMany(t => t.RecordsRefs)
                .WithOne(rt => rt.Theme!)
                .HasForeignKey(rt => rt.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiaryRecordTheme>().HasKey(nameof(DiaryRecordTheme.RecordId), nameof(DiaryRecordTheme.ThemeId));
            modelBuilder.Entity<DiaryRecordTheme>().HasIndex(nameof(DiaryRecordTheme.RecordId), nameof(DiaryRecordTheme.ThemeId));

            modelBuilder.Entity<DiaryImage>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DiaryImage>()
                .HasOne(i => i.FullImage)
                .WithOne(fi => fi!.DiaryImage!)
                .HasForeignKey<DiaryImageFull>(fi => fi.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryImage>()
               .HasOne(i => i.TempImage)
               .WithOne(ti => ti!.DiaryImage!)
               .HasForeignKey<TempImage>(ti => ti.SourceImageId)
               .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryImage>()
                .HasMany(i => i.RecordsRefs)
                .WithOne(rr => rr.Image!)
                .HasForeignKey(rr => rr.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiaryImageFull>().Property(dif => dif.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<DiaryRecordImage>().HasKey(nameof(DiaryRecordImage.RecordId), nameof(DiaryRecordImage.ImageId));
            modelBuilder.Entity<DiaryRecordImage>().HasIndex(nameof(DiaryRecordImage.RecordId), nameof(DiaryRecordImage.ImageId));

            modelBuilder.Entity<AppSetting>().HasKey(s => s.Key);
            // soft deleting
            modelBuilder.Entity<DiaryRecord>().HasQueryFilter(r => !r.Deleted);
            modelBuilder.Entity<Cogitation>().HasQueryFilter(c => !c.Deleted);
            modelBuilder.Entity<DiaryImage>().HasQueryFilter(i => !i.Deleted);
            modelBuilder.Entity<DiaryTheme>().HasQueryFilter(t => !t.Deleted);
            modelBuilder.Entity<DiaryScope>().HasQueryFilter(s => !s.Deleted);
            modelBuilder.Entity<DiaryRecordTheme>().HasQueryFilter(rt => !rt.Deleted);
            modelBuilder.Entity<DiaryRecordImage>().HasQueryFilter(ri => !ri.Deleted);
        }

        public bool SoftDeleting { get; set; } = true;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (SoftDeleting) OnBeforeSaving();
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public override int SaveChanges()
        {
            if (SoftDeleting) OnBeforeSaving();
            return base.SaveChanges();
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries<IDeletedEntity>().ToList();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Deleted when entry.Entity is DiaryRecord record:
                        // !!! this should be loaded by Include()
                        foreach (var c in record.Cogitations) c.Deleted = true;
                        foreach (var tr in record.ThemesRefs) tr.Deleted = true;
                        foreach (var ti in record.ImagesRefs) ti.Deleted = true;

                        entry.State = EntityState.Modified;
                        entry.Entity.Deleted = true;
                        break;
                    case EntityState.Deleted when entry.Entity is DiaryImage image:
                        // !!! this should be loaded by Include()
                        foreach (var rr in image.RecordsRefs) rr.Deleted = true;

                        entry.State = EntityState.Modified;
                        entry.Entity.Deleted = true;
                        break;
                    case EntityState.Deleted when entry.Entity is DiaryTheme theme:
                        // !!! this should be loaded by Include()
                        foreach (var rr in theme.RecordsRefs!) rr.Deleted = true;

                        entry.State = EntityState.Modified;
                        entry.Entity.Deleted = true;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.Deleted = true;
                        break;
                }
            }
        }
    }

    public sealed class RecordsFilter
    {
        public string? RecordNameFilter { get; set; }

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
        public static bool IsEmpty(RecordsFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            return string.IsNullOrWhiteSpace(filter.RecordNameFilter) &&
            filter.RecordDateFrom == null &&
            filter.RecordDateTo == null &&
            filter.RecordThemeIds.Count == 0;
        }
        public ReadOnlyCollection<Guid> RecordThemeIds { get; private set; } = new ReadOnlyCollection<Guid>(new List<Guid>());

        public void AddThemeId(Guid rtid)
        {
            if (!RecordThemeIds.Contains(rtid))
            {
                var list = new List<Guid>(RecordThemeIds)
                {
                    rtid
                };
                RecordThemeIds = new ReadOnlyCollection<Guid>(list);
            }
        }
        public void AddThemeId(IEnumerable<Guid> idsList)
        {
            if (idsList.Any(i => !RecordThemeIds.Contains(i)))
            {
                RecordThemeIds = new ReadOnlyCollection<Guid>(RecordThemeIds.Union(idsList).ToList());
            }
        }
        public void RemoveThemeId(Guid id)
        {
            if (RecordThemeIds.Contains(id))
            {
                RecordThemeIds = new ReadOnlyCollection<Guid>(RecordThemeIds.Where(i => i != id).ToList());
            }
        }
        public void RemoveThemeId(IEnumerable<Guid> idsList)
        {
            var foundIds = RecordThemeIds.Intersect(idsList);
            if (foundIds.Any())
            {
                RecordThemeIds = new ReadOnlyCollection<Guid>(RecordThemeIds.Except(idsList).ToList());
            }
        }
        public bool IsEmptyTypeFilter => RecordThemeIds.Count == 0;

        public bool CombineThemes { get; set; }
    }

    public static class DiaryDbContextExtensions
    {
        public static async Task<Guid> AddScope(this DiaryDbContext context, string scopeName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(scopeName)) throw new ArgumentException($"Parameter {nameof(scopeName)} should not be null or empty");
            var scope = new DiaryScope
            {
                ScopeName = scopeName
            };
            await context.Scopes.AddAsync(scope);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return scope.Id;
        }

        public static async Task<bool> CanDeleteScope(this DiaryDbContext context, Guid scopeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return !(await context.Themes.AnyAsync(th => th.ScopeId == scopeId).ConfigureAwait(false));
        }

        public static async Task DeleteScope(this DiaryDbContext context, Guid scopeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!(await context.CanDeleteScope(scopeId).ConfigureAwait(false))) return;

            var scope = await context.Scopes
                .Include(s => s.Themes)
                .ThenInclude(t => t.RecordsRefs)
                .SingleOrDefaultAsync(s => s.Id == scopeId)
                .ConfigureAwait(false);

            if (scope != null && !scope.Themes.Any(t => !t.Deleted))
            {
                context.Scopes.Remove(scope);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Guid> UpdateScope(this DiaryDbContext context, DiaryScope scope)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            var targetScope = await context.Scopes.FindAsync(scope.Id);
            if (targetScope == null) throw new ArgumentException($"Scope with id = {scope.Id} is not exists");
            if (targetScope.Deleted) throw new ArgumentException($"Scope with id = {scope.Id} is deleted");

            targetScope.ScopeName = scope.ScopeName;
            await context.SaveChangesAsync().ConfigureAwait(false);

            return targetScope.Id;
        }

        public static Task<DiaryScope> FetchScopeById(this DiaryDbContext context, Guid scopeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Scopes.Include(s => s.Themes).AsNoTracking().SingleOrDefaultAsync(s => s.Id == scopeId);
        }

        public static async Task<List<DiaryScope>> FetchAllScopes(this DiaryDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return await context.Scopes.AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        public static async Task<List<DiaryScope>> FetchScopesWithThemes(this DiaryDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return await context.Scopes.Include(s => s.Themes).AsNoTracking().OrderBy(s => s.ScopeName).ToListAsync().ConfigureAwait(false);
        }

        public static Task<int> GetScopesCount(this DiaryDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Scopes.CountAsync();
        }

        public static async Task<Guid> AddTheme(this DiaryDbContext context, Guid scopeId, string themeName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(themeName))
                throw new ArgumentException($"Parameter {nameof(themeName)} should not be null or empty");

            var theme = new DiaryTheme { ScopeId = scopeId, ThemeName = themeName };
            await context.Themes.AddAsync(theme);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return theme.Id;
        }

        public static async Task<Guid> UpdateTheme(this DiaryDbContext context, DiaryTheme theme)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (theme == null) throw new ArgumentNullException(nameof(theme));

            var targetTheme = await context.Themes.FindAsync(theme?.Id);
            if (targetTheme == null) throw new ArgumentException($"Theme with id = {theme?.Id} is not exists");
            if (targetTheme.Deleted) throw new ArgumentException($"Theme with id = {theme?.Id} is deleted");

            targetTheme.ThemeName = theme?.ThemeName ?? string.Empty;
            await context.SaveChangesAsync().ConfigureAwait(false);

            return targetTheme.Id;
        }

        public static async Task DeleteTheme(this DiaryDbContext context, Guid themeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var theme = await context.Themes
                .Include(t => t.RecordsRefs)
                .SingleOrDefaultAsync(t => t.Id == themeId)
                .ConfigureAwait(false);

            if (theme != null)
            {
                context.Themes.Remove(theme);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static Task<DiaryTheme> FetchThemeById(this DiaryDbContext context, Guid themeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Themes.SingleOrDefaultAsync(t => t.Id == themeId);
        }

        public static async Task<List<DiaryTheme>> FetchThemesOfScope(this DiaryDbContext context, Guid? scopeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return scopeId != null
                ? await context.Themes.Where(t => t.ScopeId == scopeId).ToListAsync().ConfigureAwait(false)
                : await context.Themes.ToListAsync().ConfigureAwait(false);
        }

        public static async Task<List<Guid>> FetchThemesIds(this DiaryDbContext context, Guid? scopeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return (await context.FetchThemesOfScope(scopeId).ConfigureAwait(false)).Select(t => t.Id).ToList();
        }

        public static async Task<int> GetThemesCount(this DiaryDbContext context, Guid? scopeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return (await context.FetchThemesOfScope(scopeId).ConfigureAwait(false)).Count;
        }

        public static async Task AddRecordTheme(this DiaryDbContext context, Guid recordId, Guid themeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var rt = await context.RecordThemes.IgnoreQueryFilters().SingleOrDefaultAsync(rt => rt.RecordId == recordId && rt.ThemeId == themeId).ConfigureAwait(false);
            if (rt != null)
            {
                rt.Deleted = false;
            }
            else
            {
                await context.RecordThemes.AddAsync(new DiaryRecordTheme { RecordId = recordId, ThemeId = themeId });
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public static async Task RemoveRecordTheme(this DiaryDbContext context, Guid recordId, Guid themeId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var rt = await context.RecordThemes.FirstOrDefaultAsync(r => r.RecordId == recordId && r.ThemeId == themeId).ConfigureAwait(false);
            if (rt != null)
            {
                context.RecordThemes.Remove(rt);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static async Task<List<DiaryTheme>> FetchRecordThemes(this DiaryDbContext context, Guid recordId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return await context.RecordThemes
                .Where(rt => rt.RecordId == recordId)
                .Join(context.Themes, rt => rt.ThemeId, t => t.Id, (rt, t) => t)
                .ToListAsync().ConfigureAwait(false);
        }

        public static async Task<Guid> AddImage(this DiaryDbContext context, string imageName, byte[] fullSizeImageData, int imageQuality, DateTime? taken, string? cameraModel)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (imageName == null) throw new ArgumentNullException(nameof(imageName));
            if (fullSizeImageData == null || fullSizeImageData.Length == 0) throw new ArgumentException($"Parameter '{nameof(fullSizeImageData)}' is null or empty");

            var image = new DiaryImage
            {
                Name = imageName,
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                SizeByte = fullSizeImageData.Length,
                Thumbnail = ImageHelper.ScaleImage(fullSizeImageData, imageQuality),
                Taken = taken,
                CameraModel = cameraModel
            };
            (image.Width, image.Height) = ImageHelper.ImageSize(fullSizeImageData);
            await context.Images.AddAsync(image);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var fullImage = new DiaryImageFull
            {
                Data = fullSizeImageData,
                ImageId = image.Id
            };
            await context.FullSizeImages.AddAsync(fullImage);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return image.Id;
        }

        public static async Task DeleteImage(this DiaryDbContext context, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var img = await context.Images
                .Include(i => i.RecordsRefs)
                .Include(i => i.FullImage)
                .Include(i => i.TempImage)
                .FirstOrDefaultAsync(i => i.Id == imageId)
                .ConfigureAwait(false);

            if (img != null)
            {
                context.Images.Remove(img);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static async Task UpdateImageName(this DiaryDbContext context, Guid imageId, string imageNewName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var img = await context.Images.SingleOrDefaultAsync(i => i.Id == imageId).ConfigureAwait(false);
            if (img != null)
            {
                img.Name = imageNewName;
                img.ModifyDate = DateTime.Now;
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static Task<DiaryImage> FetchImageById(this DiaryDbContext context, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Images.SingleOrDefaultAsync(i => i.Id == imageId);
        }

        public static async Task<byte[]> FetchFullImageById(this DiaryDbContext context, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var img = await context.Images.Include(i => i.FullImage).SingleOrDefaultAsync(i => i.Id == imageId).ConfigureAwait(false);
            return img?.FullImage?.Data ?? throw new Exception("Saved image is not contains image data");
        }

        public static Task<int> GetImagesCount(this DiaryDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Images.CountAsync();
        }

        public static async Task<List<DiaryImage>> FetchImageSet(this DiaryDbContext context, int skip, int count)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return await context.Images.OrderByDescending(i => i.CreateDate).Skip(skip).Take(count).ToListAsync().ConfigureAwait(false);
        }

        public static async Task AddRecordImage(this DiaryDbContext context, Guid recordId, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var ri = await context.RecordImages.IgnoreQueryFilters().SingleOrDefaultAsync(ri => ri.RecordId == recordId && ri.ImageId == imageId).ConfigureAwait(false);
            if (ri != null)
            {
                ri.Deleted = false;
            }
            else
            {
                await context.RecordImages.AddAsync(new DiaryRecordImage { ImageId = imageId, RecordId = recordId });
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public static async Task RemoveRecordImage(this DiaryDbContext context, Guid recordId, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var recordImage = await context.RecordImages.FindAsync(recordId, imageId);
            if (recordImage != null)
            {
                context.RecordImages.Remove(recordImage);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static async Task DeleteCogitation(this DiaryDbContext context, Guid cogitationId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var cogitation = await context.Cogitations.SingleOrDefaultAsync(c => c.Id == cogitationId).ConfigureAwait(false);
            if (cogitation != null)
            {
                context.Cogitations.Remove(cogitation);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private const string hostAndPortPlaceholder = "[HOST_AND_PORT]";

        public static async Task<Cogitation?> FetchCogitationById(this DiaryDbContext context, Guid cogitationId, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var c = await context.Cogitations.SingleOrDefaultAsync(c => c.Id == cogitationId).ConfigureAwait(false);
            if (c != null && c.Text != null) c.Text = c.Text.Replace(hostAndPortPlaceholder, localHostAndPort, StringComparison.OrdinalIgnoreCase);
            return c;
        }

        public static async Task<List<Cogitation>> FetchAllCogitationsOfRecord(this DiaryDbContext context, Guid recordId, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var cList = await context.Cogitations.Where(c => c.RecordId == recordId).ToListAsync().ConfigureAwait(false);
            cList.ForEach(c => c.Text = c.Text.Replace(hostAndPortPlaceholder, localHostAndPort, StringComparison.OrdinalIgnoreCase));
            return cList;
        }

        public static async Task<Guid> AddCogitation(this DiaryDbContext context, Cogitation cogitation, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (cogitation == null) throw new ArgumentNullException(nameof(cogitation));
            cogitation.Text = cogitation.Text.Replace(localHostAndPort, hostAndPortPlaceholder, StringComparison.OrdinalIgnoreCase);
            await context.Cogitations.AddAsync(cogitation);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return cogitation.Id;
        }

        public static async Task<Guid> UpdateCogitation(this DiaryDbContext context, Cogitation cogitation, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (cogitation == null) throw new ArgumentNullException(nameof(cogitation));

            var c = await context.Cogitations.FindAsync(cogitation.Id);
            if (c == null) throw new ArgumentException($"Cogitation with id = {cogitation.Id} is not exists");
            if (c.Deleted) throw new ArgumentException($"Cogitation with id = {cogitation.Id} is deleted");
            c.Date = cogitation.Date;
            c.Text = cogitation.Text.Replace(localHostAndPort, hostAndPortPlaceholder, StringComparison.OrdinalIgnoreCase);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return c.Id;
        }

        public static async Task<int> GetCogitationsCount(this DiaryDbContext context, Guid recordId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return await context.Cogitations.CountAsync(c => c.RecordId == recordId).ConfigureAwait(false);
        }

        public static async Task<Guid> AddRecord(this DiaryDbContext context, DiaryRecord record, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (record == null) throw new ArgumentNullException(nameof(record));

            record.Text = record.Text.Replace(localHostAndPort, hostAndPortPlaceholder, StringComparison.OrdinalIgnoreCase);
            await context.Records.AddAsync(record);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return record.Id;
        }

        public static async Task DeleteRecord(this DiaryDbContext context, Guid recordId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var record = await context.Records
                .Include(r => r.Cogitations)
                .Include(r => r.ImagesRefs)
                .Include(r => r.ThemesRefs)
                .FirstOrDefaultAsync(r => r.Id == recordId)
                .ConfigureAwait(false);

            if (record != null)
            {
                context.Records.Remove(record);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static async Task<DiaryRecord?> FetchRecordById(this DiaryDbContext context, Guid recordId, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var r = await context.Records.SingleOrDefaultAsync(r => r.Id == recordId).ConfigureAwait(false);
            if (r != null && r.Text != null) r.Text = r.Text.Replace(hostAndPortPlaceholder, localHostAndPort, StringComparison.OrdinalIgnoreCase);
            return r;
        }

        public static async Task<DiaryRecord?> FetchRecordByIdWithData(this DiaryDbContext context, Guid recordId, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var r = await context.Records
                .AsNoTracking()
                .Include(r => r.Cogitations)
                .Include(r => r.ImagesRefs)
                .ThenInclude(ir => ir.Image)
                .Include(r => r.ThemesRefs)
                .ThenInclude(rt => rt.Theme)
                .SingleOrDefaultAsync(r => r.Id == recordId)
                .ConfigureAwait(false);

            if (r != null && r.Text != null) r.Text = r.Text.Replace(hostAndPortPlaceholder, localHostAndPort, StringComparison.OrdinalIgnoreCase);
            r?.Cogitations?.ToList()?.ForEach(c => c.Text = c.Text.Replace(hostAndPortPlaceholder, localHostAndPort, StringComparison.OrdinalIgnoreCase));

            return r;
        }

        private static IQueryable<DiaryRecord> FetchRecordsListFilteredQuery(DiaryDbContext context, RecordsFilter filter, bool loadAllData)
        {
            IQueryable<DiaryRecord> result;

            if (loadAllData)
            {
                // for 'Retrospective'
                result = context.Records
                    .Include(r => r.Cogitations)
                    //.Include(r => r.ThemesRefs)
                    //.ThenInclude(rt => rt.Theme)
                    .Include(r => r.ImagesRefs)
                    //.ThenInclude(ri => ri.Image)
                    .AsQueryable();
            }
            else
            {
                result = context.Records.AsQueryable();
            }

            if (!filter.IsEmptyTypeFilter)
            {
                IEnumerable<Guid> temp;

                if (filter.CombineThemes)
                {
                    temp = context.RecordThemes
                        .Where(rt => filter.RecordThemeIds.Contains(rt.ThemeId))
                        .Select(rt => rt.RecordId)
                        .Distinct();
                }
                else
                {
                    temp = context.RecordThemes
                        .Where(rt => filter.RecordThemeIds.Contains(rt.ThemeId))
                        .Select(r => new { r.RecordId, r.ThemeId })
                        .ToList()
                        .GroupBy(r => r.RecordId)
                        .Where(g => filter.RecordThemeIds.All(id => g.Select(r => r.ThemeId).Contains(id)))
                        .Select(g => g.Key);
                }

                result = result.Where(r => temp.Contains(r.Id));
            }

            if (!RecordsFilter.IsEmpty(filter))
            {
                if (!string.IsNullOrWhiteSpace(filter.RecordNameFilter))
                {
#pragma warning disable CA1307 // Specify StringComparison
                    result = result.Where(r => r.Name.Contains(filter.RecordNameFilter));
#pragma warning restore CA1307 // Specify StringComparison
                }
                if (filter.RecordDateFrom != null)
                {
                    result = result.Where(r => r.Date >= filter.RecordDateFrom);
                }
                if (filter.RecordDateTo != null)
                {
                    result = result.Where(r => r.Date <= filter.RecordDateTo);
                }
            }

            return result;
        }

        public static async Task<List<DiaryRecord>> FetchRecordsListFiltered(this DiaryDbContext context, RecordsFilter filter, string localHostAndPort, bool loadAllData = false)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var rList = await FetchRecordsListFilteredQuery(context, filter, loadAllData)
                .OrderByDescending(r => r.Date)
                .Skip(filter.PageNo * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync().ConfigureAwait(false);

            rList.ForEach(r => r.Text = r.Text?.Replace(hostAndPortPlaceholder, localHostAndPort, StringComparison.OrdinalIgnoreCase) ?? string.Empty);
            return rList;
        }

        public static Task<int> GetFilteredRecordsCount(this DiaryDbContext context, RecordsFilter filter)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            return FetchRecordsListFilteredQuery(context, filter, false).CountAsync();
        }

        public static async Task<Guid> UpdateRecord(this DiaryDbContext context, DiaryRecord record, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var destRec = await context.Records.FindAsync(record.Id);
            if (destRec == null) throw new ArgumentException($"Record with ID={record.Id} is not found");
            if (destRec.Deleted) throw new ArgumentException($"Record with ID={record.Id} is deleted");

            destRec.Date = record.Date;
            destRec.CreateDate = record.CreateDate;
            destRec.ModifyDate = record.ModifyDate;
            destRec.Name = record.Name;
            destRec.Text = record.Text.Replace(localHostAndPort, hostAndPortPlaceholder, StringComparison.OrdinalIgnoreCase);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return destRec.Id;
        }

        public static async Task<Guid> UpdateCogitationText(this DiaryDbContext context, Guid cogitationId, string text, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (text == null) throw new ArgumentNullException(nameof(text));

            var c = await context.Cogitations.FindAsync(cogitationId);
            if (c == null) throw new ArgumentException($"Cogitation with id = {cogitationId} is not exists");
            if (c.Deleted) throw new ArgumentException($"Cogitation with id = {cogitationId} is deleted");

            c.Text = text.Replace(localHostAndPort, hostAndPortPlaceholder, StringComparison.OrdinalIgnoreCase);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return c.Id;
        }

        public static async Task<string?> GetAppSetting(this DiaryDbContext context, string key)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key should not be null or empty");
            return (await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key).ConfigureAwait(false))?.Value;
        }

        public static async Task<int?> GetAppSettingInt(this DiaryDbContext context, string key)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var str = await context.GetAppSetting(key).ConfigureAwait(false);
            if (int.TryParse(str, NumberStyles.None, CultureInfo.CurrentCulture.NumberFormat, out int result))
            {
                return result;
            }
            return null;
        }

        public static async Task UpdateAppSetting(this DiaryDbContext context, string key, string value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key should not be null or empty");
            if (value == null) throw new ArgumentException("Value should not be null or empty");

            var appSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key).ConfigureAwait(false);

            if (appSetting == null)
            {
                await context.AppSettings.AddAsync(new AppSetting
                {
                    Key = key,
                    Value = value,
                    ModifiedDate = DateTime.Now
                });
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                if (appSetting.Value != value)
                {
                    appSetting.Value = value;
                    appSetting.ModifiedDate = DateTime.Now;
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task<List<DateItem>> FetchDateItems(this DiaryDbContext context, Guid scopeId, DatesRange datesRange, string localHostAndPort)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var recThemes = await context.RecordThemes
                .Include(rt => rt.Theme)
                .Include(rt => rt.Record)
                .Where(rt => rt.Theme != null && rt.Theme.ScopeId == scopeId)
                .ToListAsync().ConfigureAwait(false);

            recThemes = recThemes
                .GroupBy(rt => rt.RecordId)
                .Select(gb => gb.First())
                .Where(rt => rt.Record != null && datesRange.IsDateInRange(rt.Record.Date))
                .ToList();

            var dateItems = recThemes
                .Select(rt => new DateItem(
                    datesRange,
                    rt.Record!.Id,
                    rt.Theme!.ThemeName,
                    rt.Record.Date,
                    rt.Record.Name,
                    rt.Record.Text.Replace(hostAndPortPlaceholder, localHostAndPort, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(i => i.TransferredDate)
                .ToList();

            return dateItems;
        }

        public static Task<List<DateItem>> FetchAllDateItems(this DiaryDbContext context, Guid scopeId, string localHostAndPort)
        {
            return context.FetchDateItems(scopeId, DatesRange.ForAllYear(), localHostAndPort);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "<Pending>")]
        private static IQueryable<DiaryRecord> SearchRecords(this DiaryDbContext context, string searchText)
        {
            return context.Records

                .Where(r => r.Name.Contains(searchText) ||
                    r.Text.Contains(searchText) ||
                    context.Cogitations.Any(c => c.RecordId == r.Id && c.Text.Contains(searchText)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "<Pending>")]
        public static async Task<List<DiaryRecord>> SearchRecordsByText(this DiaryDbContext context, string searchText, int skip, string localHostAndPort, int count = 20)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var rList = await context.SearchRecords(searchText)
                .OrderByDescending(r => r.Date)
                .Skip(skip)
                .Take(count)
                .ToListAsync().ConfigureAwait(false);

            rList.ForEach(r => r.Text = r.Text.Replace(hostAndPortPlaceholder, localHostAndPort));
            return rList;
        }

        public static Task<int> SearchRecordsByTextCount(this DiaryDbContext context, string searchText)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.SearchRecords(searchText).CountAsync();
        }

        public static async Task ClearDbFromDeletedRecords(this DiaryDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.SoftDeleting = false;

            var recImages = context.RecordImages.IgnoreQueryFilters().Where(ri => ri.Deleted);
            context.RecordImages.RemoveRange(recImages);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var images = context.Images.IgnoreQueryFilters().Where(i => i.Deleted);
            context.Images.RemoveRange(images);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var recThemes = context.RecordThemes.IgnoreQueryFilters().Where(rt => rt.Deleted);
            context.RecordThemes.RemoveRange(recThemes);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var themes = context.Themes.IgnoreQueryFilters().Where(t => t.Deleted);
            context.Themes.RemoveRange(themes);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var scopes = context.Scopes.IgnoreQueryFilters().Where(s => s.Deleted);
            context.Scopes.RemoveRange(scopes);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var records = context.Records.IgnoreQueryFilters().Where(r => r.Deleted);
            var cogitations = records.SelectMany(r => context.Cogitations.Where(c => c.RecordId == r.Id));
            context.Cogitations.RemoveRange(cogitations);
            context.Records.RemoveRange(records);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public static async Task Vacuum(this DiaryDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            await context.Database.ExecuteSqlRawAsync("vacuum;").ConfigureAwait(false);
        }

        public static Task<Dictionary<Guid, string>> FetchRecordsForImage(this DiaryDbContext context, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.RecordImages.Where(ri => ri.ImageId == imageId)
                .Join(context.Records, ri => ri.RecordId, r => r.Id, (ri, r) => new { r.Id, r.Name })
                .ToDictionaryAsync(r => r.Id, r => r.Name);
        }

        public static Task<TempImage> FetchTempImage(this DiaryDbContext context, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.TempImages.FirstOrDefaultAsync(t => t.SourceImageId == imageId);
        }

        public static async Task AddUnsavedTempImage(this DiaryDbContext context, TempImage image)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var oldImages = context.TempImages.Where(t => t.SourceImageId == image.SourceImageId);
            if (oldImages.Any())
            {
                context.TempImages.RemoveRange(oldImages);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            await context.TempImages.AddAsync(image);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public static async Task ApplyChangesFromTempImage(this DiaryDbContext context, TempImage tempImage, int imageQuality)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (tempImage == null) throw new ArgumentNullException(nameof(tempImage));

            var image = await context.Images.FindAsync(tempImage.SourceImageId) ?? throw new ArgumentException($"Image with Id ={tempImage.SourceImageId} is not found");
            var fullImage = await context.FullSizeImages
                .SingleOrDefaultAsync(fi => fi.ImageId == tempImage.SourceImageId)
                .ConfigureAwait(false) ??
                throw new ArgumentException($"Full image with Id ={tempImage.SourceImageId} is not found! Database is inconsist");

            if (image.Deleted) throw new ArgumentException("Image is deleted. Can't update deleted image");

            image.Thumbnail = ImageHelper.ScaleImage(tempImage.Data, imageQuality);
            image.ModifyDate = DateTime.Now;
            image.SizeByte = tempImage.Data.Length;
            (image.Width, image.Height) = ImageHelper.ImageSize(tempImage.Data);

            fullImage.Data = tempImage.Data;

            context.TempImages.Remove(await context.TempImages.FindAsync(tempImage.Id));

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public static async Task DeleteTempImage(this DiaryDbContext context, Guid imageId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var oldImages = context.TempImages.Where(t => t.SourceImageId == imageId);
            if (oldImages.Any())
            {
                context.TempImages.RemoveRange(oldImages);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static async Task ChangeThemeActuality(this DiaryDbContext context, Guid themeId, bool actual)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var theme = await context.Themes.FindAsync(themeId);
            if (theme.Actual != actual)
            {
                theme.Actual = actual;
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public static async Task<List<CalendarRecordItem>> FetchCalendarDates(this DiaryDbContext context, int? year, Guid[] themes, bool combineThemes)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var calendarItems = new List<CalendarRecordItem>();
            if (year != null)
            {
                var firstYearDay = new DateTime(year.Value, 01, 01);
                var lastYearDay = new DateTime(year.Value, 12, 31);

                calendarItems = await context.Records
                    .Where(r => r.Date >= firstYearDay && r.Date <= lastYearDay)
                    .Select(r => new CalendarRecordItem(r.Id, r.Name, r.Date))
                    .ToListAsync().ConfigureAwait(false);
            }
            else
            {
                calendarItems = await context.Records
                    .Select(r => new CalendarRecordItem(r.Id, r.Name, r.Date))
                    .ToListAsync().ConfigureAwait(false);
            }

            if (themes != null && themes.Length > 0)
            {
                var recordThemes = await context.RecordThemes.ToListAsync().ConfigureAwait(false);

                if (combineThemes)
                {
                    return calendarItems
                        .Where(ci => recordThemes.Any(rt => rt.RecordId == ci.Id && themes.Contains(rt.ThemeId))).ToList();
                }
                else
                {
                    return calendarItems
                        .Where(ci =>
                            themes.All(id => recordThemes
                                .Where(rt => rt.RecordId == ci.Id)
                                .Select(rt => rt.ThemeId).Contains(id)))
                        .ToList();
                }
            }

            return calendarItems;
        }

        public static async Task<List<DiaryScope>> GetAllScopes(this DiaryDbContext context)
        {
            return await (context?.Scopes
                .AsNoTracking()
                .Include(s => s.Themes)
                .ToListAsync()
                .ConfigureAwait(false) ?? throw new ArgumentNullException(nameof(context)));
        }

        public static async Task<List<DiaryImage>> FetchImagesFiltered(this DiaryDbContext context, Guid recordId, string filterPart)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var images = !string.IsNullOrWhiteSpace(filterPart)
                ? context.Images.Where(i => !i.Deleted && i.Name.Contains(filterPart, StringComparison.OrdinalIgnoreCase))
                : context.Images.Where(i => !i.Deleted);

            images = recordId != Guid.Empty
                ? images.Where(i => !context.RecordImages.Where(ri => !ri.Deleted).Any(ri => ri.RecordId == recordId && ri.ImageId == i.Id))
                : images;

            var filteredImages = await images
                .OrderByDescending(i => i.CreateDate)
                .Take(10)
                .ToListAsync()
                .ConfigureAwait(false);

            return filteredImages;
        }
    }
}
