using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Linq;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.SqliteStorages.IntegratedTests
{
    [TestFixture]
    class ImageTests : TestFixtureBase
    {
        private DiaryImage GetTestImage() => new DiaryImage
        {
            CreateDate = DateTime.Now,
            Name = Guid.NewGuid().ToString(),
            Data = new byte[1024 * 1024 * 25]
        };

        [Test]
        public async Task AddImage_ShouldNotThrowException()
        {
            var context =  CreateContext();

            int id = await context.AddImage(GetTestImage()); 

            Assert.AreNotEqual(0, id);
        }

        [Test]
        public async Task GetImage_WithNotExistingId_ShouldReturnNull()
        {
            var context =  CreateContext();

            var img = await context.FetchImageById(101);

            Assert.IsNull(img);
        }

        [Test]
        public async Task GetImage_ShouldReturnImage()
        {
            var context =  CreateContext();
            var img = GetTestImage();
            img.Data = new byte[] { 1, 1, 1, 1 };

            int id = await context.AddImage(img);
            var imgSaved = await context.FetchImageById(id);

            Assert.IsNotNull(imgSaved);
            Assert.AreEqual(img.Name, imgSaved.Name);
            Assert.AreEqual(img.CreateDate, imgSaved.CreateDate);
        }

        [Test]
        public async Task GetImageData_ShouldReturnImageData()
        {
            var context =  CreateContext();
            var img = GetTestImage();
            img.Data = new byte[] { 1, 2, 3, 4 };

            int id = await context.AddImage(img);
            var imgDataSaved = await context.FetchImageDataById(id);

            Assert.IsNotNull(imgDataSaved);
            Assert.AreEqual(img.Data.Length, imgDataSaved.Length);
            Assert.IsTrue(img.Data.Zip(imgDataSaved, (byte1, byte2) => byte1 == byte2).All(concat => concat));
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnZero()
        {
            var context =  CreateContext();

            int count = await context.GetImagesCount();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnThree()
        {
            var context =  CreateContext();
            for (int i = 0; i < 3; i++)
            {
                await context.AddImage(GetTestImage());
            }

            int count = await context.GetImagesCount();

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task DeleteImage_ShouldDeleteOneImage()
        {
            var context =  CreateContext();
            var ids = Enumerable.Range(0, 3).Select(async i => await context.AddImage(GetTestImage())).ToArray();

            await context.DeleteImage(await ids[1]);

            Assert.IsNull(await context.FetchImageById(await ids[1]));
            Assert.IsNotNull(await context.FetchImageById(await ids[0]));
            Assert.IsNotNull(await context.FetchImageById(await ids[2]));
        }

        [Test]
        public async Task UpdateImageName_ShouldUpdateImageName()
        {
            var context =  CreateContext();
            int id = await context.AddImage(GetTestImage());
            string newName = Guid.NewGuid().ToString();

            await context.UpdateImageName(id, newName);
            var imgSaved = await context.FetchImageById(id);

            Assert.NotNull(imgSaved);
            Assert.AreEqual(newName, imgSaved.Name);
        }

        [Test]
        public async Task GetImages_ShouldReturn3LastImages()
        {
            var context =  CreateContext();
            var ids = Enumerable.Range(0, 10).Select(async i => await context.AddImage(GetTestImage())).ToArray();

            var page = await context.FetchImageSet(7, 5);

            Assert.IsNotNull(page);
            Assert.AreEqual(3, page.Count);
        }
    }
}
