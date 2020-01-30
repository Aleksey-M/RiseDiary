using NUnit.Framework;
using RiseDiary.Model;
using System;
using System.Linq;

namespace RiseDiary.IntegratedTests
{
    [TestFixture]
    internal class DatesTests : TestFixtureBase
    {


    }
}

namespace RiseDiary.UnitTests
{
    [TestFixture]
    internal class DatesRangeClassTests
    {
        [Test]
        public void DatesRange_ctor1_WithCorrectParameters_ShouldCreate()
        {
            var today = new DateTime(2020, 03, 11);
            int datesRange = 7;

            var testRange = new DatesRange(today, datesRange);

            Assert.AreEqual(today, testRange.Today);
            Assert.AreEqual(today.AddDays(datesRange), testRange.ToDate);
            Assert.AreEqual(today.AddDays(-datesRange), testRange.FromDate);
            Assert.IsFalse(testRange.AtTheJunctionOfYears);
            Assert.AreEqual(14, testRange.AllRangeDates.Count());
        }
    }
}   
