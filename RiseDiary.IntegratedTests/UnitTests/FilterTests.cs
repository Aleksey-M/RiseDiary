using NUnit.Framework;
using RiseDiary.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RiseDiary.UnitTests
{
#pragma warning disable CA1812
    [TestFixture]
    class FilterTests
    {
        [Test]
        public void Filter_SetDateTo_ShouldCleareTime()
        {
            var recFilter = RecordsFilter.Empty;
            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            recFilter.ToDate = date;

            Assert.AreEqual(recFilter.ToDate, date);
        }

        [Test]
        public void Filter_SetDateFrom_ShouldCleareTime()
        {
            var recFilter = RecordsFilter.Empty;
            var date = DateOnly.FromDateTime(DateTime.UtcNow);

            recFilter.FromDate = date;

            Assert.AreEqual(recFilter.FromDate, date);
        }

        [Test]
        public void Filter_AddRecTypesIds_ShouldAddId()
        {
            var recFilter = RecordsFilter.Empty;
            var itemId1 = Guid.NewGuid();
            var itemId2 = Guid.NewGuid();

            recFilter.AddThemeId(itemId1);
            recFilter.AddThemeId(itemId2);

            Assert.AreEqual(2, recFilter.Themes.Count);
            Assert.IsTrue(recFilter.Themes.Contains(itemId1));
            Assert.IsTrue(recFilter.Themes.Contains(itemId2));
        }

        [Test]
        public void Filter_AddExistingRecTypesIds_ShouldNotAddId()
        {
            var recFilter = RecordsFilter.Empty;
            var itemId1 = Guid.NewGuid();

            recFilter.AddThemeId(itemId1);
            recFilter.AddThemeId(itemId1);// same id

            Assert.AreEqual(1, recFilter.Themes.Count);
            Assert.AreEqual(itemId1, recFilter.Themes[0]);
        }

        [Test]
        public void Filter_AddRecTypesList_ShouldAddId()
        {
            var recFilter = RecordsFilter.Empty;
            var recList = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            recFilter.AddThemeId(recList[4]);
            recFilter.AddThemeId(recList[0]);
            recFilter.AddThemeId(recList);

            Assert.AreEqual(recList.Length, recFilter.Themes.Count);
            Assert.IsTrue(recFilter.Themes.All(i => recList.Contains(i)));
            Assert.IsTrue(recList.All(i => recFilter.Themes.Contains(i)));
        }

        [Test]
        public void Filter_RemoveNotExistingId_ShouldDoNothing()
        {
            var recFilter = RecordsFilter.Empty;
            var id = Guid.NewGuid();

            recFilter.AddThemeId(id);

            recFilter.RemoveThemeId(Guid.NewGuid());

            Assert.AreEqual(1, recFilter.Themes.Count);
            Assert.AreEqual(id, recFilter.Themes[0]);
        }

        [Test]
        public void Filter_RemoveId_ShouldRemoveId()
        {
            var recFilter = RecordsFilter.Empty;
            var id = Guid.NewGuid();

            recFilter.AddThemeId(id);

            recFilter.RemoveThemeId(id);

            Assert.AreEqual(0, recFilter.Themes.Count);
        }

        [Test]
        public void Filter_RemoveIdList_ShouldRemoveId()
        {
            var recFilter = RecordsFilter.Empty;
            var recList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var id4 = recList[2];

            recFilter.AddThemeId(recList);
            recFilter.AddThemeId(id1);
            recFilter.AddThemeId(id2);

            recList.Remove(id4);
            recList.Add(id3);
            recFilter.RemoveThemeId(recList);

            Assert.AreEqual(3, recFilter.Themes.Count);
            Assert.IsTrue(recFilter.Themes.Contains(id4));
            Assert.IsTrue(recFilter.Themes.Contains(id1));
            Assert.IsTrue(recFilter.Themes.Contains(id2));
        }

        [Test]
        public void Filter_SetDateToProperty_ShouldSet()
        {
            var filter = RecordsFilter.Empty;
            var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));

            filter.ToDate = date;
            Assert.AreEqual(filter.ToDate, date);
            filter.ToDate = null;
            Assert.IsNull(filter.ToDate);
        }

        [Test]
        public void Filter_SetDateFromProperty_ShouldSet()
        {
            var filter = RecordsFilter.Empty;
            var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

            filter.FromDate = date;
            Assert.AreEqual(filter.FromDate, date);
            filter.FromDate = null;
            Assert.IsNull(filter.FromDate);
        }

        [Test]
        public void Filter_SetDateFromAndDateToProperties_TheSameDate_ShouldSet()
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

            var filter = new RecordsFilter { FromDate = date, ToDate = date };

            Assert.AreEqual(filter.FromDate, date);
            Assert.AreEqual(filter.ToDate, date);
            Assert.AreEqual(filter.ToDate, filter.FromDate);
            filter.FromDate = null;
            filter.ToDate = null;
            Assert.IsNull(filter.FromDate);
            Assert.IsNull(filter.ToDate);
        }

        [Test]
        public void Filter_SetDateFrom_GreaterThanDateTo_ShouldSetToNull()
        {
            var filter = RecordsFilter.Empty;
            var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));

            filter.FromDate = dateFrom;
            filter.ToDate = dateTo;

            Assert.AreEqual(filter.FromDate, dateFrom);
            Assert.IsNull(filter.ToDate);
        }

        [Test]
        public void Filter_SetDateTo_LessThanDateFrom_ShouldSetToNull()
        {
            var filter = RecordsFilter.Empty;
            var dateTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));

            filter.ToDate = dateTo;
            filter.FromDate = dateFrom;

            Assert.AreEqual(filter.ToDate, dateTo);
            Assert.IsNull(filter.FromDate);
        }
    }
}
