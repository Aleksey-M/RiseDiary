using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RiseDiary.Data.SqliteStorages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MigrateFromDapperToEF
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string sourceFile = @"F:\Dev\VisualStudio\RiseDiary\MigrateFromDapperToEF\bin\Debug\netcoreapp2.1\DB\RiseDiaryData.db";
            string destFile = @"F:\Dev\VisualStudio\RiseDiary\MigrateFromDapperToEF\bin\Debug\netcoreapp2.1\DB\EF_RiseDiaryData.db";
            if (File.Exists(destFile)) { File.Delete(destFile); }

            if (!File.Exists(sourceFile)) { Console.WriteLine("Source file is not found!"); return; }

            var reposFactory = new RepositoriesFactory(Path.GetDirectoryName(sourceFile), Path.GetFileName(sourceFile));
            //
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .Build();

            var builder = new DbContextOptionsBuilder<DiaryDbContext>();
            builder.UseSqlite($"Data Source={destFile};");

            var context = new DiaryDbContext(builder.Options);
            context.Database.EnsureCreated();
            //
            var areas = await reposFactory.AreasRepository.FetchAllAreas();
            Console.WriteLine($"Areas count - {areas.Count}");
            context.Scopes.AddRange(areas.Select(a => new DiaryScope { ScopeName = a.AreaName }));
            await context.SaveChangesAsync();
            Console.WriteLine("Work done!");
            //
            var themes = await reposFactory.RecordTypesRepository.FetchRecordTypesWithAreas();
            Console.WriteLine($"Themes Count - {themes.Count}");
            context.Themes.AddRange(themes.Select(t => new DiaryTheme { DiaryScopeId = context.Scopes.FirstOrDefault(s=>s.ScopeName == t.AreaName).Id, ThemeName = t.RecordTypeName }));
            await context.SaveChangesAsync();
            Console.WriteLine("Work done!");
            //
            var images = await reposFactory.DiaryImagesRepository.FetchImageSet(0, await reposFactory.DiaryImagesRepository.GetImagesCount());
            Console.WriteLine($"Images Count - {images.Count}");
            byte[] data;
            foreach (var img in images)
            {
                data = await reposFactory.DiaryImagesRepository.FetchImageDataById(img.ImageId);
                context.Images.Add(new DiaryImage
                {
                    CreateDate = img.CreateDate,
                    Data = data,
                    Name = img.ImageName
                });
            }
            await context.SaveChangesAsync();
            Console.WriteLine("Work done!");           
            //
            var filter = new RiseDiary.Domain.Repositories.RecordsFilter { PageSize = 1000000};
            filter.PageSize = await reposFactory.RecordsRepository.GetFilteredRecordsCount(filter);
            var records = await reposFactory.RecordsRepository.FetchRecordsListFiltered(filter);
            Console.WriteLine($"Records Count - {filter.PageSize}");

            foreach(var rec in records)
            {
                var newRecord = new DiaryRecord
                {
                    CreateDate = rec.RecordCreateDate,
                    Date = rec.RecordDate,
                    ModifyDate = rec.RecordModifyDate,
                    Name = rec.RecordName,
                    Text = rec.RecordText
                };
                context.Records.Add(newRecord);
                await context.SaveChangesAsync();
                //
                var cogitations = await reposFactory.CogitationRepository.FetchAllCogitationsOfRecord(rec.RecordId);
                context.Cogitations.AddRange(cogitations.Select(c => new Cogitation { Date = c.CogitationDate, RecordId = newRecord.Id, Text = c.Text }));
                await context.SaveChangesAsync();
                //
                var recThemes = await reposFactory.RecordTypesRepository.FetchTypesForRecord(rec.RecordId);
                context.RecordThemes.AddRange(recThemes.Select(t => new DiaryRecordTheme { RecordId = newRecord.Id, ThemeId = context.Themes.First(th => th.ThemeName == t.RecordTypeName).Id }));
                await context.SaveChangesAsync();
                //
                var recImages = await reposFactory.DiaryImagesRepository.FetchImagesForRecord(rec.RecordId);
                context.RecordImages.AddRange(recImages.Select(ri => new DiaryRecordImage { RecordId = newRecord.Id, ImageId = context.Images.First(i => i.Name == ri.ImageName).Id }));
                await context.SaveChangesAsync();
            }

            Console.WriteLine("Work done!");
            //
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
