using NUnit.Framework;
using RiseDiary.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests.Tests
{
    [TestFixture]
    class RecordsFilterTests
    {
        [Test]
        public void Filter_SetDateTo_ShouldCleareTime()
        {
            var recFilter = RecordsFilter.Empty;
            var date = DateTime.Now;

            recFilter.RecordDateTo = date;

            Assert.AreEqual(recFilter.RecordDateTo, date.Date);
        }

        [Test]
        public void Filter_SetDateFrom_ShouldCleareTime()
        {
            var recFilter = RecordsFilter.Empty;
            var date = DateTime.Now;

            recFilter.RecordDateFrom = date;

            Assert.AreEqual(recFilter.RecordDateFrom, date.Date);
        }

        [Test]
        public void Filter_AddRecTypesIds_ShouldAddId()
        {
            var recFilter = RecordsFilter.Empty;

            recFilter.AddRecordTypeId(105);
            recFilter.AddRecordTypeId(106);

            Assert.AreEqual(2, recFilter.RecordTypeIds.Count);
            Assert.IsTrue(recFilter.RecordTypeIds.Contains(105));
            Assert.IsTrue(recFilter.RecordTypeIds.Contains(106));
        }

        [Test]
        public void Filter_AddExistingRecTypesIds_ShouldNotAddId()
        {
            var recFilter = RecordsFilter.Empty;

            recFilter.AddRecordTypeId(105);
            recFilter.AddRecordTypeId(105);// same id

            Assert.AreEqual(1, recFilter.RecordTypeIds.Count);
            Assert.AreEqual(105, recFilter.RecordTypeIds[0]);
        }

        [Test]
        public void Filter_AddRecTypesList_ShouldAddId()
        {
            var recFilter = RecordsFilter.Empty;
            var recList = new int[] { 101, 102, 103, 104, 105 };

            recFilter.AddRecordTypeId(105);
            recFilter.AddRecordTypeId(101);
            recFilter.AddRecordTypeId(recList);

            Assert.AreEqual(recList.Count(), recFilter.RecordTypeIds.Count);
            Assert.IsTrue(recFilter.RecordTypeIds.All(i => recList.Contains(i)));
            Assert.IsTrue(recList.All(i => recFilter.RecordTypeIds.Contains(i)));
        }

        [Test]
        public void Filter_RemoveNotExistingId_ShouldDoNothing()
        {
            var recFilter = RecordsFilter.Empty;
            recFilter.AddRecordTypeId(105);

            recFilter.RemoveRecordTypeId(101);

            Assert.AreEqual(1, recFilter.RecordTypeIds.Count);
            Assert.AreEqual(105, recFilter.RecordTypeIds[0]);
        }

        [Test]
        public void Filter_RemoveId_ShouldRemoveId()
        {
            var recFilter = RecordsFilter.Empty;
            recFilter.AddRecordTypeId(105);

            recFilter.RemoveRecordTypeId(105);

            Assert.AreEqual(0, recFilter.RecordTypeIds.Count);
        }

        [Test]
        public void Filter_RemoveIdList_ShouldRemoveId()
        {
            var recFilter = RecordsFilter.Empty;
            var recList = new List<int> { 101, 102, 103, 104, 105 };
            recFilter.AddRecordTypeId(recList);
            recFilter.AddRecordTypeId(108);
            recFilter.AddRecordTypeId(99);

            recList.Remove(103);
            recList.Add(111);
            recFilter.RemoveRecordTypeId(recList);

            Assert.AreEqual(3, recFilter.RecordTypeIds.Count);
            Assert.IsTrue(recFilter.RecordTypeIds.Contains(103));
            Assert.IsTrue(recFilter.RecordTypeIds.Contains(99));
            Assert.IsTrue(recFilter.RecordTypeIds.Contains(108));
        }

        [Test]
        public void Filter_SetDateToProperty_ShouldSet()
        {
            var filter = RecordsFilter.Empty;
            var date = DateTime.Now.AddDays(-3);

            filter.RecordDateTo = date;
            Assert.AreEqual(filter.RecordDateTo, date.Date);
            filter.RecordDateTo = null;
            Assert.IsNull(filter.RecordDateTo);
        }

        [Test]
        public void Filter_SetDateFromProperty_ShouldSet()
        {
            var filter = RecordsFilter.Empty;
            var date = DateTime.Now.AddDays(-7);

            filter.RecordDateFrom = date;
            Assert.AreEqual(filter.RecordDateFrom, date.Date);
            filter.RecordDateFrom = null;
            Assert.IsNull(filter.RecordDateFrom);
        }

        [Test]
        public void Filter_SetDateFromAndDateToProperties_TheSameDate_ShouldSet()
        {
            var date = DateTime.Now.AddDays(-7);

            var filter = new RecordsFilter { RecordDateFrom = date, RecordDateTo = date };

            Assert.AreEqual(filter.RecordDateFrom, date.Date);
            Assert.AreEqual(filter.RecordDateTo, date.Date);
            Assert.AreEqual(filter.RecordDateTo, filter.RecordDateFrom);
            filter.RecordDateFrom = null;
            filter.RecordDateTo = null;
            Assert.IsNull(filter.RecordDateFrom);
            Assert.IsNull(filter.RecordDateTo);
        }

        [Test]
        public void Filter_SetDateFrom_GreaterThanDateTo_ShouldSetToNull()
        {
            var filter = RecordsFilter.Empty;
            var dateTo = DateTime.Now.AddDays(-7);
            var dateFrom = DateTime.Now.AddDays(-3);

            filter.RecordDateFrom = dateFrom;
            filter.RecordDateTo = dateTo;

            Assert.AreEqual(filter.RecordDateFrom, dateFrom.Date);
            Assert.IsNull(filter.RecordDateTo);
        }

        [Test]
        public void Filter_SetDateTo_LessThanDateFrom_ShouldSetToNull()
        {
            var filter = RecordsFilter.Empty;
            var dateTo = DateTime.Now.AddDays(-7);
            var dateFrom = DateTime.Now.AddDays(-3);

            filter.RecordDateTo = dateTo;
            filter.RecordDateFrom = dateFrom;            

            Assert.AreEqual(filter.RecordDateTo, dateTo.Date);
            Assert.IsNull(filter.RecordDateFrom);
        }
    }
}
