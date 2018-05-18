using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RiseDiary.Data.SqliteStorages.IntegratedTests.DbContextAdapter;
using RiseDiary.Data.SqliteStorages.IntegratedTests;
using RiseDiary.Data.SqliteStorages.IntegratedTests.TestDomain;

namespace RiseDiary.SqliteStorages.IntegratedTests
{
    [TestFixture]
    class AreasStorageTests : CleanUpTestFixtureBase
    {        
        [Test]
        public async Task AddArea_ShouldNotThrowException()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            
            int newId = await areaStor.AddArea(@"New Area ""!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй");

            Assert.AreEqual(1, newId);
        }
        
        [Test]
        public async Task AddSeveralAreas_ShouldReturnDifferentIds()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());

            int Id1 = await areaStor.AddArea("New Area 1");
            int Id2 = await areaStor.AddArea("New Area 2");
            int Id3 = await areaStor.AddArea("New Area 3");

            Assert.AreNotEqual(Id1, Id2);
            Assert.AreNotEqual(Id1, Id3);
            Assert.AreNotEqual(Id3, Id2);
        }

        [Test]
        public void AddArea_WithNullParameter_ShouldThrowArgumentException()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea(null));
        }

        [Test]
        public void AddArea_WithEmptyParameter_ShouldThrowArgumentException()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea(""));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea(" "));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea("   "));            
        }
        
        [Test]
        public async Task GetAreasCount_ShouldReturnZero()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());

            int count = await areaStor.GetAreasCount();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task GetAreasCount_ShouldReturn3()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());

            await areaStor.AddArea("New Area 1");
            await areaStor.AddArea("New Area 2");
            await areaStor.AddArea("New Area 3");
            int count = await areaStor.GetAreasCount();

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task GetArea_WithNotExistingId_ShouldReturnNull()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());

            var area = await areaStor.FetchAreaById(100);

            Assert.IsNull(area);
        }

        [Test]
        public async Task GetArea_ShouldReturnAreas()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            var areasData = new Dictionary<string, int>
            {
                { @"""_)(*&^%$#@!фівраХЇЇїіййєєє", await areaStor.AddArea(@"""_)(*&^%$#@!фівраХЇЇїіййєєє") },
                { @"...^&*(::;[]", await areaStor.AddArea(@"...^&*(::;[]") },
                { @"'|||\\//", await areaStor.AddArea(@"'|||\\//") }
            };
            
            foreach(KeyValuePair<string, int> pair in areasData)
            {
                Assert.AreEqual(pair.Key, (await areaStor.FetchAreaById(pair.Value)).AreaName);
            }
        }

        [Test]
        public async Task GetAreas_ShouldReturnEmptyList()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());

            var list = await areaStor.FetchAllAreas();

            Assert.NotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public async Task GetAreas_ShouldReturnListWith3Areas()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            await areaStor.AddArea("New Area 1");
            await areaStor.AddArea("New Area 2");
            await areaStor.AddArea("New Area 3");

            var list = await areaStor.FetchAllAreas();

            Assert.NotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        [Test]
        public void UpdateArea_WithNotExistingId_ShouldThrowArgumentException()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            var area = new DiaryArea { AreaId = 7, AreaName = "not existing area" };

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.UpdateArea(area));
        }

        [Test]
        public async Task UpdateArea_ShouldUpdateAreaName()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            int id = await areaStor.AddArea("New Area 1");
            var area = await areaStor.FetchAreaById(id);

            area.AreaName = @"""'''[]!@#$%^&*()_+::"":;;<><>,.";                            
            int id2 = await areaStor.UpdateArea(area);
            var updatedArea = await areaStor.FetchAreaById(id2);

            Assert.AreEqual(id, id2);
            Assert.AreEqual(area.AreaName, updatedArea.AreaName);
        }

        [Test]
        public async Task CanDeleteArea_ShouldReturnTrue()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            int id = await areaStor.AddArea("New Area 1");

            Assert.IsTrue(await areaStor.CanDeleteArea(id));
        }
        
        [Test]
        public async Task CanDeleteArea_ShouldReturnFalse()
        {
            var mngr = TestHelper.GetClearBase();
            var areaStor = new AreasRepository(mngr);
            var typesStor = new RecordTypesRepository(mngr);
            
            int areaId = await areaStor.AddArea("New Area 1");
            int typeId = await typesStor.AddRecordType(areaId, "New type");

            Assert.IsFalse(await areaStor.CanDeleteArea(areaId));
        }
        
        [Test]
        public async Task DeleteArea_WithNotExistingId_ShouldNotThrowException()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            await areaStor.DeleteArea(150);
        }

        [Test]
        public async Task DeleteArea_ShouldDeleteArea()
        {
            var areaStor = new AreasRepository(TestHelper.GetClearBase());
            int id = await areaStor.AddArea("New Area 1");

            await areaStor.DeleteArea(id);
            var area = await areaStor.FetchAreaById(id);

            Assert.IsNull(area);
        }
    }
}
