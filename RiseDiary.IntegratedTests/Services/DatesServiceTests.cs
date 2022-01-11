using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.IntegratedTests.Stubs;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RiseDiary.IntegratedTests.Services
{
    [TestFixture]
    internal class DatesServiceTests : TestedServices
    {
        [Test]
        public async Task GetAllDates_ShouldReplacePlaceholdereByHostAndPort_InRecordName()
        {
            var context = CreateContext();
            await AddTestData(context);
            var datesService = GetDatesService(10, context);
            var hostAndPortService = new HostAndPortStub();

            var list = await datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

            var r1 = list[2];
            r1?.Name.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r1?.Name.Should().Contain(hostAndPortService.GetHostAndPort());
            var r2 = list[5];
            r2?.Name.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r2?.Name.Should().Contain(hostAndPortService.GetHostAndPort());
        }

        [Test]
        public async Task GetAllDates_ShouldReplacePlaceholdereByHostAndPort_InRecordText()
        {
            var context = CreateContext();
            await AddTestData(context);
            var datesService = GetDatesService(10, context);
            var hostAndPortService = new HostAndPortStub();

            var list = await datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

            var r1 = list[2];
            r1?.Text.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r1?.Text.Should().Contain(hostAndPortService.GetHostAndPort());
            var r2 = list[5];
            r2?.Text.Should().NotContain(hostAndPortService.GetHostAndPortPlaceholder());
            r2?.Text.Should().Contain(hostAndPortService.GetHostAndPort());
        }
        
        [Test]
        public async Task GetAllDates_ShouldReturnAllAssignedThemesNames()
        {
            var context = CreateContext();
            var (scopes, _) = await AddTestData(context);
            var datesService = GetDatesService(10, context);
            
            var list = await datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

            list[1].Themes.Should().ContainAll(
                scopes[0].Themes.ElementAt(0).ThemeName,
                scopes[0].Themes.ElementAt(1).ThemeName,
                scopes[0].Themes.ElementAt(2).ThemeName,
                scopes[1].Themes.ElementAt(2).ThemeName);
        }

        [Test]
        public async Task GetAllDates_ShouldReturnOrdered()
        {
            var context = CreateContext();
            await AddTestData(context);
            var datesService = GetDatesService(10, context);

            var list = await datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

            list.Should().HaveCount(8);
            list.Should().BeInAscendingOrder(item => item.TransferredDate);
        }
        
        [Test]
        public async Task SoftDeleting_GetAllDates_ShouldNotReturnDeletedRecords()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var (_, records) = await AddTestData(context);
            records[3].Deleted = true;
            await context.SaveChangesAsync();
            var datesService = GetDatesService(10, context);

            var list = await datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

            list.Should().HaveCount(7);
            list.Should().NotContain(item => item.Id == records[3].Id);
        }

        [Test]
        public async Task SoftDeleting_GetAllDates_ShouldNotReturnRecordsWithRemovedThemes()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var (scopes, records) = await AddTestData(context);
            var themeId = scopes[0].Themes.ElementAt(1).Id;
            var recordId = records[0].Id;
            var tr = await context.RecordThemes.SingleOrDefaultAsync(tr => tr.RecordId == recordId && tr.ThemeId == themeId);
            tr.Deleted = true;
            await context.SaveChangesAsync();
            var datesService = GetDatesService(10, context);

            var list = await datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow));

            list.Should().HaveCount(7);
            list.Should().NotContain(item => item.Id == records[0].Id);
        }

        [Test]
        public async Task GetDatesFromRange_ShouldReturn1Records_Then2Records()
        {
            var context = CreateContext();
            await AddTestData(context);
            var datesService = GetDatesService(5, context);

            var list = await datesService.GetDatesFromRange(new DateOnly(2020, 04, 28), false);

            list.Should().HaveCount(1);
            list[0].Date.Should().Be(DateOnly.Parse("2012-04-23"));

            //============================//

            list = await datesService.GetDatesFromRange(new DateOnly(2020, 06, 19), false);

            list.Should().HaveCount(2);
            var expected = new string[] { "2019-06-23", "2015-06-23" }.Select(t => DateOnly.Parse(t)).ToList();
            list.Select(i => i.Date).Should().OnlyContain(d => expected.Contains(d));
        }
        
        [Test]
        public async Task GetDatesFromRange_OnNewYear_ShouldReturn3Records()
        {
            var context = CreateContext();
            var (_, records) = await AddTestData(context);
            var datesService = GetDatesService(5, context);

            var list = await datesService.GetDatesFromRange(new DateOnly(2020, 01, 01), false);

            list.Should().HaveCount(2);
            list.Should().SatisfyRespectively(
                first => first.Date.Should().Be(DateOnly.Parse("2019-12-31")),
                second => second.Date.Should().Be(DateOnly.Parse("2018-01-01")));
        }

        [Test]
        public async Task GetDatesFromRange_WithEmptyDates_ShouldReturnRecordsForAllDaysFromRange()
        {
            var context = CreateContext();
            await AddTestData(context);
            var datesService = GetDatesService(5, context);

            var list = await datesService.GetDatesFromRange(new DateOnly(2020, 06, 19), true);

            list.Should().HaveCount(11);
        }
        

#pragma warning disable CA1822 // Mark members as static
        private List<DiaryRecord> GetTestRecords() => new()
        {
            new DiaryRecord //1
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2018-01-01"),
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            },
             new DiaryRecord // 0
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2016-02-10"),
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            },
             new DiaryRecord // 3
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2010-04-22"),
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            },
            new DiaryRecord // 1
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2012-04-23"),
                Name = Guid.NewGuid().ToString() + $@"Link: <a href=""{new HostAndPortStub().GetHostAndPortPlaceholder()}/images/123"">Some Image</a>",
                Text = Guid.NewGuid().ToString() + $@"Link: <a href=""{new HostAndPortStub().GetHostAndPortPlaceholder()}/images/123"">Some Image</a>"
            },
            new DiaryRecord // 0
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2008-06-19"), 
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            },
            new DiaryRecord // 2
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2019-06-23"),
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            },
            new DiaryRecord // 3
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2015-06-23"),
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            },
            new DiaryRecord // 3
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2020-06-24"),
                Name = Guid.NewGuid().ToString() + $@"Link: <a href=""{new HostAndPortStub().GetHostAndPortPlaceholder()}/records/987987987"">Some record</a>",
                Text = Guid.NewGuid().ToString() + $@"Link: <a href=""{new HostAndPortStub().GetHostAndPortPlaceholder()}/records/987987987"">Some record</a>"
            },
            new DiaryRecord // 3
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2005-09-17"),
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            },
            new DiaryRecord // 1
            {
                Id = Guid.NewGuid(),
                Date = DateOnly.Parse("2019-12-31"),
                Name = Guid.NewGuid().ToString(),
                Text = Guid.NewGuid().ToString()
            }            
        };

        private List<DiaryScope> GetTestScopes() => new()
        {
            new DiaryScope
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                ScopeName = "Dates Scope",
                Themes = new List<DiaryTheme>
                {
                    new DiaryTheme
                    {
                        Id = Guid.NewGuid(),
                        ThemeName = "Dates 1"
                    },
                    new DiaryTheme
                    {
                        Id = Guid.NewGuid(),
                        ThemeName = "Dates 2"
                    },
                    new DiaryTheme
                    {
                        Id = Guid.NewGuid(),
                        ThemeName = "Dates 3"
                    }
                }
            },
            new DiaryScope
            {
                Id = Guid.NewGuid(),
                ScopeName = "Some other scope",
                Themes = new List<DiaryTheme>
                {
                    new DiaryTheme
                    {
                        Id = Guid.NewGuid(),
                        ThemeName = "Other Theme 1"
                    },
                    new DiaryTheme
                    {
                        Id = Guid.NewGuid(),
                        ThemeName = "Other Theme 2"
                    },
                    new DiaryTheme
                    {
                        Id = Guid.NewGuid(),
                        ThemeName = "Other Theme 3"
                    }
                }
            }
        };

        private async Task<(List<DiaryScope> scopes, List<DiaryRecord> records)> AddTestData(DiaryDbContext context)
        {
            var scopes = GetTestScopes();
            var records = GetTestRecords();

            await context.Records.AddRangeAsync(records);
            await context.Scopes.AddRangeAsync(scopes);

            var rs = new List<DiaryRecordTheme>
            {
                new DiaryRecordTheme
                {
                    Record = records[0],
                    Theme = scopes[0].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[2],
                    Theme = scopes[0].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[2],
                    Theme = scopes[0].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[2],
                    Theme = scopes[0].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[3],
                    Theme = scopes[0].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[5],
                    Theme = scopes[0].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[5],
                    Theme = scopes[0].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[6],
                    Theme = scopes[0].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[6],
                    Theme = scopes[0].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[6],
                    Theme = scopes[0].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[7],
                    Theme = scopes[0].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[7],
                    Theme = scopes[0].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[7],
                    Theme = scopes[0].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[8],
                    Theme = scopes[0].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[8],
                    Theme = scopes[0].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[8],
                    Theme = scopes[0].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[9],
                    Theme = scopes[0].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[0],
                    Theme = scopes[1].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[1],
                    Theme = scopes[1].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[2],
                    Theme = scopes[1].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[4],
                    Theme = scopes[1].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[5],
                    Theme = scopes[1].Themes.ElementAt(1)
                },
                new DiaryRecordTheme
                {
                    Record = records[6],
                    Theme = scopes[1].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[7],
                    Theme = scopes[1].Themes.ElementAt(0)
                },
                new DiaryRecordTheme
                {
                    Record = records[8],
                    Theme = scopes[1].Themes.ElementAt(2)
                },
                new DiaryRecordTheme
                {
                    Record = records[9],
                    Theme = scopes[1].Themes.ElementAt(1)
                }
            };

            await context.RecordThemes.AddRangeAsync(rs);

            await context.SaveChangesAsync();

            return (scopes, records);
        }
    }
}
