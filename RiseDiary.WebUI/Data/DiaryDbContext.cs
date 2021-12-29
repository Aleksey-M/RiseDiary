using Microsoft.EntityFrameworkCore;
using RiseDiary.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Data
{
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
                .WithOne()
                .HasForeignKey<DiaryImageFull>(fi => fi.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DiaryImage>()
               .HasOne(i => i.TempImage)
               .WithOne()
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
                        foreach (var c in record?.Cogitations ?? Enumerable.Empty<Cogitation>()) c.Deleted = true;
                        foreach (var tr in record?.ThemesRefs ?? Enumerable.Empty<DiaryRecordTheme>()) tr.Deleted = true;
                        foreach (var ti in record?.ImagesRefs ?? Enumerable.Empty<DiaryRecordImage>()) ti.Deleted = true;

                        entry.State = EntityState.Modified;
                        entry.Entity.Deleted = true;
                        break;
                    case EntityState.Deleted when entry.Entity is DiaryImage image:
                        // !!! this should be loaded by Include()
                        foreach (var rr in image.RecordsRefs) rr.Deleted = true;

                        if(image.FullImage is not null)
                        {
                            Entry(image.FullImage).State = EntityState.Unchanged;
                        }                        

                        if(image.TempImage != null)
                        {
                            Entry(image.TempImage).State = EntityState.Unchanged;
                        }

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
}
