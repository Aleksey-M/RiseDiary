using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RiseDiary.Data;
using RiseDiary.IntegratedTests.Services;
using RiseDiary.IntegratedTests.Stubs;
using RiseDiary.Model;

#pragma warning disable CA1822 // Mark members as static

namespace RiseDiary.IntegratedTests;

internal class TestFixtureBase
{
    [OneTimeTearDown]
    public void CleanUp()
    {
        RemoveTmpDbFiles();
    }

    protected const string FullImage_1280X814 = @"images/TestImage.jpg";
    protected const string FullImage_512X341 = @"images/TestImage2.jpg";
    protected const string FullImage_341X512 = @"images/TestImage3.jpg";
    protected const string FullImage_597X1300 = @"images/TestImage4.jpg";
    protected const string FullImage_1587X544 = @"images/TestImage5.jpg";

    protected static IFormFile TestFile
    {
        get
        {
            var bytes = File.ReadAllBytes(FullImage_1280X814);
            var s = new MemoryStream(bytes);
            return new FormFile(s, 0, s.Length, "UploadImage", FullImage_1280X814);
        }
    }

    private readonly static List<string> _dbFileNames = new();

    protected static string DirNameFull => AppDomain.CurrentDomain.BaseDirectory ?? throw new ArgumentNullException(nameof(AppDomain.CurrentDomain.BaseDirectory));

    protected static DiaryDbContext CreateContext()
    {
        var (context, _) = GetContextWithFileName();
        return context;
    }

    private static DbConnection CreateInMemoryDatabase()
    {
        var connection = new SqliteConnection("Filename=:memory:");

        connection.Open();

        return connection;
    }

    protected static (DiaryDbContext context, string fileName) GetContextWithFileName(bool inMemory = true)
    {
        var dbFileFullName = inMemory ? ":memory:" : Path.Combine(DirNameFull, Path.GetFileName(Path.GetTempFileName()));

        if (!inMemory)
        {
            if (File.Exists(dbFileFullName))
            {
                File.Delete(dbFileFullName);
            }
            _dbFileNames.Add(dbFileFullName);
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
        .Build();

        var builder = new DbContextOptionsBuilder<DiaryDbContext>();
        if (inMemory)
        {

            builder.UseSqlite(CreateInMemoryDatabase(), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
        else
        {
            builder.UseSqlite($"Data Source={dbFileFullName};", o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
        builder.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

        var context = new DiaryDbContext(builder.Options);
        context.Database.EnsureCreated();

        return (context, dbFileFullName);
    }

    protected static void RemoveTmpDbFiles()
    {
        _dbFileNames.ForEach(fn => File.Delete(fn));
        _dbFileNames.Clear();
    }

    protected static DiaryRecord GetTestRecord(int? yearsAdd = null, int? month = null)
    {
        DateTime now = DateTime.UtcNow;
        if (yearsAdd == null)
            now = month == null ? now : new DateTime(now.Year, (int)month, 10);
        else
            now = month == null ? now.AddYears((int)yearsAdd) : new DateTime(now.Year + (int)yearsAdd, (int)month, 10);

        return new DiaryRecord
        {
            Date = DateOnly.FromDateTime(now),
            CreateDate = DateTime.UtcNow,
            ModifyDate = DateTime.UtcNow,
            Name = @"/-*/--!@#$+_)*&(^*^$#?>?<ЪъЇЇіячсіопдоавйцу",
            Text = @";'][[p][p]}{}{Pрпорпырорпорёёёё"
        };
    }

    protected static Guid CreateRecord(DiaryDbContext context)
    {
        var rec = GetTestRecord();
        context.Records.Add(rec);
        context.SaveChanges();
        return rec.Id;
    }

    protected static DiaryImage GetTestImage(string fileName)
    {
        var data = File.ReadAllBytes(fileName);
        var (w, h) = ImageHelper.GetImageSize(data);

        return new DiaryImage
        {
            Name = Guid.NewGuid().ToString(),
            Thumbnail = File.ReadAllBytes(FullImage_1280X814),
            CameraModel = "Some model",
            Taken = DateTime.UtcNow.AddDays(-2),
            Deleted = false,
            Height = h,
            Width = w,
            CreateDate = DateTime.UtcNow.AddHours(-3),
            ModifyDate = DateTime.UtcNow.AddHours(-2),
            SizeByte = 120000,
            Id = Guid.NewGuid(),
            FullImage = new DiaryImageFull
            {
                Id = Guid.NewGuid(),
                Data = data
            }
        };
    }

    protected static Guid CreateImage(DiaryDbContext context, string imageName = FullImage_512X341)
    {
        var img = GetTestImage(imageName);
        context.Images.Add(img);
        context.SaveChanges();
        return img.Id;
    }

    protected static (Guid recId, Guid cogId) Create3RecordsAnd1Cogitation(DiaryDbContext context)
    {
        Guid recId;

        context.Records.Add(new Model.DiaryRecord
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Name = "first",
            Text = "1111"
        });
        context.Records.Add(new Model.DiaryRecord
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Name = "second",
            Text = "2222"
        });
        context.Records.Add(new Model.DiaryRecord
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Name = "third",
            Text = "3333"
        });
        context.SaveChanges();
        recId = context.Records.Where(r => r.Name == "second").First().Id;

        var cogitation = new Model.Cogitation
        {
            RecordId = recId,
            Text = "COGITATION"
        };
        context.Cogitations.Add(cogitation);
        context.SaveChanges();

        return (recId, cogitation.Id);
    }

    protected static IEnumerable<string> GetNumberList(int count, string? prefix = null) => Enumerable.Range(1, count).Select(i => prefix ?? "" + i.ToString("00", CultureInfo.InvariantCulture));
    protected static IEnumerable<DateOnly> GetDatesList(int count) => Enumerable.Range(1, count).Select(i => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)));
    protected static IEnumerable<DateOnly> GetDatesListWithTwoSameDatesWeekAgo(int count) => Enumerable.Range(1, count).Select(i => i == 2 ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)) : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)));

    protected static void Create20Records(DiaryDbContext context, IEnumerable<string> _20recordNames, IEnumerable<DateOnly> _20recordDates, List<string>? _20recordsText = null)
    {
        if (_20recordDates.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordDates));
        if (_20recordNames.Count() != 20) throw new ArgumentOutOfRangeException(nameof(_20recordNames));

        var recList = _20recordNames.Select((n, i) => new DiaryRecord { Name = n, Date = _20recordDates.ElementAt(i) }).ToList();

        if (_20recordsText != null)
        {
            for (int i = 0; i < 20; i++)
            {
                recList[i].Text = _20recordsText[i];
            }
        }

        context.Records.AddRange(recList);
        context.SaveChanges();
    }

    protected static void Create30ThemesAnd20Records(DiaryDbContext context, IEnumerable<string> _20recordNames, IEnumerable<DateOnly> _20recordDates)
    {
        Create20Records(context, _20recordNames, _20recordDates);

        var scope = new DiaryScope { ScopeName = "Test" };
        context.Scopes.Add(scope);
        context.SaveChanges();

        context.Themes.AddRange(Enumerable.Range(0, 30).Select(i => new DiaryTheme { ScopeId = scope.Id, ThemeName = i.ToString(CultureInfo.InvariantCulture) }));
        context.SaveChanges();
    }

    protected static void BindRecordsWithThemes(DiaryDbContext context, Dictionary<string, List<string>> binding)
    {
        foreach (var recordTheme in binding)
        {
            foreach (var tName in recordTheme.Value)
            {
                context.RecordThemes.Add(new Model.DiaryRecordTheme
                {
                    RecordId = context.Records.First(r => r.Name == recordTheme.Key).Id,
                    ThemeId = context.Themes.First(r => r.ThemeName == tName).Id,
                });
            }
        }
        context.SaveChanges();
    }

    protected static Guid CreateScope(DiaryDbContext context, string? scopeName = null, string? scopeDescription = null)
    {
        var scope = new DiaryScope
        {
            ScopeName = scopeName ?? Guid.NewGuid().ToString(),
            Description = scopeDescription ?? ""
        };

        context.Scopes.Add(scope);
        context.SaveChanges();
        return scope.Id;
    }

    protected static (Guid scopeId, Guid themeId) CreateThemeWithScope(
        DiaryDbContext context, string themeName, string? themeDescription = null)
    {
        var scopeId = CreateScope(context);

        var theme = new DiaryTheme
        {
            Id = Guid.NewGuid(),
            ScopeId = scopeId,
            ThemeName = themeName,
            Description = themeDescription ?? "",
            Actual = true
        };

        context.Themes.Add(theme);
        context.SaveChanges();
        return (scopeId, theme.Id);
    }

    protected static Guid CreateTheme(DiaryDbContext context, string themeName, Guid? scopeId = null, string? themeDescription = null)
    {
        if (scopeId == null)
        {
            var (_, tId) = CreateThemeWithScope(context, themeName);
            return tId;
        }

        var theme = new DiaryTheme
        {
            Id = Guid.NewGuid(),
            ScopeId = scopeId.Value,
            ThemeName = themeName,
            Description = themeDescription ?? "",
            Actual = true
        };

        context.Themes.Add(theme);
        context.SaveChanges();
        return theme.Id;
    }

    protected static List<DiaryScope> Create3ScopesWith1ThemeForEach(DiaryDbContext context)
    {
        for (int i = 0; i < 3; i++)
        {
            var theme = new DiaryTheme
            {
                Id = Guid.NewGuid(),
                ThemeName = $"Theme For Scope {i + 1}",
                Scope = new DiaryScope
                {
                    Id = Guid.NewGuid(),
                    ScopeName = $"Scope {i + 1}"
                }
            };
            context.Themes.Add(theme);
        }
        context.SaveChanges();
        return context.Scopes.ToList();
    }

    protected static (DiaryRecord record, DiaryScope scope, DiaryImage image) CreateEntities(DiaryDbContext context)
    {
        var rec = GetTestRecord();
        var theme = new DiaryTheme
        {
            ThemeName = "Some Theme",
            Actual = true,
            Scope = new DiaryScope { ScopeName = $"Some Scope" }
        };

        var img = GetTestImage(FullImage_512X341);

        context.Add(rec);
        context.Add(theme);
        context.Add(img);

        context.SaveChanges();

        context.Add(new DiaryRecordImage { Record = rec, Image = img });
        context.Add(new DiaryRecordTheme { Record = rec, Theme = theme });
        context.Add(new Cogitation { Record = rec, Date = DateTime.UtcNow, Text = "Some Cogitation text" });
        context.SaveChanges();

        return (rec, theme.Scope, img);
    }

    protected static async Task<List<DiaryRecord>> AddSetOfRecordsWithDates(DiaryDbContext context, IEnumerable<DateOnly> recDates)
    {
        var recs = recDates.Select(d => new DiaryRecord
        {
            Date = d,
            CreateDate = d.ToDateTime(TimeOnly.MinValue),
            ModifyDate = d.ToDateTime(TimeOnly.MinValue),
            Id = Guid.NewGuid(),
            Name = "Record for date " + d.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture),
            Text = "Record for date " + d.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture)
        }).ToList();

        context.Records.AddRange(recs);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return recs;
    }

    protected async static Task<List<(DateOnly date, List<Guid> themesIds)>> AddThemesForRecords(DiaryDbContext context, Dictionary<DateOnly, List<string>> themesByDate)
    {
        _ = await AddSetOfRecordsWithDates(context, themesByDate.Select(tbd => tbd.Key).ToList());

        var addedThemes = new List<DiaryTheme>();

        foreach (var kv in themesByDate)
        {
            var rec = context.Records.Single(r => r.Date == kv.Key);

            foreach (var tn in kv.Value)
            {
                var themeAdded = addedThemes.Any(t => t.ThemeName == tn);
                if (!themeAdded)
                {
                    var theme = new DiaryTheme
                    {
                        ThemeName = tn,
                        Id = Guid.NewGuid(),
                        Scope = new DiaryScope
                        {
                            Id = Guid.NewGuid(),
                            ScopeName = "Scope For " + kv.Value
                        }
                    };

                    context.Themes.Add(theme);
                    context.SaveChanges();

                    addedThemes.Add(theme);
                }
            }

            foreach (var tn in kv.Value)
            {
                var t = addedThemes.Single(tt => tt.ThemeName == tn);

                context.RecordThemes.Add(new DiaryRecordTheme { RecordId = rec.Id, ThemeId = t.Id });
            }

            context.SaveChanges();
        }

        var res = context.RecordThemes.Include(rt => rt.Record).ToList();

        var retRes = new List<(DateOnly date, List<Guid> themesIds)>();
        foreach (var kv in themesByDate)
        {
            retRes.Add((kv.Key, res.Where(r => r.Record!.Date == kv.Key).Select(rt => rt.ThemeId).ToList()));
        }

        return retRes;
    }

    protected static async Task<DiaryImage> CreateImageWithTempImage(DiaryDbContext context)
    {
        var imgData = File.ReadAllBytes(FullImage_1280X814);
        var imgData2 = File.ReadAllBytes(FullImage_512X341);
        var (w, h) = ImageHelper.GetImageSize(imgData2);

        var image = new DiaryImage
        {
            Id = Guid.NewGuid(),
            Name = "new image " + Guid.NewGuid().ToString(),
            CreateDate = DateTime.UtcNow,
            FullImage = new DiaryImageFull
            {
                Id = Guid.NewGuid(),
                Data = imgData
            },
            Height = 3000,
            Width = 4000,
            CameraModel = "camera model",
            Taken = DateTime.UtcNow.AddDays(-1),
            SizeByte = imgData.Length,
            ModifyDate = DateTime.UtcNow,
            TempImage = new TempImage
            {
                Id = Guid.NewGuid(),
                Data = imgData2,
                Modification = "Some modification",
                Height = h,
                Width = w,
                SizeByte = imgData2.Length
            }
        };

        await context.Images.AddAsync(image);
        await context.SaveChangesAsync();

        return image;
    }

    protected async Task Add3ImagesForEachRecord(DiaryDbContext context)
    {
        await foreach (var rec in context.Records.Include(r => r.ImagesRefs).AsAsyncEnumerable())
        {
            var recordImages = Enumerable.Range(1, 3)
                .Select(_ => GetTestImage(FullImage_512X341))
                .Select(img => new DiaryRecordImage { Image = img, Record = rec })
                .ToList();

            await context.RecordImages.AddRangeAsync(recordImages);
        }

        await context.SaveChangesAsync();
    }

    protected async Task<(Guid, Guid)> For2RecordsRemoveImagesAndGetThousIds(DiaryDbContext context)
    {
        int count = await context.Records.CountAsync();
        var rnd = new Random();

        int ind1 = rnd.Next(0, count / 2);
        var rec = (await context.Records.Include(r => r.ImagesRefs).ToListAsync()).ElementAt(ind1);
        rec.ImagesRefs.First().Deleted = true;
        var imgId1 = rec.ImagesRefs.First().ImageId;

        int ind2 = rnd.Next(count / 2, count - 1);
        rec = (await context.Records.Include(r => r.ImagesRefs).ToListAsync()).ElementAt(ind2);
        rec.ImagesRefs.First().Deleted = true;
        var imgId2 = rec.ImagesRefs.First().ImageId;

        await context.SaveChangesAsync();

        return (imgId1, imgId2);
    }

    protected async Task Add3ThemesForEachRecord(DiaryDbContext context)
    {
        await foreach (var rec in context.Records.Include(r => r.ThemesRefs).AsAsyncEnumerable())
        {
            var themesList = Enumerable.Range(1, 3)
                .Select(c => new DiaryTheme
                {
                    Id = Guid.NewGuid(),
                    ThemeName = $"Theme {c}",
                    Scope = new DiaryScope
                    {
                        Id = Guid.NewGuid(),
                        ScopeName = $"Scope {Guid.NewGuid()} {c}"
                    }
                });

            await context.Themes.AddRangeAsync(themesList);
            await context.RecordThemes.AddRangeAsync(themesList.Select(t => new DiaryRecordTheme { Record = rec, Theme = t }));
        }

        await context.SaveChangesAsync();
    }

    protected async Task<(Guid, Guid)> For2RecordsRemoveThemesAndGetThousIds(DiaryDbContext context)
    {
        int count = await context.Records.CountAsync();
        var rnd = new Random();

        int ind1 = rnd.Next(0, count / 2);
        var rec = (await context.Records.Include(r => r.ThemesRefs).ToListAsync()).ElementAt(ind1);
        rec.ThemesRefs.First().Deleted = true;
        var themId1 = rec.ThemesRefs.First().ThemeId;

        int ind2 = rnd.Next(count / 2, count - 1);
        rec = (await context.Records.Include(r => r.ThemesRefs).ToListAsync()).ElementAt(ind2);
        rec.ThemesRefs.First().Deleted = true;
        var themId2 = rec.ThemesRefs.First().ThemeId;

        await context.SaveChangesAsync();

        return (themId1, themId2);
    }

    protected async Task Add3CogitationsForEachRecord(DiaryDbContext context, string? prefix = null, IEnumerable<string>? additionalCogitationsText = null)
    {
        await foreach (var rec in context.Records.Include(r => r.Cogitations).AsAsyncEnumerable())
        {
            Enumerable.Range(1, 3)
                .Select(i => new Cogitation { Id = Guid.NewGuid(), Record = rec, Text = prefix ?? "" + Guid.NewGuid().ToString() })
                .ToList()
                .ForEach(c => context.Cogitations.Add(c));
        }

        if (additionalCogitationsText != null)
        {
            context.Records
                .ToList()
                .Zip(additionalCogitationsText, (rec, cogText) => new Cogitation { Id = Guid.NewGuid(), Record = rec, Text = cogText })
                .ToList()
                .ForEach(c => context.Cogitations.Add(c));
        }

        await context.SaveChangesAsync();
    }

    protected async Task<(Guid, Guid)> For2RecordsRemoveCogitationsAndGetThousIds(DiaryDbContext context)
    {
        int count = await context.Records.CountAsync();
        var rnd = new Random();

        int ind1 = rnd.Next(0, count / 2);
        var rec = (await context.Records.Include(r => r.Cogitations).ToListAsync()).ElementAt(ind1);
        rec.Cogitations.First().Deleted = true;
        var cogId1 = rec.Cogitations.First().Id;

        int ind2 = rnd.Next(count / 2, count - 1);
        rec = (await context.Records.Include(r => r.Cogitations).ToListAsync()).ElementAt(ind2);
        rec.Cogitations.First().Deleted = true;
        var cogId2 = rec.Cogitations.First().Id;

        await context.SaveChangesAsync();

        return (cogId1, cogId2);
    }

    protected async Task<(Guid, Guid)> For2RecordsAddLinkToName(DiaryDbContext context, HostAndPortStub hostAndPortService)
    {
        int count = await context.Records.CountAsync();
        var rnd = new Random();

        int ind1 = rnd.Next(0, count / 2);
        var rec = (await context.Records.ToListAsync()).ElementAt(ind1);
        rec.Name += $@"Link: <a href=""{hostAndPortService.GetHostAndPortPlaceholder()}/images/123"">Some Image</a>";
        var rec1 = rec.Id;

        int ind2 = rnd.Next(count / 2, count - 1);
        rec = (await context.Records.ToListAsync()).ElementAt(ind2);
        rec.Name += $@"Link: <a href=""{hostAndPortService.GetHostAndPortPlaceholder()}/records/987987987"">Some record</a>";
        var rec2 = rec.Id;

        await context.SaveChangesAsync();

        return (rec1, rec2);
    }

    protected async Task<(Guid, Guid)> For2RecordsAddLinkToText(DiaryDbContext context, HostAndPortStub hostAndPortService)
    {
        int count = await context.Records.CountAsync();
        var rnd = new Random();

        int ind1 = rnd.Next(0, count / 2);
        var rec = (await context.Records.ToListAsync()).ElementAt(ind1);
        rec.Text += $@"Link: <a href=""{hostAndPortService.GetHostAndPortPlaceholder()}/images/123"">Some Image</a>";
        var rec1 = rec.Id;

        int ind2 = rnd.Next(count / 2, count - 1);
        rec = (await context.Records.ToListAsync()).ElementAt(ind2);
        rec.Text += $@"Link: <a href=""{hostAndPortService.GetHostAndPortPlaceholder()}/records/987987987"">Some record</a>";
        var rec2 = rec.Id;

        await context.SaveChangesAsync();

        return (rec1, rec2);
    }

    protected async Task<(Guid, Guid)> For2RecordsAddLinkToCogitations(DiaryDbContext context, HostAndPortStub hostAndPortService)
    {
        int count = await context.Records.CountAsync();
        var rnd = new Random();

        int ind1 = rnd.Next(0, count / 2);
        var rec = (await context.Records.Include(r => r.Cogitations).ToListAsync()).ElementAt(ind1);
        rec.Cogitations.Skip(1).First().Text += $@"Link: <a href=""{hostAndPortService.GetHostAndPortPlaceholder()}/images/123"">Some Image</a>";
        var cog1 = rec.Cogitations.Skip(1).First().Id;

        int ind2 = rnd.Next(count / 2, count - 1);
        rec = (await context.Records.Include(r => r.Cogitations).ToListAsync()).ElementAt(ind2);
        rec.Cogitations.Skip(2).First().Text += $@"Link: <a href=""{hostAndPortService.GetHostAndPortPlaceholder()}/records/987987987"">Some record</a>";
        var cog2 = rec.Cogitations.Skip(2).First().Id;

        await context.SaveChangesAsync();

        return (cog1, cog2);
    }

    protected enum ThemesTestDataSet { ThemesOnly, DatesAndThemesRec, DatesAndThemesCount }

    protected (Guid themeId1, List<string> recordsNamesForTheme1, Guid themeId2, List<string> recordsNamesForTheme2) BindRecordsWithThemes(DiaryDbContext context, ThemesTestDataSet dataSetType)
    {
        var recThemes = (dataSetType) switch
        {
            ThemesTestDataSet.ThemesOnly => new Dictionary<string, List<string>>() {
            { "03", new List<string>() { "10" } },
            { "05", new List<string>() { "10", "20" } },
            { "11", new List<string>() { "10" } },
            { "12", new List<string>() { "10" } },
            { "17", new List<string>() { "20" } },
            { "19", new List<string>() { "10" } }
        },
            ThemesTestDataSet.DatesAndThemesRec => new Dictionary<string, List<string>>() {
            { "04", new List<string>() { "10" } },
            { "05", new List<string>() { "10", "20" } },
            { "07", new List<string>() { "10" } },
            { "09", new List<string>() { "10" } },
            { "10", new List<string>() { "10" } },
            { "17", new List<string>() { "20" } },
            { "19", new List<string>() { "10" } }
        },
            ThemesTestDataSet.DatesAndThemesCount => new Dictionary<string, List<string>>() {
            { "04", new List<string>() { "10" } },
            { "05", new List<string>() { "10", "20" } },
            { "07", new List<string>() { "10" } },
            { "08", new List<string>() { "20" } },
            { "10", new List<string>() { "10" } },
            { "11", new List<string>() { "10" } },
            { "19", new List<string>() { "10" } }
        },
            _ => throw new Exception()
        };

        BindRecordsWithThemes(context, recThemes);

        var themeId1 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 10).Id;
        var themeId2 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 20).Id;

        return (themeId1, new List<string> { "03", "05", "11", "12", "19" }, themeId2, new List<string> { "05", "17" });
    }

    protected string SearchSubstring4 => "SearchТекстІї*01";

    protected IEnumerable<string> SearchResult4 => GetNamesList().Where(s => s.ToUpper().Contains(SearchSubstring4.ToUpper()));

    protected IEnumerable<string> GetNamesList()
    {
        yield return "ghorgh";
        yield return "893472983 SearchТекстІї*01";
        yield return "_)+_)+_)JK";
        yield return "kjgh  afkgj lsfg g sjg";
        yield return "прлапрыл";
        yield return "іїхїхїхїх'їїхїхаїіхмава";
        yield return "fgsgsfgs";
        yield return "56";
        yield return "* /*SearchтекстІї*01/*";
        yield return "врлпываорпыра";
        yield return "_SearchТекстІї*011АРРОПРОлрффлвыа";
        yield return "РЛРОРЛолврфылваоф";
        yield return "жлажфлывлаДЛДЛО";
        yield return "321321230";
        yield return ",0,0,0,4уыы";
        yield return "название";
        yield return "фвафыа";
        yield return "№%№SearCHТекстІї*01%№!::!;%№:%; ";
        yield return "ывп  ыапыап   папы ап ыап ыа";
        yield return ".юб.б.юбс.б";
    }

    protected async Task<(Guid, Guid, List<string>)> CreateRecordsWithNamesAndThemes(DiaryDbContext context)
    {
        var namesList = new Dictionary<int, string> {
            { 0, "ghorgh"},
            { 1, "893472983 SearchТекстІї*01"},
            { 2, "_)+_)+_)JK"},
            { 3, "kjgh  afkgj lsfg g sjg"},
            { 4, "прлапрыл"},
            { 5, "іїхїхїхїх'їїхїхаїіхмава"},
            { 6, "fgsgsfgs"},
            { 7, "56SearchТекстІї*01"},
            { 8, "* /*SearchТекстІї*01/*"},//<--
            { 9, "врлпываорпыра"},
            { 10, "_SearchТекстІї*011АРРОПРОлрффлвыа"}, //<--
            { 11, "РЛРОРЛолврфылваоф SearchТекстІї*01"},
            { 12, "жлажфлывлаДЛДЛО"},
            { 13, "321321230"},
            { 14, ",0,0,0,4уыы"},
            { 15, "название"},
            { 16, "фвафыа"},
            { 17, "№%№SearchТекстІї*01%№!::!;%№:%; "},
            { 18, "ывп  ыапыап   папы ап ыап ыа"},
            { 19,  ".юб.б.юбс.б" } };

        Guid GetRecordIdByNameIndex(int index) => context.Records.First(r => r.Name == namesList[index]).Id;
        Create30ThemesAnd20Records(context, namesList.Values.ToArray(), GetDatesList(20));
        var themeId1 = context.Themes.ToList().ElementAt(10).Id;
        var themeId2 = context.Themes.ToList().ElementAt(22).Id;
        var recordsThemes = new List<DiaryRecordTheme> {
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(4),  ThemeId = themeId1},
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(7),  ThemeId = themeId1},
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(8),  ThemeId = themeId2},
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(10), ThemeId = themeId1},
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(11), ThemeId = themeId2},
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(17), ThemeId = themeId2},
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(19), ThemeId = themeId1},
            new DiaryRecordTheme {RecordId = GetRecordIdByNameIndex(7),  ThemeId = themeId2}
        };
        context.RecordThemes.AddRange(recordsThemes);
        await context.SaveChangesAsync();

        return (themeId1, themeId2, new List<string> { "* /*searchтекстІї*01/*", "_SearchТекстІї*011АРРОПРОлрффлвыа" });
    }

    protected async Task<(Guid rec1Id, Guid rec2Id, Guid rec3Id, Guid imgId)> CreateRecordsWithLinkedImage(DiaryDbContext context)
    {
        var rec1Id = CreateRecord(context);
        var rec2Id = CreateRecord(context);
        var rec3Id = CreateRecord(context);
        var imgId = CreateImage(context);

        context.RecordImages.Add(new DiaryRecordImage { ImageId = imgId, RecordId = rec1Id, Order = 1 });
        context.RecordImages.Add(new DiaryRecordImage { ImageId = imgId, RecordId = rec2Id, Order = 1 });
        context.RecordImages.Add(new DiaryRecordImage { ImageId = imgId, RecordId = rec3Id, Order = 1 });

        for (int i = 2; i <= 4; i++)
        {
            context.RecordImages.Add(new DiaryRecordImage { ImageId = CreateImage(context), RecordId = rec1Id, Order = i });
        }

        for (int i = 2; i <= 4; i++)
        {
            context.RecordImages.Add(new DiaryRecordImage { ImageId = CreateImage(context), RecordId = rec2Id, Order = i });
        }

        for (int i = 2; i <= 4; i++)
        {
            context.RecordImages.Add(new DiaryRecordImage { ImageId = CreateImage(context), RecordId = rec3Id, Order = i });
        }

        await context.SaveChangesAsync();

        return (rec1Id, rec2Id, rec3Id, imgId);
    }

    protected async Task<(Guid recordId, List<Guid> imagesIds, DiaryDbContext context)> CreateRecordAnd4Images()
    {
        var context = CreateContext();
        var imageSvc = GetImagesService(context);

        var imagesIds = new List<Guid>
        {
            await imageSvc.AddImage(TestFile, imageName: "Image 01"),
            await imageSvc.AddImage(TestFile, imageName: "Image 02"),
            await imageSvc.AddImage(TestFile, imageName: "Image 1"),
            await imageSvc.AddImage(TestFile, imageName: "Image 03")
        };

        var recordId = CreateRecord(context);

        return (recordId, imagesIds, context);
    }

    static protected ICalendarService GetCalendarService(DiaryDbContext? context = null) => TestedServices.GetCalendarService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected IDatesService GetDatesService(int daysRange, DiaryDbContext? context = null) => TestedServices.GetDatesService(context ?? CreateContext(), new AppSettingsForDatesServiceStub(daysRange));

    static protected IRecordsSearchTextService GetRecordsSearchTextService(DiaryDbContext? context = null) => TestedServices.GetRecordsSearchTextService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected IRecordsSearchService GetRecordsSearchService(DiaryDbContext? context = null) => TestedServices.GetRecordsSearchService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected IRecordsService GetRecordsService(DiaryDbContext? context = null) => TestedServices.GetRecordsService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected ICropImageService GetCropImageService(DiaryDbContext? context = null) => TestedServices.GetCropImageService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected IImagesEditService GetImagesEditService(DiaryDbContext? context = null) => TestedServices.GetImagesEditService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected IImagesService GetImagesService(DiaryDbContext? context = null) => TestedServices.GetImagesService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected IRecordsImagesService GetRecordsImagesService(DiaryDbContext? context = null) => TestedServices.GetRecordsImagesService(context ?? CreateContext());

    static protected IAppSettingsService GetAppSettingsService(DiaryDbContext? context = null) => TestedServices.GetAppSettingsService(context ?? CreateContext());

    static protected IRecordsThemesService GetRecordsThemesService(DiaryDbContext? context = null) => TestedServices.GetRecordsThemesService(context ?? CreateContext());

    static protected IScopesService GetScopesService(DiaryDbContext? context = null) => TestedServices.GetScopesService(context ?? CreateContext(), new AppSettingsServiceStub());

    static protected ICogitationsService GetCogitationsService(DiaryDbContext? context = null) => TestedServices.GetCogitationsService(context ?? CreateContext(), new AppSettingsServiceStub());
}
