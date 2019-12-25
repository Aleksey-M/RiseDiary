using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1812
    [TestFixture]
    internal class RecordTests : TestFixtureBase
    {
        [Test]
        public async Task AddDiaryRecord_ShouldReturnUniqueIdForEveryRecord()
        {
            var context = CreateContext();
            var rec = GetTestRecord();

            var i = await context.AddRecord(rec, "host");
            rec.Id = Guid.NewGuid();
            var j = await context.AddRecord(rec, "host");

            Assert.AreNotEqual(i, j);
        }

        [Test]
        public async Task GetRecord_ShouldReturnSavedRecord()
        {
            var context = CreateContext();
            var rec = GetTestRecord();

            var id = await context.AddRecord(rec, "host");
            var loadedRec = await context.FetchRecordById(id, "host");

            Assert.NotNull(loadedRec);
            Assert.AreEqual(rec.CreateDate, loadedRec?.CreateDate);
            Assert.AreEqual(rec.Date, loadedRec?.Date);
            Assert.AreEqual(rec.ModifyDate, loadedRec?.ModifyDate);
            Assert.AreEqual(rec.Name, loadedRec?.Name);
            Assert.AreEqual(rec.Text, loadedRec?.Text);
        }

        [Test]
        public async Task DeleteRecord_ShouldDeleteRecord()
        {
            var context = CreateContext();
            var rec = GetTestRecord();

            var id = await context.AddRecord(rec, "host");
            var loadedRec = await context.FetchRecordById(id, "host");
            if (loadedRec == null) Assert.Ignore("Error saving diary records");
            await context.DeleteRecord(id);
            loadedRec = await context.FetchRecordById(id, "host");

            Assert.IsNull(loadedRec);

            Assert.IsNull(context.Records.FirstOrDefault(r => r.Id == id));
        }

        [Test]
        public async Task UpdateRecord_ShouldUpdateFields()
        {
            var context = CreateContext();
            DiaryRecord? rec = GetTestRecord();
            var updatedRec = new DiaryRecord
            {
                CreateDate = DateTime.Now.AddDays(-5),
                ModifyDate = DateTime.Now.AddDays(-3),
                Date = DateTime.Now.AddDays(-4),
                Name = "!@#$&))* /-/-* /яяъъъыыіііїїїїххїїїйїхїцйхц",
                Text = "Инфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты AS"
            };

            updatedRec.Id = rec.Id = await context.AddRecord(rec, "host");
            await context.UpdateRecord(updatedRec, "host");
            rec = await context.FetchRecordById(updatedRec?.Id ?? Guid.Empty, "host");

            Assert.NotNull(rec);
            Assert.AreEqual(updatedRec?.CreateDate, rec?.CreateDate);
            Assert.AreEqual(updatedRec?.Date, rec?.Date);
            Assert.AreEqual(updatedRec?.ModifyDate, rec?.ModifyDate);
            Assert.AreEqual(updatedRec?.Name, rec?.Name);
            Assert.AreEqual(updatedRec?.Text, rec?.Text);
        }

        private class DiaryRecordNameAndIdComparer : IEqualityComparer<DiaryRecord>
        {
            public bool Equals(DiaryRecord x, DiaryRecord y) => x.Id == y.Id && x.Name == y.Name;
            public int GetHashCode(DiaryRecord obj) => obj.Id.GetHashCode() + obj.Name.GetHashCode(StringComparison.InvariantCulture);
            public static DiaryRecordNameAndIdComparer Instance => new DiaryRecordNameAndIdComparer();
        }

        [Test]
        public async Task FetchRecordsListFiltered_WithEmptyFilterCriterias_ShouldReturnAllPageRecords()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));

            var pageList = await context.FetchRecordsListFiltered(RecordsFilter.Empty, "host");

            Assert.AreEqual(20, pageList.Count);
            Assert.True(pageList.All(rec => context.Records.ToList().Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
        }

        [Test]
        public async Task GetFilteredRecordsCount_WithEmptyFilterCriterias_ShouldReturnAllRecordsCount()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));

            int recCount = await context.GetFilteredRecordsCount(RecordsFilter.Empty);

            Assert.AreEqual(context.Records.Count(), recCount);
        }

        [Test]
        public async Task FetchRecordsListFiltered_WithWhiteSpaceInNameFilter_ShouldReturnAllPageRecords()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));
            var filterWithWhiteSpace = new RecordsFilter { RecordNameFilter = "       " };
            var filterWithNull = new RecordsFilter { RecordNameFilter = null };
            var filterWithRmRf = new RecordsFilter { RecordNameFilter = "\r\r\n\r\n  \r  \n\n " };

            var pageList1 = await context.FetchRecordsListFiltered(filterWithWhiteSpace, "host");
            var pageList2 = await context.FetchRecordsListFiltered(filterWithNull, "host");
            var pageList3 = await context.FetchRecordsListFiltered(filterWithRmRf, "host");

            Assert.AreEqual(context.Records.Count(), pageList1.Count);
            Assert.True(pageList1.All(rec => context.Records.ToList().Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
            Assert.AreEqual(context.Records.Count(), pageList2.Count);
            Assert.True(pageList2.All(rec => context.Records.ToList().Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
            Assert.AreEqual(context.Records.Count(), pageList3.Count);
            Assert.True(pageList3.All(rec => context.Records.ToList().Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
        }

        [Test]
        public async Task GetFilteredRecordsCount_WithWhiteSpaceInNameFilter_ShouldReturnAllRecordsCount()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));
            var filterWithWhiteSpace = new RecordsFilter { RecordNameFilter = "       " };
            var filterWithNull = new RecordsFilter { RecordNameFilter = null };
            var filterWithRmRf = new RecordsFilter { RecordNameFilter = "\r\r\n\r\n  \r  \n\n " };

            int recCount1 = await context.GetFilteredRecordsCount(filterWithWhiteSpace);
            int recCount2 = await context.GetFilteredRecordsCount(filterWithNull);
            int recCount3 = await context.GetFilteredRecordsCount(filterWithRmRf);

            Assert.AreEqual(context.Records.Count(), recCount1);
            Assert.AreEqual(context.Records.Count(), recCount2);
            Assert.AreEqual(context.Records.Count(), recCount3);
        }

        [Test]
        public async Task FetchRecordsListFiltered_WithNameFilter_ShouldReturnAllMatches()
        {
            string searchName = "SearchТекстІї*01";
            const int matches = 4;
            var namesList = new[] {
                "ghorgh",
                "893472983 SearchТекстІї*01",
                "_)+_)+_)JK",
                "kjgh  afkgj lsfg g sjg",
                "прлапрыл",
                "іїхїхїхїх'їїхїхаїіхмава",
                "fgsgsfgs",
                "56",
                "* /*SearchТекстІї*01/*",
                "врлпываорпыра",
                "_SearchТекстІї*011АРРОПРОлрффлвыа",
                "РЛРОРЛолврфылваоф",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var context = CreateContext();
            Create_20Records(context, namesList, GetDatesList(20));
            var filters = new RecordsFilter { RecordNameFilter = searchName };

            var resPage = await context.FetchRecordsListFiltered(filters, "host");

            Assert.AreEqual(matches, resPage.Count);
            Assert.True(resPage.All(rec => rec.Name.IndexOf(searchName, StringComparison.InvariantCultureIgnoreCase) != -1));
        }

        [Test]
        public async Task GetFilteredRecordsCount_WithNameFilter_ShouldReturnAllMathchesCount()
        {
            string searchName = "SearchТекстІї*01";
            const int matches = 4;
            var namesList = new[] {
                "ghorgh",
                "893472983 SearchТекстІї*01",
                "_)+_)+_)JK",
                "kjgh  afkgj lsfg g sjg",
                "прлапрыл",
                "іїхїхїхїх'їїхїхаїіхмава",
                "fgsgsfgs",
                "56",
                "* /*SearchТекстІї*01/*",
                "врлпываорпыра",
                "_SearchТекстІї*011АРРОПРОлрффлвыа",
                "РЛРОРЛолврфылваоф",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var context = CreateContext();
            Create_20Records(context, namesList, GetDatesList(20));
            var filters = new RecordsFilter { RecordNameFilter = searchName };

            int matchesFound = await context.GetFilteredRecordsCount(filters);

            Assert.AreEqual(matches, matchesFound);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ByDate_ForBeforeLastWeek_ShouldReturn13Records()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));
            var dateTo = DateTime.Now.AddDays(-8);
            var filters = new RecordsFilter { RecordDateTo = dateTo };

            var resList = await context.FetchRecordsListFiltered(filters, "host");

            Assert.True(resList.All(rec => rec.Date < dateTo));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ByDate_ForBeforeLastWeek_ShouldReturn13()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));
            var dateTo = DateTime.Now.AddDays(-7);
            int testCount = 20 - 6; /* 20 days except last week (Today date is not added)*/
            var filters = new RecordsFilter { RecordDateTo = dateTo };

            int count = await context.GetFilteredRecordsCount(filters);

            Assert.AreEqual(testCount, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ByDate_ForLastWeek_ShouldReturn6Records()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));
            var dateFrom = DateTime.Now.AddDays(-7);
            var filters = new RecordsFilter { RecordDateFrom = dateFrom };

            var resList = await context.FetchRecordsListFiltered(filters, "host");

            Assert.True(resList.All(rec => rec.Date >= dateFrom.Date), "Not All Records have correct date");
        }

        [Test]
        public async Task GetFilteredRecordsCount_ByDate_ForLastWeek_ShouldReturn7()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesList(20));
            var dateFrom = DateTime.Now.AddDays(-6);
            int testCount = 6; /* all records for last week, except today*/
            var filters = new RecordsFilter { RecordDateFrom = dateFrom };

            int count = await context.GetFilteredRecordsCount(filters);

            Assert.AreEqual(testCount, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ByDate_ForConcreteDate_ShouldReturnWeekAgoRecords()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var concreteDate = DateTime.Now.AddDays(-7);
            var filters = new RecordsFilter { RecordDateFrom = concreteDate, RecordDateTo = concreteDate };

            var resList = await context.FetchRecordsListFiltered(filters, "host");

            Assert.IsNotEmpty(resList);
            Assert.True(resList.All(rec => rec.Date.Year == concreteDate.Year && rec.Date.Day == concreteDate.Day));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ByDate_ForConcreteDate_ShouldReturn2()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var concreteDate = DateTime.Now.AddDays(-7);
            int testCount = 2;
            var filters = new RecordsFilter { RecordDateFrom = concreteDate, RecordDateTo = concreteDate };

            int count = await context.GetFilteredRecordsCount(filters);

            Assert.AreEqual(testCount, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ByDateAndName_ShouldReturn2Records()
        {
            string searchName = "SearchТекстІї*01";
            var dateFrom = DateTime.Now.AddDays(-14).Date;
            var dateTo = DateTime.Now.AddDays(-9).Date;
            var namesList = new[] {
                "ghorgh",
                "893472983 SearchТекстІї*01",
                "_)+_)+_)JK",
                "kjgh  afkgj lsfg g sjg",
                "прлапрыл",
                "іїхїхїхїх'їїхїхаїіхмава",
                "fgsgsfgs",
                "56",
                "* /*SearchТекстІї*01/*",//<--
                "врлпываорпыра",
                "_searchТекстІї*011АРРОПРОлрффлвыа", //<--
                "РЛРОРЛолврфылваоф",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var context = CreateContext();
            Create_20Records(context, namesList, GetDatesList(20));
            var filters = new RecordsFilter
            {
                RecordNameFilter = searchName,
                RecordDateFrom = dateFrom,
                RecordDateTo = dateTo
            };

            var resList = await context.FetchRecordsListFiltered(filters, "host");

            Assert.IsTrue(resList.All(rec =>
                rec.Name.IndexOf(searchName, StringComparison.InvariantCultureIgnoreCase) != -1 &&
                rec.Date >= dateFrom &&
                rec.Date <= dateTo));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ByDateAndName_ShouldReturn2()
        {
            string searchName = "SearchТекстІї*01";
            const int matchesInRange = 2;
            var dateFrom = DateTime.Now.AddDays(-14).Date;
            var dateTo = DateTime.Now.AddDays(-9).Date;
            var namesList = new[] {
                "ghorgh",
                "893472983 SearchТекстІї*01",
                "_)+_)+_)JK",
                "kjgh  afkgj lsfg g sjg",
                "прлапрыл",
                "іїхїхїхїх'їїхїхаїіхмава",
                "fgsgsfgs",
                "56",
                "* /*SearchТекстІї*01/*",//<--
                "врлпываорпыра",
                "_SearchТекстІї*011АРРОПРОлрффлвыа", //<--
                "РЛРОРЛолврфылваоф",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var context = CreateContext();
            Create_20Records(context, namesList, GetDatesList(20));
            var filters = new RecordsFilter
            {
                RecordNameFilter = searchName,
                RecordDateFrom = dateFrom,
                RecordDateTo = dateTo
            };

            int count = await context.GetFilteredRecordsCount(filters);

            Assert.AreEqual(matchesInRange, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_WithNotExistingThemeId_ShouldReturnEmptyList()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var filter = new RecordsFilter();
            filter.AddThemeId(Guid.NewGuid());

            var list = await context.FetchRecordsListFiltered(filter, "");

            Assert.IsEmpty(list);
        }

        [Test]
        public async Task GetFilteredRecordsCount_WithNotExistingThemeId_ShouldReturn0()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var filter = new RecordsFilter();
            filter.AddThemeId(Guid.NewGuid());

            int count = await context.GetFilteredRecordsCount(filter);

            Assert.Zero(count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ForExistingThemeFilter_ShouldReturnAllMatches()
        {
            var recThemes = new Dictionary<string, List<string>>() {
                { "03", new List<string>() { "10" } },
                { "05", new List<string>() { "10", "20" } },
                { "11", new List<string>() { "10" } },
                { "12", new List<string>() { "10" } },
                { "13", new List<string>() { "10" } },
                { "17", new List<string>() { "20" } },
                { "19", new List<string>() { "10" } }
            };
            var context = CreateContext();
            Create_30Themes_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var ThemeId1 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 10).Id;
            var ThemeId2 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 20).Id;
            BindRecordsWithThemes(context, recThemes);
            var filter = new RecordsFilter();

            filter.AddThemeId(ThemeId1);
            var result = await context.FetchRecordsListFiltered(filter, "host");

            Assert.IsNotEmpty(result);
            Assert.IsTrue(HasRecordWithIntName(result, 3));
            Assert.IsTrue(HasRecordWithIntName(result, 5));
            Assert.IsTrue(HasRecordWithIntName(result, 11));
            Assert.IsTrue(HasRecordWithIntName(result, 12));
            Assert.IsTrue(HasRecordWithIntName(result, 19));

            filter.AddThemeId(ThemeId2);
            filter.RemoveThemeId(ThemeId1);
            result = await context.FetchRecordsListFiltered(filter, "host");

            Assert.IsNotEmpty(result);
            Assert.IsTrue(HasRecordWithIntName(result, 5));
            Assert.IsTrue(HasRecordWithIntName(result, 17));

            filter.AddThemeId(ThemeId1);
            result = await context.FetchRecordsListFiltered(filter, "host");

            Assert.IsNotEmpty(result);
            Assert.IsTrue(HasRecordWithIntName(result, 5));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ForExistingThemeFilter_ShouldReturnAllMatchesCount()
        {
            var recThemes = new Dictionary<string, List<string>>() {
                { "03", new List<string>() { "10" } },
                { "05", new List<string>() { "10", "20" } },
                { "11", new List<string>() { "10" } },
                { "12", new List<string>() { "10" } },
                { "17", new List<string>() { "20" } },
                { "19", new List<string>() { "10" } }
            };
            var context = CreateContext();
            Create_30Themes_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var ThemeId1 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 10).Id;
            var ThemeId2 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 20).Id;
            BindRecordsWithThemes(context, recThemes);
            var filter = new RecordsFilter();
            int matchesCount1 = 5;
            int matchesCount2 = 2;
            int matchesCountAll = 1;

            filter.AddThemeId(ThemeId1);
            int count = await context.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matchesCount1, count);

            filter.AddThemeId(ThemeId2);
            filter.RemoveThemeId(ThemeId1);
            count = await context.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matchesCount2, count);

            filter.AddThemeId(ThemeId1);
            count = await context.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matchesCountAll, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ForDateAndTheme()
        {
            var recThemes = new Dictionary<string, List<string>>() {
                { "04", new List<string>() { "10" } },
                { "05", new List<string>() { "10", "20" } },
                { "07", new List<string>() { "10" } },
                { "09", new List<string>() { "10" } },
                { "10", new List<string>() { "10" } },
                { "17", new List<string>() { "20" } },
                { "19", new List<string>() { "10" } }
            };
            var context = CreateContext();
            Create_30Themes_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var ThemeId1 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 10).Id;
            var ThemeId2 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 20).Id;
            BindRecordsWithThemes(context, recThemes);
            var dateFrom = DateTime.Now.AddDays(-10).Date;
            var dateTo = DateTime.Now.AddDays(-5).Date;
            var filter = new RecordsFilter { RecordDateFrom = dateFrom, RecordDateTo = dateTo };
            filter.AddThemeId(ThemeId1);

            var result = await context.FetchRecordsListFiltered(filter, "host");

            Assert.IsNotEmpty(result);
            Assert.True(result.TrueForAll(rec => rec.Date >= dateFrom && rec.Date <= dateTo), "Dates of records is not correct");
            Assert.IsTrue(HasRecordWithIntName(result, 5));
            Assert.IsTrue(HasRecordWithIntName(result, 7));
            Assert.IsTrue(HasRecordWithIntName(result, 9));

            filter.AddThemeId(ThemeId2);
            filter.RemoveThemeId(ThemeId1);

            result = await context.FetchRecordsListFiltered(filter, "host");

            Assert.IsTrue(HasRecordWithIntName(result, 5));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ForDateAndTheme()
        {
            var recThemes = new Dictionary<string, List<string>>() {
                { "04", new List<string>() { "10" } },
                { "05", new List<string>() { "10", "20" } },
                { "07", new List<string>() { "10" } },
                { "08", new List<string>() { "20" } },
                { "10", new List<string>() { "10" } },
                { "11", new List<string>() { "10" } },
                { "19", new List<string>() { "10" } }
            };
            var context = CreateContext();
            Create_30Themes_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var ThemeId1 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 10).Id;
            var ThemeId2 = context.Themes.ToList().First(t => int.Parse(t.ThemeName, CultureInfo.InvariantCulture) == 20).Id;
            BindRecordsWithThemes(context, recThemes);
            var dateFrom = DateTime.Now.AddDays(-10).Date;
            var dateTo = DateTime.Now.AddDays(-5).Date;
            var filter = new RecordsFilter { RecordDateFrom = dateFrom, RecordDateTo = dateTo };
            filter.AddThemeId(ThemeId1);
            int expectedMatches = 3;

            int count = await context.GetFilteredRecordsCount(filter);

            Assert.AreEqual(expectedMatches, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ForNameAndTheme()
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
            var context = CreateContext();
            Guid GetRecordIdByNameIndex(int index) => context.Records.First(r => r.Name == namesList[index]).Id;

            Create_30Themes_20Records(context, namesList.Values.ToArray(), GetDatesList(20));
            var themeId1 = context.Themes.ToList().ElementAt(10).Id;
            var themeId2 = context.Themes.ToList().ElementAt(22).Id;

            await context.AddRecordTheme(GetRecordIdByNameIndex(4), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(7), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(8), themeId2);
            await context.AddRecordTheme(GetRecordIdByNameIndex(10), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(11), themeId2);
            await context.AddRecordTheme(GetRecordIdByNameIndex(17), themeId2);
            await context.AddRecordTheme(GetRecordIdByNameIndex(19), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(7), themeId2);
            string searchName = "SearchТекстІї*01";


            var filter = new RecordsFilter { RecordNameFilter = searchName };
            filter.AddThemeId(themeId1);

            var result = await context.FetchRecordsListFiltered(filter, "host");


            Assert.IsNotEmpty(result);
            Assert.True(result[0].Name.IndexOf(searchName, StringComparison.CurrentCultureIgnoreCase) != -1);
            Assert.True(result[1].Name.IndexOf(searchName, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        [Test]
        public async Task GetFilteredRecordsCount_ForNameAndTheme()
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
            var context = CreateContext();
            Guid GetRecordIdByNameIndex(int index) => context.Records.First(r => r.Name == namesList[index]).Id;

            Create_30Themes_20Records(context, namesList.Values.ToArray(), GetDatesList(20));
            var themeId1 = context.Themes.ToList().ElementAt(10).Id;
            var themeId2 = context.Themes.ToList().ElementAt(22).Id;

            await context.AddRecordTheme(GetRecordIdByNameIndex(4), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(7), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(8), themeId2);
            await context.AddRecordTheme(GetRecordIdByNameIndex(10), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(11), themeId2);
            await context.AddRecordTheme(GetRecordIdByNameIndex(17), themeId2);
            await context.AddRecordTheme(GetRecordIdByNameIndex(19), themeId1);
            await context.AddRecordTheme(GetRecordIdByNameIndex(7), themeId2);
            string searchName = "SearchТекстІї*01";
            const int matchesInRange = 2;


            var filter = new RecordsFilter { RecordNameFilter = searchName };
            filter.AddThemeId(themeId1);

            int count = await context.GetFilteredRecordsCount(filter);


            Assert.AreEqual(matchesInRange, count);
        }

        [Test]
        public async Task CheckEntityFrameworkQuert()
        {
            var context = CreateContext();
            Create_20Records(context, GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var filter = new RecordsFilter();
            filter.AddThemeId(Guid.NewGuid());

            IQueryable<DiaryRecord> result;
            List<DiaryRecord>? r = null;

            if (!filter.IsEmptyTypeFilter)
            {
                var temp = context.RecordThemes
                    .Where(rt => filter.RecordThemeIds.Contains(rt.ThemeId))
                    .Select(r => new { r.RecordId, r.ThemeId })
                    .ToList()
                    .GroupBy(r => r.RecordId)
                    .Where(g => filter.RecordThemeIds.All(id => g.Select(r => r.ThemeId).Contains(id)))
                    .Select(g => g.Key);

                result = context.Records.Where(r => temp.Contains(r.Id));
            }
            else
            {
                result = context.Records.AsQueryable();
            }

            if (!RecordsFilter.IsEmpty(filter))
            {
                if (!string.IsNullOrWhiteSpace(filter.RecordNameFilter))
                {
                    result = result.Where(r => r.Name.Contains(filter.RecordNameFilter, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.RecordDateFrom != null)
                {
                    result = result.Where(r => r.Date >= filter.RecordDateFrom);
                }
                if (filter.RecordDateTo != null)
                {
                    result = result.Where(r => r.Date <= filter.RecordDateTo);
                }
                //if (!filter.IsEmptyTypeFilter)
                //{
                //    r = (await result.ToListAsync())
                //        .Where(r => filter.RecordThemeIds.All(id => context.RecordThemes.ToList().Where(rt => rt.RecordId == r.Id).Select(rt => rt.ThemeId).Contains(id)))
                //        .ToList();
                //}
            }

            var list = r ?? await result.ToListAsync();

            Assert.IsEmpty(list);
        }
    }
}
