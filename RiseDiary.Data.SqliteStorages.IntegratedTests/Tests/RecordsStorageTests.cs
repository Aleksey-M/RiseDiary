using NUnit.Framework;
using RiseDiary.Domain.Model;
using RiseDiary.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RiseDiary.Data.SqliteStorages.IntegratedTests.TestsHelper;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    [TestFixture]
    class RecordsStorageTests : CleanUpTestFixtureBase
    {
        [Test]
        public async Task AddDiaryRecord_ShouldReturnUniqueIdForEveryRecord()
        {
            var recStor = new RecordsRepository(GetClearBase());
            var rec = GetTestRecord();

            int i = await recStor.AddRecord(rec);
            int j = await recStor.AddRecord(rec);

            Assert.AreNotEqual(i, j);
        }

        [Test]
        public async Task GetRecord_ShouldReturnSavedRecord()
        {
            var recStor = new RecordsRepository(GetClearBase());
            var rec = GetTestRecord();

            int id = await recStor.AddRecord(rec);
            var loadedRec = await recStor.FetchRecordById(id);

            Assert.NotNull(loadedRec);
            Assert.AreEqual(rec.RecordCreateDate, loadedRec.RecordCreateDate);
            Assert.AreEqual(rec.RecordDate, loadedRec.RecordDate);
            Assert.AreEqual(rec.RecordModifyDate, loadedRec.RecordModifyDate);
            Assert.AreEqual(rec.RecordName, loadedRec.RecordName);
            Assert.AreEqual(rec.RecordText, loadedRec.RecordText);
        }

        [Test]
        public async Task DeleteRecord_ShouldDeleteRecord()
        {
            var recStor = new RecordsRepository(GetClearBase());
            var rec = GetTestRecord();

            int id = await recStor.AddRecord(rec);
            var loadedRec = await recStor.FetchRecordById(id);
            if (loadedRec == null) Assert.Ignore("Error saving diary records");
            await recStor.DeleteRecord(id);
            loadedRec = await recStor.FetchRecordById(id);

            Assert.IsNull(loadedRec);
        }

        [Test]
        public async Task UpdateRecord_ShouldUpdateFields()
        {
            var recStor = new RecordsRepository(GetClearBase());
            var rec = GetTestRecord();
            var updatedRec = new DiaryRecord
            {
                RecordCreateDate = DateTime.Now.AddDays(-5),
                RecordModifyDate = DateTime.Now.AddDays(-3),
                RecordDate = DateTime.Now.AddDays(-4),
                RecordName = "!@#$&))*/-/-*/яяъъъыыіііїїїїххїїїйїхїцйхц",
                RecordText = "Инфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты AS"
            };

            updatedRec.RecordId = rec.RecordId = await recStor.AddRecord(rec);
            await recStor.UpdateRecord(updatedRec);
            rec = await recStor.FetchRecordById(updatedRec.RecordId);

            Assert.NotNull(rec);
            Assert.AreEqual(updatedRec.RecordCreateDate, rec.RecordCreateDate);
            Assert.AreEqual(updatedRec.RecordDate, rec.RecordDate);
            Assert.AreEqual(updatedRec.RecordModifyDate, rec.RecordModifyDate);
            Assert.AreEqual(updatedRec.RecordName, rec.RecordName);
            Assert.AreEqual(updatedRec.RecordText, rec.RecordText);
        }

        [Test]
        public async Task FetchRecordsByMonth_WithourMonth_ShouldReturn3RecordForYear()
        {
            var recRep = new RecordsRepository(GetClearBase());
            for (int i = -5; i < 5; i++)
                for (int j = 1; j < 10; j = j + 3)
                {
                    await recRep.AddRecord(GetTestRecord(i, j));
                }
            int year = DateTime.Now.Year - 2;

            var list = await recRep.FetchRecordsByMonth(year);

            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list.All(rec => rec.RecordDate.Year == year));
        }

        [Test]
        public async Task FetchRecordsByMonth_ForMarch_ShouldReturn1Record()
        {
            var recRep = new RecordsRepository(GetClearBase());
            for (int i = -5; i < 5; i++)
                for (int j = 3; j < 10; j = j + 3)
                {
                    await recRep.AddRecord(GetTestRecord(i, j));
                }
            int year = DateTime.Now.Year - 2;

            var list = await recRep.FetchRecordsByMonth(year, 3);

            Assert.AreEqual(1, list.Count);
            Assert.IsTrue(list.All(rec => rec.RecordDate.Year == year && rec.RecordDate.Month == 3));
        }

        [Test]
        public async Task GetMonthRecordsCount_WithourMonth_ShouldReturn3()
        {
            var recRep = new RecordsRepository(GetClearBase());
            for (int i = -5; i < 5; i++)
                for (int j = 1; j < 10; j = j + 3)
                {
                    await recRep.AddRecord(GetTestRecord(i, j));
                }
            int year = DateTime.Now.Year - 2;

            int recCount = await recRep.GetMonthRecordsCount(year);

            Assert.AreEqual(3, recCount);
        }

        [Test]
        public async Task GetMonthRecordsCount_ForMarch_ShouldReturn1()
        {
            var recRep = new RecordsRepository(GetClearBase());
            for (int i = -5; i < 5; i++)
                for (int j = 3; j < 10; j = j + 3)
                {
                    await recRep.AddRecord(GetTestRecord(i, j));
                }
            int year = DateTime.Now.Year - 2;

            int recCount = await recRep.GetMonthRecordsCount(year, 3);

            Assert.AreEqual(1, recCount);
        }

        [Test]
        public async Task GetRecordByCogitation_ShouldReturnRecord()
        {
            var testData = GetBaseWith3RecordsAndOneCogitation();
            var recRep = new RecordsRepository(testData.dbManager);

            var rec = await recRep.GetRecordByCogitation(testData.cogId);

            Assert.NotNull(rec);
            Assert.AreEqual(testData.recId, rec.RecordId);
        }

        private class DiaryRecordNameAndIdComparer : IEqualityComparer<DiaryRecord>
        {
            public bool Equals(DiaryRecord x, DiaryRecord y) => x.RecordId == y.RecordId && x.RecordName == y.RecordName;
            public int GetHashCode(DiaryRecord obj) => obj.RecordId.GetHashCode() + obj.RecordName.GetHashCode();
            public static DiaryRecordNameAndIdComparer Instance => new DiaryRecordNameAndIdComparer();
        }

        [Test]
        public async Task FetchRecordsListFiltered_WithEmptyFilterCriterias_ShouldReturnAllPageRecords()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);

            var pageList = await recRepos.FetchRecordsListFiltered(RecordsFilter.Empty);

            Assert.AreEqual(testData.addedRecords.Count(), pageList.Count);
            Assert.True(pageList.All(rec => testData.addedRecords.Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
        }

        [Test]
        public async Task GetFilteredRecordsCount_WithEmptyFilterCriterias_ShouldReturnAllRecordsCount()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);

            int recCount = await recRepos.GetFilteredRecordsCount(RecordsFilter.Empty);

            Assert.AreEqual(testData.addedRecords.Count(), recCount);
        }

        [Test]
        public async Task FetchRecordsListFiltered_WithWhiteSpaceInNameFilter_ShouldReturnAllPageRecords()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filterWithWhiteSpace = new RecordsFilter { RecordNameFilter = "       " };
            var filterWithNull = new RecordsFilter { RecordNameFilter = null };
            var filterWithRmRf = new RecordsFilter { RecordNameFilter = "\r\r\n\r\n  \r  \n\n " };

            var pageList1 = await recRepos.FetchRecordsListFiltered(filterWithWhiteSpace);
            var pageList2 = await recRepos.FetchRecordsListFiltered(filterWithNull);
            var pageList3 = await recRepos.FetchRecordsListFiltered(filterWithRmRf);

            Assert.AreEqual(testData.addedRecords.Count(), pageList1.Count);
            Assert.True(pageList1.All(rec => testData.addedRecords.Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
            Assert.AreEqual(testData.addedRecords.Count(), pageList2.Count);
            Assert.True(pageList2.All(rec => testData.addedRecords.Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
            Assert.AreEqual(testData.addedRecords.Count(), pageList3.Count);
            Assert.True(pageList3.All(rec => testData.addedRecords.Contains(rec, DiaryRecordNameAndIdComparer.Instance)));
        }

        [Test]
        public async Task GetFilteredRecordsCount_WithWhiteSpaceInNameFilter_ShouldReturnAllRecordsCount()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filterWithWhiteSpace = new RecordsFilter { RecordNameFilter = "       " };
            var filterWithNull = new RecordsFilter { RecordNameFilter = null };
            var filterWithRmRf = new RecordsFilter { RecordNameFilter = "\r\r\n\r\n  \r  \n\n " };

            int recCount1 = await recRepos.GetFilteredRecordsCount(filterWithWhiteSpace);
            int recCount2 = await recRepos.GetFilteredRecordsCount(filterWithNull);
            int recCount3 = await recRepos.GetFilteredRecordsCount(filterWithRmRf);

            Assert.AreEqual(testData.addedRecords.Count(), recCount1);
            Assert.AreEqual(testData.addedRecords.Count(), recCount2);
            Assert.AreEqual(testData.addedRecords.Count(), recCount3);
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
                "*/*SearchТекстІї*01/*",
                "врлпываорпыра",
                "_searchТекстІї*011АРРОПРОлрффлвыа",
                "РЛРОРЛолврфылваоф",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var testData = GetBaseWith20Records(namesList, GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filters = new RecordsFilter { RecordNameFilter = searchName };

            var resPage = await recRepos.FetchRecordsListFiltered(filters);

            Assert.AreEqual(matches, resPage.Count);
            Assert.True(resPage.All(rec => rec.RecordName.IndexOf(searchName, StringComparison.InvariantCultureIgnoreCase) != -1));
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
                "*/*SearchТекстІї*01/*",
                "врлпываорпыра",
                "_searchТекстІї*011АРРОПРОлрффлвыа",
                "РЛРОРЛолврфылваоф",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var testData = GetBaseWith20Records(namesList, GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filters = new RecordsFilter { RecordNameFilter = searchName };

            int matchesFound = await recRepos.GetFilteredRecordsCount(filters);

            Assert.AreEqual(matches, matchesFound);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ByDate_ForBeforeLastWeek_ShouldReturn13Records()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var dateTo = DateTime.Now.AddDays(-8);
            var filters = new RecordsFilter { RecordDateTo = dateTo };

            var resList = await recRepos.FetchRecordsListFiltered(filters);

            Assert.True(resList.All(rec => rec.RecordDate < dateTo));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ByDate_ForBeforeLastWeek_ShouldReturn13()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var dateTo = DateTime.Now.AddDays(-7);
            int testCount = 20 - 6; /* 20 days except last week (Today date is not added)*/
            var filters = new RecordsFilter { RecordDateTo = dateTo };

            int count = await recRepos.GetFilteredRecordsCount(filters);

            Assert.AreEqual(testCount, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ByDate_ForLastWeek_ShouldReturn6Records()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var dateFrom = DateTime.Now.AddDays(-7);
            var filters = new RecordsFilter { RecordDateFrom = dateFrom };

            var resList = await recRepos.FetchRecordsListFiltered(filters);

            Assert.True(resList.All(rec => rec.RecordDate >= dateFrom.Date), "Not All Records have correct date");
        }

        [Test]
        public async Task GetFilteredRecordsCount_ByDate_ForLastWeek_ShouldReturn7()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var dateFrom = DateTime.Now.AddDays(-6);
            int testCount = 6; /* all records for last week, except today*/
            var filters = new RecordsFilter { RecordDateFrom = dateFrom };

            int count = await recRepos.GetFilteredRecordsCount(filters);

            Assert.AreEqual(testCount, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ByDate_ForConcreteDate_ShouldReturnWeekAgoRecords()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var concreteDate = DateTime.Now.AddDays(-7);
            var filters = new RecordsFilter { RecordDateFrom = concreteDate, RecordDateTo = concreteDate };

            var resList = await recRepos.FetchRecordsListFiltered(filters);

            Assert.IsNotEmpty(resList);
            Assert.True(resList.All(rec => rec.RecordDate.Year == concreteDate.Year && rec.RecordDate.Day == concreteDate.Day));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ByDate_ForConcreteDate_ShouldReturn2()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var concreteDate = DateTime.Now.AddDays(-7);
            int testCount = 2;
            var filters = new RecordsFilter { RecordDateFrom = concreteDate, RecordDateTo = concreteDate };

            int count = await recRepos.GetFilteredRecordsCount(filters);

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
                "*/*SearchТекстІї*01/*",//<--
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
            var testData = GetBaseWith20Records(namesList, GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filters = new RecordsFilter {
                RecordNameFilter = searchName,
                RecordDateFrom = dateFrom,
                RecordDateTo = dateTo };

            var resList = await recRepos.FetchRecordsListFiltered(filters);

            Assert.IsTrue(resList.All(rec =>
                rec.RecordName.IndexOf(searchName, StringComparison.InvariantCultureIgnoreCase) != -1 &&
                rec.RecordDate >= dateFrom &&
                rec.RecordDate <= dateTo));
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
                "*/*SearchТекстІї*01/*",//<--
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
            var testData = GetBaseWith20Records(namesList, GetDatesList(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filters = new RecordsFilter
            {
                RecordNameFilter = searchName,
                RecordDateFrom = dateFrom,
                RecordDateTo = dateTo
            };

            int count = await recRepos.GetFilteredRecordsCount(filters);

            Assert.AreEqual(matchesInRange, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_WithNotExistingTypeId_ShouldReturnEmptyList()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter();
            filter.AddRecordTypeId(11);

            var list = await recRepos.FetchRecordsListFiltered(filter);

            Assert.IsEmpty(list);
        }

        [Test]
        public async Task GetFilteredRecordsCount_WithNotExistingTypeId_ShouldReturn0()
        {
            var testData = GetBaseWith20Records(GetNumberList(20), GetDatesListWithTwoSameDatesWeekAgo(20));
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter();
            filter.AddRecordTypeId(11);

            int count = await recRepos.GetFilteredRecordsCount(filter);

            Assert.Zero(count);
        }

        [Test]        
        public async Task FetchRecordsListFiltered_ForExistingTypeFilter_ShouldReturnAllMatches()
        {
            var recTypes = Enumerable.Range(0, 20).Select(i => new int[0]).ToArray();
            int typeId1 = 10;
            int typeId2 = 22;
            recTypes[3] = new int[] { typeId1 };
            recTypes[5] = new int[] { typeId1, typeId2 };
            recTypes[11] = new int[] { typeId1 };
            recTypes[12] = new int[] { typeId1 };
            recTypes[13] = new int[] { typeId1 };
            recTypes[17] = new int[] { typeId2};
            recTypes[19] = new int[] { typeId1 };

            var recNames = GetNumberList(20);
            var testData = GetBaseWith20RecordsAndRecordTypes(recNames, GetDatesList(20), recTypes);
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter();

            filter.AddRecordTypeId(typeId1);
            var result = await recRepos.FetchRecordsListFiltered(filter);

            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(3)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(5)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(11)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(12)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(19)));

            filter.AddRecordTypeId(typeId2);
            filter.RemoveRecordTypeId(typeId1);
            result = await recRepos.FetchRecordsListFiltered(filter);

            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(5)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(17)));

            filter.AddRecordTypeId(typeId1);
            result = await recRepos.FetchRecordsListFiltered(filter);

            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(3)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(5)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(11)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(12)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(17)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(19)));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ForExistingTypeFilter_ShouldReturnAllMatchesCount()
        {
            var recTypes = Enumerable.Range(0, 20).Select(i => new int[0]).ToArray();
            int typeId1 = 10;
            int typeId2 = 22;
            recTypes[3] = new int[] { typeId1 };
            recTypes[5] = new int[] { typeId1, typeId2 };
            recTypes[11] = new int[] { typeId1 };
            recTypes[12] = new int[] { typeId1 };
            recTypes[17] = new int[] { typeId2 };
            recTypes[19] = new int[] { typeId1 };
            int matchesCount1 = 5;
            int matchesCount2 = 2;
            int matchesCountAll = 6;
            var testData = GetBaseWith20RecordsAndRecordTypes(GetNumberList(20), GetDatesList(20), recTypes);
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter();

            filter.AddRecordTypeId(typeId1);
            int count = await recRepos.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matchesCount1, count);

            filter.AddRecordTypeId(typeId2);
            filter.RemoveRecordTypeId(typeId1);
            count = await recRepos.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matchesCount2, count);

            filter.AddRecordTypeId(typeId1);
            count = await recRepos.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matchesCountAll, count);
        }

        [Test]
        public async Task FetchRecordsListFiltered_ForDateAndType()
        {
            var recTypes = Enumerable.Range(0, 20).Select(i => new int[0]).ToArray();
            int typeId1 = 10;
            int typeId2 = 22;
            recTypes[4] = new int[] { typeId1 };
            recTypes[5] = new int[] { typeId1, typeId2 };
            recTypes[7] = new int[] { typeId1 };
            recTypes[9] = new int[] { typeId1 };
            recTypes[10] = new int[] { typeId1 };
            recTypes[17] = new int[] { typeId2 };
            recTypes[19] = new int[] { typeId1 };

            var recNames = GetNumberList(20);
            var testData = GetBaseWith20RecordsAndRecordTypes(recNames, GetDatesList(20), recTypes);
            var dateFrom = DateTime.Now.AddDays(-10).Date;
            var dateTo = DateTime.Now.AddDays(-5).Date;
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter { RecordDateFrom = dateFrom, RecordDateTo = dateTo };
            filter.AddRecordTypeId(typeId1);

            var result = await recRepos.FetchRecordsListFiltered(filter);

            Assert.IsNotEmpty(result);
            Assert.True(result.TrueForAll(rec => rec.RecordDate >= dateFrom && rec.RecordDate <= dateTo), "Dates of records is not correct");
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(5)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(7)));
            Assert.IsTrue(result.Any(rec => rec.RecordName == recNames.ElementAt(9)));
        }

        [Test]
        public async Task GetFilteredRecordsCount_ForDateAndType()
        {
            var recTypes = Enumerable.Range(0, 20).Select(i => new int[0]).ToArray();
            int typeId1 = 10;
            int typeId2 = 22;
            recTypes[4] = new int[] { typeId1 };
            recTypes[5] = new int[] { typeId1, typeId2 };
            recTypes[7] = new int[] { typeId1 };
            recTypes[8] = new int[] { typeId2 };
            recTypes[10] = new int[] { typeId1 };
            recTypes[11] = new int[] { typeId1 };
            recTypes[19] = new int[] { typeId1 };
            int matches = 3;

            var recNames = GetNumberList(20);
            var testData = GetBaseWith20RecordsAndRecordTypes(recNames, GetDatesList(20), recTypes);
            var dateFrom = DateTime.Now.AddDays(-10).Date;
            var dateTo = DateTime.Now.AddDays(-5).Date;
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter { RecordDateFrom = dateFrom, RecordDateTo = dateTo };
            filter.AddRecordTypeId(typeId1);

            int count = await recRepos.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matches, count);
        }

        [Test]        
        public async Task FetchRecordsListFiltered_ForNameAndType()
        {
            var recTypes = Enumerable.Range(0, 20).Select(i => new int[0]).ToArray();
            int typeId1 = 10;
            int typeId2 = 22;
            recTypes[4] = new int[] { typeId1 };
            recTypes[7] = new int[] { typeId1, typeId2 };
            recTypes[8] = new int[] { typeId2 };
            recTypes[10] = new int[] { typeId1 };
            recTypes[11] = new int[] { typeId2 };
            recTypes[17] = new int[] { typeId2 };
            recTypes[19] = new int[] { typeId1 };
            string searchName = "SearchТекстІї*01";
            
            var namesList = new[] {
                "ghorgh",
                "893472983 SearchТекстІї*01",
                "_)+_)+_)JK",
                "kjgh  afkgj lsfg g sjg",
                "прлапрыл",
                "іїхїхїхїх'їїхїхаїіхмава",
                "fgsgsfgs",
                "56SearchТекстІї*01",
                "*/*SearchТекстІї*01/*",//<--
                "врлпываорпыра",
                "_searchТекстІї*011АРРОПРОлрффлвыа", //<--
                "РЛРОРЛолврфылваоф SearchТекстІї*01",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var testData = GetBaseWith20RecordsAndRecordTypes(namesList, GetDatesList(20), recTypes);
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter { RecordNameFilter = searchName };
            filter.AddRecordTypeId(typeId1);

            var result = await recRepos.FetchRecordsListFiltered(filter);

            Assert.IsNotEmpty(result);
            Assert.True(result[0].RecordName.IndexOf(searchName, StringComparison.CurrentCultureIgnoreCase) != -1);
            Assert.True(result[1].RecordName.IndexOf(searchName, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        [Test]        
        public async Task GetFilteredRecordsCount_ForNameAndType()
        {
            var recTypes = Enumerable.Range(0, 20).Select(i => new int[0]).ToArray();
            int typeId1 = 10;
            int typeId2 = 22;
            recTypes[4] = new int[] { typeId1 };
            recTypes[7] = new int[] { typeId1, typeId2 };
            recTypes[8] = new int[] { typeId2 };
            recTypes[10] = new int[] { typeId1 };
            recTypes[11] = new int[] { typeId2 };
            recTypes[17] = new int[] { typeId2 };
            recTypes[19] = new int[] { typeId1 };
            string searchName = "SearchТекстІї*01";
            const int matchesInRange = 2;
            var namesList = new[] {
                "ghorgh",
                "893472983 SearchТекстІї*01",
                "_)+_)+_)JK",
                "kjgh  afkgj lsfg g sjg",
                "прлапрыл",
                "іїхїхїхїх'їїхїхаїіхмава",
                "fgsgsfgs",
                "56SearchТекстІї*01",
                "*/*SearchТекстІї*01/*",//<--
                "врлпываорпыра",
                "_searchТекстІї*011АРРОПРОлрффлвыа", //<--
                "РЛРОРЛолврфылваоф SearchТекстІї*01",
                "жлажфлывлаДЛДЛО",
                "321321230",
                ",0,0,0,4уыы",
                "название",
                "фвафыа",
                "№%№SearchТекстІї*01%№!::!;%№:%; ",
                "ывп  ыапыап   папы ап ыап ыа",
                ".юб.б.юбс.б" };
            var dateFrom = DateTime.Now.AddDays(-10).Date;
            var dateTo = DateTime.Now.AddDays(-5).Date;
            var testData = GetBaseWith20RecordsAndRecordTypes(namesList, GetDatesList(20), recTypes);
            var recRepos = new RecordsRepository(testData.dbManager);
            var filter = new RecordsFilter { RecordNameFilter = searchName };
            filter.AddRecordTypeId(typeId1);

            int count = await recRepos.GetFilteredRecordsCount(filter);

            Assert.AreEqual(matchesInRange, count);
        }

        [Test]
        public async Task FetchYearsList_With7Records_ShouldReturnUniqueYears()
        {
            var recStor = new RecordsRepository(GetClearBase());
            var recs = new List<DiaryRecord>
            {
                new DiaryRecord { RecordDate = DateTime.Parse("2016-10-31 00:00:00") },
                new DiaryRecord { RecordDate = DateTime.Parse("2016-10-31 00:00:00") },
                new DiaryRecord { RecordDate = DateTime.Parse("2015-10-31 00:00:00") },
                new DiaryRecord { RecordDate = DateTime.Parse("2014-10-31 00:00:00") },
                new DiaryRecord { RecordDate = DateTime.Parse("2015-10-31 00:00:00") },
                new DiaryRecord { RecordDate = DateTime.Parse("2015-10-31 00:00:00") },
                new DiaryRecord { RecordDate = DateTime.Parse("2016-10-31 00:00:00") }
            };
            foreach(var rec in recs)
            {
                await recStor.AddRecord(rec);
            }

            List<int> result = await recStor.FetchYearsList();

            Assert.NotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.Contains(2016, result);
            Assert.Contains(2015, result);
            Assert.Contains(2014, result);
        }
    }
}
