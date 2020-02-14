using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Linq;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace RiseDiary.IntegratedTests
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CA1812
    [TestFixture]
    class ImageTests : TestFixtureBase
    {
        [Test]
        public async Task AddImage_ShouldNotThrowException()
        {
            var context = CreateContext();

            var id = await context.AddImage(UniqueString, ImageBytes, 75);

            Assert.AreNotEqual(0, id);
        }

        [Test]
        public async Task GetImage_WithNotExistingId_ShouldReturnNull()
        {
            var context = CreateContext();

            var img = await context.FetchImageById(Guid.NewGuid());

            Assert.IsNull(img);
        }

        [Test]
        public async Task GetImage_ShouldReturnImage()
        {
            var context = CreateContext();
            var img = GetTestImage();

            var id = await context.AddImage(img.Name, img.Thumbnail, 75);
            var imgSaved = await context.FetchImageById(id);

            Assert.IsNotNull(imgSaved);
            Assert.AreEqual(img.Name, imgSaved.Name);
            Assert.AreEqual(img.CreateDate.Date, imgSaved.CreateDate.Date);
        }

        [Test]
        public async Task GetImageData_ShouldReturnImageData()
        {
            var context = CreateContext();
            var img = GetTestImage();
            var fullImage = img.Thumbnail;

            var id = await context.AddImage(img.Name, img.Thumbnail, 75);
            var imgDataSaved = await context.FetchFullImageById(id);

            Assert.IsNotNull(imgDataSaved);
            Assert.AreEqual(fullImage.Length, imgDataSaved.Length);
            Assert.IsTrue(fullImage.Zip(imgDataSaved, (byte1, byte2) => byte1 == byte2).All(concat => concat));
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnZero()
        {
            var context = CreateContext();

            int count = await context.GetImagesCount();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnThree()
        {
            var context = CreateContext();
            for (int i = 0; i < 3; i++)
            {
                await context.AddImage(GetTestImage().Name, GetTestImage().Thumbnail, 75);
            }

            int count = await context.GetImagesCount();

            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task DeleteImage_ShouldDeleteOneImage()
        {
            var context = CreateContext();
            var imagesId = new List<Guid>();
            for (int i = 0; i < 3; i++)
            {
                imagesId.Add(await context.AddImage(UniqueString, ImageBytes, 75));
            }

            await context.DeleteImage(imagesId[1]);

            Assert.IsNull(await context.FetchImageById(imagesId[1]));
            Assert.IsNotNull(await context.FetchImageById(imagesId[0]));
            Assert.IsNotNull(await context.FetchImageById(imagesId[2]));

            var id = imagesId[1];
            Assert.IsNull(context.Images.FirstOrDefault(i => i.Id == id));
        }

        [Test]
        public async Task UpdateImageName_ShouldUpdateImageName()
        {
            var context = CreateContext();
            var id = await context.AddImage(UniqueString, ImageBytes, 75);
            string newName = Guid.NewGuid().ToString();

            await context.UpdateImageName(id, newName);
            var imgSaved = await context.FetchImageById(id);

            Assert.NotNull(imgSaved);
            Assert.AreEqual(newName, imgSaved.Name);
        }

        [Test]
        public async Task GetImages_ShouldReturn3LastImages()
        {
            var context = CreateContext();
            for (int i = 0; i < 10; i++)
            {
                await context.AddImage(UniqueString, ImageBytes, 75);
            }

            var page = await context.FetchImageSet(7, 5);

            Assert.IsNotNull(page);
            Assert.AreEqual(3, page.Count);
        }

        [Test]
        public async Task BindRecord_RecordImage_DeleteImage_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            var imageId = Create_Image(context);
            var recId = Create_Record(context);
            await context.AddRecordImage(recId, imageId);

            var bindRec = await context.RecordImages.FirstOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imageId);
            Assert.IsNotNull(bindRec);

            await context.DeleteImage(imageId);

            bindRec = await context.RecordImages.FirstOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imageId);
            Assert.IsNull(bindRec);
        }

        [Test]
        public async Task BindRecord_RecordImage_DeleteRecord_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            var imageId = Create_Image(context);
            var recId = Create_Record(context);
            await context.AddRecordImage(recId, imageId);

            var bindRec = await context.RecordImages.FirstOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imageId);
            Assert.IsNotNull(bindRec);

            await context.DeleteRecord(recId);

            bindRec = await context.RecordImages.FirstOrDefaultAsync(br => br.RecordId == recId && br.ImageId == imageId);
            Assert.IsNull(bindRec);
        }

        [Test]
        public async Task AddImageForRecord_AfterDeletingTheSame_ShouldUnmarkAsDeleted()
        {
            var context = CreateContext();
            var imageId = Create_Image(context);
            var recId = Create_Record(context);

            await context.AddRecordImage(recId, imageId);
            await context.RemoveRecordImage(recId, imageId);
            await context.AddRecordImage(recId, imageId);

            int boundRecordCount = context.RecordImages.Count(br => br.RecordId == recId && br.ImageId == imageId);
            Assert.AreEqual(1, boundRecordCount);
        }
    }
}
