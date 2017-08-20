﻿using NUnit.Framework;
using RiseDiary.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    [TestFixture]
    class AreasStorageTests : CleanUpTestFixtureBase
    {        
        [Test]
        public async Task AddArea_ShouldNotThrowException()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            
            int newId = await areaStor.AddArea(@"New Area ""!@#$%^''""&*()_+,.<><>?//[]||\\апрорпывоаъъЇЇііі.єєєйй");

            Assert.AreEqual(1, newId);
        }

        [Test]
        public async Task AddSeveralAreas_ShouldReturnDifferentIds()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());

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
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea(null));
        }

        [Test]
        public void AddArea_WithEmptyParameter_ShouldThrowArgumentException()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea(""));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea(" "));
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.AddArea("   "));            
        }

        [Test]
        public async Task GetAreasCount_ShouldReturnZero()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());

            int count = await areaStor.GetAreasCount();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task GetAreasCount_ShouldReturn3()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());

            await areaStor.AddArea("New Area 1");
            await areaStor.AddArea("New Area 2");
            await areaStor.AddArea("New Area 3");
            int count = await areaStor.GetAreasCount();

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task GetArea_WithNotExistingId_ShouldReturnNull()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());

            var area = await areaStor.GetArea(100);

            Assert.IsNull(area);
        }

        [Test]
        public async Task GetArea_ShouldReturnAreas()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            var areasData = new Dictionary<string, int>
            {
                { @"""_)(*&^%$#@!фівраХЇЇїіййєєє", await areaStor.AddArea(@"""_)(*&^%$#@!фівраХЇЇїіййєєє") },
                { @"...^&*(::;[]", await areaStor.AddArea(@"...^&*(::;[]") },
                { @"'|||\\//", await areaStor.AddArea(@"'|||\\//") }
            };
            
            foreach(KeyValuePair<string, int> pair in areasData)
            {
                Assert.AreEqual(pair.Key, (await areaStor.GetArea(pair.Value)).AreaName);
            }
        }

        [Test]
        public async Task GetAreas_ShouldReturnEmptyList()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());

            var list = await areaStor.GetAreas();

            Assert.NotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public async Task GetAreas_ShouldReturnListWith3Areas()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            await areaStor.AddArea("New Area 1");
            await areaStor.AddArea("New Area 2");
            await areaStor.AddArea("New Area 3");

            var list = await areaStor.GetAreas();

            Assert.NotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        [Test]
        public void UpdateArea_WithNotExistingId_ShouldThrowArgumentException()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            var area = new DiaryArea { AreaId = 7, AreaName = "not existing area" };

            int id;
            Assert.ThrowsAsync<ArgumentException>(async () => id = await areaStor.UpdateArea(area));
        }

        [Test]
        public async Task UpdateArea_ShouldUpdateAreaName()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            int id = await areaStor.AddArea("New Area 1");
            var area = await areaStor.GetArea(id);

            area.AreaName = @"""'''[]!@#$%^&*()_+::"":;;<><>,.";                            
            int id2 = await areaStor.UpdateArea(area);
            var updatedArea = await areaStor.GetArea(id2);

            Assert.AreEqual(id, id2);
            Assert.AreEqual(area.AreaName, updatedArea.AreaName);
        }

        [Test]
        public async Task CanDeleteArea_ShouldReturnTrue()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            int id = await areaStor.AddArea("New Area 1");

            Assert.IsTrue(await areaStor.CanDeleteArea(id));
        }

        [Test]
        public async Task CanDeleteArea_ShouldReturnFalse()
        {
            var mngr = TestsHelper.GetClearBase();
            var areaStor = new AreasStorage(mngr);
            var typesStor = new RecordTypesStorage(mngr);
            
            int areaId = await areaStor.AddArea("New Area 1");
            int typeId = await typesStor.AddRecordType(areaId, "New type");

            Assert.IsFalse(await areaStor.CanDeleteArea(areaId));
        }

        [Test]
        public async Task DeleteArea_WithNotExistingId_ShouldNotThrowException()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            await areaStor.DeleteArea(150);
        }

        [Test]
        public async Task DeleteArea_ShouldDeleteArea()
        {
            var areaStor = new AreasStorage(TestsHelper.GetClearBase());
            int id = await areaStor.AddArea("New Area 1");

            await areaStor.DeleteArea(id);
            var area = await areaStor.GetArea(id);

            Assert.IsNull(area);
        }
    }
}