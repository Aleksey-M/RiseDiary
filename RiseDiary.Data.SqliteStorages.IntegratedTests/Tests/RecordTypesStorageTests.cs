using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    [TestFixture]
    class RecordTypesStorageTests : CleanUpTestFixtureBase
    {
        [Test]
        public async Task AddRecordType_ShouldNotThrowException()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            int newRecId = await typesStor.AddRecordType(10, @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй");

            Assert.AreEqual(1, newRecId);
        }

        [Test]
        public async Task GetRecordTypesCount_WithoutAreaIds_ShouldReturn3()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            await typesStor.AddRecordType(await areaStor.AddArea("1"), "type 1");
            await typesStor.AddRecordType(await areaStor.AddArea("2"), "type 2");
            await typesStor.AddRecordType(await areaStor.AddArea("3"), "type 3");
            int recTypesCount = await typesStor.GetRecordTypesCount(null);

            Assert.AreEqual(3, recTypesCount);
        }

        [Test]
        public async Task GetRecordTypesCount_WithAreaId_ShouldReturnFilteredCount()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            int aId1 = await areaStor.AddArea("1");
            int aId2 = await areaStor.AddArea("2");
            await typesStor.AddRecordType(aId1, "type 1-1");
            await typesStor.AddRecordType(aId1, "type 1-2");
            await typesStor.AddRecordType(aId2, "type 2-1");
            await typesStor.AddRecordType(aId2, "type 2-2");
            await typesStor.AddRecordType(aId2, "type 2-3");

            int recTypesCount = await typesStor.GetRecordTypesCount(aId1);
            Assert.AreEqual(2, recTypesCount);
            recTypesCount = await typesStor.GetRecordTypesCount(aId2);
            Assert.AreEqual(3, recTypesCount);
        }

        [Test]
        public async Task GetRecordType_WithNotExistingId_ShouldReturnNull()
        {
            var mngr = TestsHelper.GetClearBase();
            var typesStor = new RecordTypesRepository(mngr);

            var recType = await typesStor.FetchRecordTypeById(110);

            Assert.IsNull(recType);
        }

        [Test]
        public async Task GetRecordType_ShouldReturnRecordType()
        {
            var mngr = TestsHelper.GetClearBase();
            var typesStor = new RecordTypesRepository(mngr);
            string recTypeName = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй";

            int recTypeId = await typesStor.AddRecordType(1, recTypeName);
            var recType = await typesStor.FetchRecordTypeById(recTypeId);

            Assert.AreEqual(1, recType.AreaId);
            Assert.AreEqual(recTypeName, recType.RecordTypeName);
        }

        [Test]
        public async Task UpdateRecordType_ShouldUpdateRecordTypeName()
        {
            var mngr = TestsHelper.GetClearBase();
            var typesStor = new RecordTypesRepository(mngr);
            string recTypeNameOld = @"!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй";
            string recTypeNameNew = @"}{}{P}ЪХЪХъыйыбЙЇіїіїієєєжж.ююббьчсимеуdgfjsjs";

            int recTypeId = await typesStor.AddRecordType(1, recTypeNameOld);
            var recType = await typesStor.FetchRecordTypeById(recTypeId);
            recType.RecordTypeName = recTypeNameNew;
            await typesStor.UpdateRecordType(recType);
            recType = await typesStor.FetchRecordTypeById(recTypeId);

            Assert.AreEqual(recTypeNameNew, recType.RecordTypeName);
        }

        [Test]
        public async Task DeleteRecordType_WithNotExistingId_ShouldNotThrowException()
        {
            var mngr = TestsHelper.GetClearBase();
            var typesStor = new RecordTypesRepository(mngr);

            await typesStor.DeleteRecordType(1112);
        }

        [Test]
        public async Task DeleteRecordType_ShouldDeleteRecordType()
        {
            var mngr = TestsHelper.GetClearBase();
            var typesStor = new RecordTypesRepository(mngr);

            int recTypeId = await typesStor.AddRecordType(10, "111");
            var recType = await typesStor.FetchRecordTypeById(recTypeId);
            Assert.IsNotNull(recType);

            await typesStor.DeleteRecordType(recTypeId);
            recType = await typesStor.FetchRecordTypeById(recTypeId);
            Assert.IsNull(recType);
        }

        [Test]
        public async Task GetRecordTypes_ShouldReturnEmptyList()
        {
            var mngr = TestsHelper.GetClearBase();
            var typesStor = new RecordTypesRepository(mngr);
            var areaStor = new AreasRepository(mngr);
            int areaId = await areaStor.AddArea("1111");

            var lst = await typesStor.FetchRecordTypesOfArea(null);
            Assert.IsNotNull(lst);
            Assert.AreEqual(0, lst.Count);

            lst = await typesStor.FetchRecordTypesOfArea(areaId);
            Assert.IsNotNull(lst);
            Assert.AreEqual(0, lst.Count);
        }

        [Test]
        public async Task GetRecordTypes_WithoutAreaId_ShouldReturnAllRecordTypes()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            await typesStor.AddRecordType(await areaStor.AddArea("1"), "type 1");
            await typesStor.AddRecordType(await areaStor.AddArea("2"), "type 2");
            await typesStor.AddRecordType(await areaStor.AddArea("3"), "type 3");
            int recTypesCount = await typesStor.GetRecordTypesCount(null);

            var lst = await typesStor.FetchRecordTypesOfArea(null);
            Assert.IsNotNull(lst);
            Assert.AreEqual(3, lst.Count);
        }

        [Test]
        public async Task GetRecordTypes_WithAreaId_ShouldReturnFilteredLists()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            int aId1 = await areaStor.AddArea("1");
            int aId2 = await areaStor.AddArea("2");
            await typesStor.AddRecordType(aId1, "type 1-1");
            await typesStor.AddRecordType(aId1, "type 1-2");
            await typesStor.AddRecordType(aId2, "type 2-1");
            await typesStor.AddRecordType(aId2, "type 2-2");
            await typesStor.AddRecordType(aId2, "type 2-3");

            var lst = await typesStor.FetchRecordTypesOfArea(aId1);
            Assert.IsNotNull(lst);
            Assert.AreEqual(2, lst.Count);
            lst = await typesStor.FetchRecordTypesOfArea(aId2);
            Assert.IsNotNull(lst);
            Assert.AreEqual(3, lst.Count);
        }

        [Test]
        public async Task GetRecordTypesJoined_ShouldReturnJoinedListOfRecordTypesAndAreasNames()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            int aId1 = await areaStor.AddArea("1");
            int aId2 = await areaStor.AddArea("2");
            await typesStor.AddRecordType(aId1, "type 1-1");
            await typesStor.AddRecordType(aId1, "type 1-2");
            await typesStor.AddRecordType(aId2, "type 2-1");
            await typesStor.AddRecordType(aId2, "type 2-2");
            await typesStor.AddRecordType(aId2, "type 2-3");
            await typesStor.AddRecordType(1001, "type 000");

            var joinedList = await typesStor.FetchRecordTypesWithAreas();
            Assert.IsNotNull(joinedList);
            Assert.AreEqual(6, joinedList.Count);
            Assert.AreEqual(2, joinedList.Where(jr => jr.AreaName == "1").Count());
            Assert.AreEqual(3, joinedList.Where(jr => jr.AreaName == "2").Count());
            Assert.AreEqual(1, joinedList.Where(jr => jr.AreaName == null).Count());
        }

        [Test]
        public async Task FetchRecordTypesIds_WithoutAreaId_ShouldReturnAllRecordTypes()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            await typesStor.AddRecordType(await areaStor.AddArea("1"), "type 1");
            await typesStor.AddRecordType(await areaStor.AddArea("2"), "type 2");
            await typesStor.AddRecordType(await areaStor.AddArea("3"), "type 3");
            int recTypesCount = await typesStor.GetRecordTypesCount(null);

            var lst = await typesStor.FetchRecordTypesIds(null);
            Assert.IsNotNull(lst);
            Assert.AreEqual(3, lst.Count);
        }

        [Test]
        public async Task FetchRecordTypesIds_WithAreaId_ShouldReturnFilteredLists()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);

            int aId1 = await areaStor.AddArea("1");
            int aId2 = await areaStor.AddArea("2");
            await typesStor.AddRecordType(aId1, "type 1-1");
            await typesStor.AddRecordType(aId1, "type 1-2");
            await typesStor.AddRecordType(aId2, "type 2-1");
            await typesStor.AddRecordType(aId2, "type 2-2");
            await typesStor.AddRecordType(aId2, "type 2-3");

            var lst = await typesStor.FetchRecordTypesIds(aId1);
            Assert.IsNotNull(lst);
            Assert.AreEqual(2, lst.Count);
            var lst2 = await typesStor.FetchRecordTypesIds(aId2);
            Assert.IsNotNull(lst2);
            Assert.AreEqual(3, lst2.Count);
            Assert.True(lst.TrueForAll(i => !lst2.Contains(i)));
        }
    }
}
