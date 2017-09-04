using NUnit.Framework;
using RiseDiary.Domain.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.Data.SqliteStorages.IntegratedTests
{
    [TestFixture]
    class DiaryImagesStorageTests : CleanUpTestFixtureBase
    {
        private DiaryImage GetTestImage() => new DiaryImage
        {
            CreateDate = DateTime.Now,
            ImageName = Guid.NewGuid().ToString()            
        };        
                
        [Test]
        public async Task AddImage_ShouldNotThrowException()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);

            int id = await imageStor.AddImage(GetTestImage(), new byte[1024*1024*25]); //new byte[] { 1, 2, 3, 4, 5 }

            Assert.AreNotEqual(0, id);
        }

        [Test]
        public async Task GetImage_WithNotExistingId_ShouldReturnNull()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);

            var img = await imageStor.FetchImageById(101);

            Assert.IsNull(img);
        }

        [Test]
        public async Task GetImage_ShouldReturnImage()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);
            var img = GetTestImage();

            int id = await imageStor.AddImage(img, new byte[] { 1, 1, 1, 1 });
            var imgSaved = await imageStor.FetchImageById(id);

            Assert.IsNotNull(imgSaved);
            Assert.AreEqual(img.ImageName, imgSaved.ImageName);
            Assert.AreEqual(img.CreateDate, imgSaved.CreateDate);
        }

        [Test]
        public async Task GetImageData_ShouldReturnImageData()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);
            var img = GetTestImage();

            var imgData = new byte[] { 1, 2, 3, 4 };
            int id = await imageStor.AddImage(img, imgData);
            var imgDataSaved = await imageStor.FetchImageDataById(id);

            Assert.IsNotNull(imgDataSaved);
            Assert.AreEqual(imgData.Length, imgDataSaved.Length);
            Assert.IsTrue(imgData.Zip(imgDataSaved, (byte1, byte2) => byte1 == byte2).All(concat => concat));
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnZero()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);

            int count = await imageStor.GetImagesCount();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnThree()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);
            for(int i = 0; i<3; i++)
            {
                await imageStor.AddImage(GetTestImage(), new byte[1024 * 1024 * 10]);
            }
            
            int count = await imageStor.GetImagesCount();

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task DeleteImage_ShouldDeleteOneImage()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);
            var ids = Enumerable.Range(0, 3).Select(async i => await imageStor.AddImage(GetTestImage(), new byte[1024 * 1024 * 10])).ToArray();

            await imageStor.DeleteImage(await ids[1]);

            Assert.IsNull(await imageStor.FetchImageById(await ids[1]));
            Assert.IsNotNull(await imageStor.FetchImageById(await ids[0]));
            Assert.IsNotNull(await imageStor.FetchImageById(await ids[2]));
        }

        [Test]
        public async Task UpdateImageName_ShouldUpdateImageName()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);
            int id = await imageStor.AddImage(GetTestImage(), new byte[1024 * 1024]);
            string newName = Guid.NewGuid().ToString();

            await imageStor.UpdateImageName(id, newName);
            var imgSaved = await imageStor.FetchImageById(id);

            Assert.NotNull(imgSaved);
            Assert.AreEqual(newName, imgSaved.ImageName);
        }

        [Test]
        public async Task GetImages_ShouldReturn3LastImages()
        {
            var manager = TestsHelper.GetClearBase();
            var imageStor = new DiaryImagesRepository(manager);
            var ids = Enumerable.Range(0, 10).Select(async i => await imageStor.AddImage(GetTestImage(), new byte[1024 * 1024 * 10])).ToArray();

            var page = await imageStor.FetchImageSet(7, 5);

            Assert.IsNotNull(page);
            Assert.AreEqual(3, page.Count);
        }
    }
}
