using Microsoft.EntityFrameworkCore;
using RiseDiary.Model;
using RiseDiary.WebAPI.Scopes.Model;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.Data;

public sealed class DiaryDbContext : DbContext
{
    public DiaryDbContext(DbContextOptions<DiaryDbContext> options) : base(options) { }

    public DbSet<ScopeEntity> Scopes { get; set; } = null!;
    public DbSet<ThemeEntity> Themes { get; set; } = null!;
    public DbSet<ImageEntity> Images { get; set; } = null!;
    public DbSet<RecordEntity> Records { get; set; } = null!;
    public DbSet<RecordCommentEntity> Cogitations { get; set; } = null!;
    public DbSet<RecordThemeEntity> RecordThemes { get; set; } = null!;
    public DbSet<RecordImageEntity> RecordImages { get; set; } = null!;
    public DbSet<SettingEntity> AppSettings { get; set; } = null!;
    public DbSet<TempImageEntity> TempImages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<RecordEntity>().Property(r => r.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<RecordEntity>().HasMany(r => r.Cogitations)
            .WithOne(c => c.Record!)
            .HasForeignKey(c => c.RecordId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RecordEntity>().HasMany(r => r.ThemesRefs)
            .WithOne(tr => tr.Record!)
            .HasForeignKey(tr => tr.RecordId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RecordEntity>().HasMany(r => r.ImagesRefs)
            .WithOne(ir => ir.Record!)
            .HasForeignKey(ir => ir.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecordCommentEntity>().Property(c => c.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<ScopeEntity>().Property(s => s.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<ScopeEntity>()
            .HasMany(s => s.Themes)
            .WithOne(t => t.Scope!)
            .HasForeignKey(t => t.ScopeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ThemeEntity>().Property(t => t.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<ThemeEntity>()
            .HasMany(t => t.RecordsRefs)
            .WithOne(rt => rt.Theme!)
            .HasForeignKey(rt => rt.ThemeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecordThemeEntity>().HasKey(nameof(RecordThemeEntity.RecordId), nameof(RecordThemeEntity.ThemeId));
        modelBuilder.Entity<RecordThemeEntity>().HasIndex(nameof(RecordThemeEntity.RecordId), nameof(RecordThemeEntity.ThemeId));

        modelBuilder.Entity<ImageEntity>().Property(i => i.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<ImageEntity>()
           .HasOne(i => i.TempImage)
           .WithOne()
           .HasForeignKey<TempImageEntity>(ti => ti.SourceImageId)
           .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ImageEntity>()
            .HasMany(i => i.RecordsRefs)
            .WithOne(rr => rr.Image!)
            .HasForeignKey(rr => rr.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecordImageEntity>().HasKey(nameof(RecordImageEntity.RecordId), nameof(RecordImageEntity.ImageId));
        modelBuilder.Entity<RecordImageEntity>().HasIndex(nameof(RecordImageEntity.RecordId), nameof(RecordImageEntity.ImageId));

        modelBuilder.Entity<SettingEntity>()
            .Property(x => x.Key)
            .HasConversion(v => (int)v, i => (SettingsKey)i);

        modelBuilder.Entity<SettingEntity>().HasKey(s => s.Key);

        // soft deleting
        modelBuilder.Entity<RecordEntity>().HasQueryFilter(r => !r.Deleted);
        modelBuilder.Entity<RecordCommentEntity>().HasQueryFilter(c => !c.Deleted);
        modelBuilder.Entity<ImageEntity>().HasQueryFilter(i => !i.Deleted);
        modelBuilder.Entity<ThemeEntity>().HasQueryFilter(t => !t.Deleted);
        modelBuilder.Entity<ScopeEntity>().HasQueryFilter(s => !s.Deleted);
        modelBuilder.Entity<RecordThemeEntity>().HasQueryFilter(rt => !rt.Deleted);
        modelBuilder.Entity<RecordImageEntity>().HasQueryFilter(ri => !ri.Deleted);
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
        // todo переписать на interceptors
        var entries = ChangeTracker.Entries<IDeletableEntity>().ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Deleted when entry.Entity is RecordEntity record:
                    // !!! this should be loaded by Include()
                    foreach (var c in record?.Cogitations ?? Enumerable.Empty<RecordCommentEntity>()) c.Deleted = true;
                    foreach (var tr in record?.ThemesRefs ?? Enumerable.Empty<RecordThemeEntity>()) tr.Deleted = true;
                    foreach (var ti in record?.ImagesRefs ?? Enumerable.Empty<RecordImageEntity>()) ti.Deleted = true;

                    entry.State = EntityState.Modified;
                    entry.Entity.Deleted = true;
                    break;
                case EntityState.Deleted when entry.Entity is ImageEntity image:
                    // !!! this should be loaded by Include()
                    foreach (var rr in image.RecordsRefs) rr.Deleted = true;

                    if (image.TempImage != null)
                    {
                        Entry(image.TempImage).State = EntityState.Unchanged;
                    }

                    entry.State = EntityState.Modified;
                    entry.Entity.Deleted = true;
                    break;
                case EntityState.Deleted when entry.Entity is ThemeEntity theme:
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
