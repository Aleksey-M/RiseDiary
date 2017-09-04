using NUnit.Framework;
using RiseDiary.Domain.Model;
using System;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    [TestFixture]
    class RecordsStorageTests : CleanUpTestFixtureBase
    {
        [Test]
        public void AddDiaryRecord_ShouldReturnUniqueIdForEveryRecord()
        {
            var recStor = new RecordsRepository(TestsHelper.GetClearBase());
            var rec = TestsHelper.GetTestRecord();

            int i = recStor.AddRecord(rec).Result;
            int j = recStor.AddRecord(rec).Result;

            Assert.AreNotEqual(i, j);
        }

        [Test]
        public void GetRecord_ShouldReturnSavedRecord()
        {
            var recStor = new RecordsRepository(TestsHelper.GetClearBase());
            var rec = TestsHelper.GetTestRecord();

            int id = recStor.AddRecord(rec).Result;

            var loadedRec = recStor.FetchRecordById(id).Result;

            Assert.NotNull(loadedRec);
            Assert.AreEqual(rec.RecordCreateDate, loadedRec.RecordCreateDate);
            Assert.AreEqual(rec.RecordDate, loadedRec.RecordDate);
            Assert.AreEqual(rec.RecordModifyDate, loadedRec.RecordModifyDate);
            Assert.AreEqual(rec.RecordName, loadedRec.RecordName);
            Assert.AreEqual(rec.RecordText, loadedRec.RecordText);
        }

        [Test]
        public void DeleteRecord_ShouldDeleteRecord()
        {
            var recStor = new RecordsRepository(TestsHelper.GetClearBase());
            var rec = TestsHelper.GetTestRecord();

            int id = recStor.AddRecord(rec).Result;
            var loadedRec = recStor.FetchRecordById(id).Result;
            if (loadedRec == null) Assert.Ignore("Error saving diary records");
            recStor.DeleteRecord(id).Wait();
            loadedRec = recStor.FetchRecordById(id).Result;

            Assert.IsNull(loadedRec);
        }

        [Test]
        public void UpdateRecord_ShouldUpdateFields()
        {
            var recStor = new RecordsRepository(TestsHelper.GetClearBase());
            var rec = TestsHelper.GetTestRecord();
            var updatedRec = new DiaryRecord
            {
                RecordCreateDate = DateTime.Now.AddDays(-5),
                RecordModifyDate = DateTime.Now.AddDays(-3),
                RecordDate = DateTime.Now.AddDays(-4),
                RecordName = "!@#$&))*/-/-*/яяъъъыыіііїїїїххїїїйїхїцйхц",
                RecordText= "Инфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты ASИнфраструктура ASP.NET MVC 5 представляет собой последнюю версию веб-платформы ASP.NET от Microsoft. Она предлагает высокопродуктивную модель программирования, которая способствует построению более чистой кодовой архитектуры, обеспечивает разработку через тестирование и поддерживает повсеместную расширяемость в комбинации со всеми преимуществами ASP.NET. У инфраструктуры ASP.NET MVC есть множество преимуществ, по сравнению с классической платформой веб-разработки ASP.NET Web Forms. Компоненты AS"
            };

            updatedRec.RecordId = rec.RecordId = recStor.AddRecord(rec).Result;
            recStor.UpdateRecord(updatedRec).Wait();
            rec = recStor.FetchRecordById(updatedRec.RecordId).Result;

            Assert.NotNull(rec);
            Assert.AreEqual(updatedRec.RecordCreateDate, rec.RecordCreateDate);
            Assert.AreEqual(updatedRec.RecordDate, rec.RecordDate);
            Assert.AreEqual(updatedRec.RecordModifyDate, rec.RecordModifyDate);
            Assert.AreEqual(updatedRec.RecordName, rec.RecordName);
            Assert.AreEqual(updatedRec.RecordText, rec.RecordText);
        }
    }
}
