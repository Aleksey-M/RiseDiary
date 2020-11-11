using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Services
{
    internal class ImagesServiceTests : TestedServices
    {

        [Test]
        public async Task AddImage_ShouldNotThrowException()
        {
            var svc = GetImagesService();

            var imageId = await svc.AddImage(TestFile);

            imageId.Should().NotBeEmpty();
        }

        [Test]
        public async Task FetchImageById_WithNotExistingId_ShouldThrowArgumentException()
        {
            var svc = GetImagesService();
            Func<Task> action = async () => _ = await svc.FetchImageById(Guid.NewGuid());

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task FetchImageById_ShouldReturnAddedImage()
        {
            var svc = GetImagesService();
            var imageId = await svc.AddImage(TestFile, "Test image name");

            var image = await svc.FetchImageById(imageId);

            image.Should().NotBeNull();
            image.Name.Should().Be("Test image name");
            image.CreateDate.Should().BeCloseTo(DateTime.Now, 500);
        }

        [Test]
        public async Task GetImageData_ShouldReturnImageData()
        {
            var svc = GetImagesService();
            var imgFile = TestFile;
            using var ms = new MemoryStream();
            await imgFile.OpenReadStream().CopyToAsync(ms);
            var sourceImageData = ms.ToArray();
            var imageId = await svc.AddImage(imgFile, "Test image name");

            var fullImage = await svc.FetchFullImageById(imageId);

            fullImage.Should().NotBeNull();
            fullImage.Length.Should().Be((int)imgFile.Length);
            fullImage.Should().BeEquivalentTo(sourceImageData);
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnZero()
        {
            var svc = GetImagesService();

            int count = await svc.GetImagesCount();

            count.Should().Be(0);
        }

        [Test]
        public async Task GetImagesCount_ShouldReturnThree()
        {
            var svc = GetImagesService();
            for (int i = 0; i < 3; i++)
            {
                await svc.AddImage(TestFile, $"Test image {i}");
            }

            int count = await svc.GetImagesCount();

            count.Should().Be(3);
        }

        [Test]
        public async Task DeleteImage_ShouldDeleteOneImage()
        {
            var context = CreateContext();
            var imagesId = new List<Guid>();
            var svc = GetImagesService(context);
            for (int i = 0; i < 3; i++)
            {
                var id = await svc.AddImage(TestFile, $"Test image {i}");
                imagesId.Add(id);
            }

            await svc.DeleteImage(imagesId[1]);

            bool imageExists = await context.Images.AnyAsync(i => i.Id == imagesId[1]);
            imageExists.Should().BeFalse();
            imageExists = await context.Images.AnyAsync(i => i.Id == imagesId[0]);
            imageExists.Should().BeTrue();
            imageExists = await context.Images.AnyAsync(i => i.Id == imagesId[2]);
            imageExists.Should().BeTrue();
        }

        [Test]
        public async Task UpdateImage_ShouldUpdateImageName()
        {
            var context = CreateContext();
            var svc = GetImagesService(context);
            var id = await svc.AddImage(TestFile, $"Test image");
            await Task.Delay(1000);
            string newName = Guid.NewGuid().ToString();

            await svc.UpdateImage(id, newName);
            var imgSaved = await context.Images.SingleOrDefaultAsync(i => i.Id == id);

            imgSaved.Should().NotBeNull();
            imgSaved.Name.Should().Be(newName);
            imgSaved.ModifyDate.Should().BeCloseTo(DateTime.Now, 500);
        }

        [Test]
        public async Task FetchImageSet_ShouldReturn3LastImages()
        {
            var context = CreateContext();
            var svc = GetImagesService(context);
            for (int i = 0; i < 10; i++)
            {
                await svc.AddImage(TestFile, $"Test image {i}");
                await Task.Delay(500);
            }

            var page = await svc.FetchImageSet(7, 5);

            var page2 = await context.Images.AsNoTracking().OrderByDescending(i => i.CreateDate).Skip(7).Take(5).ToListAsync();
            page.Should().NotBeNull();
            page.Should().HaveCount(3);
            page.Should().BeEquivalentTo(page2);
        }

        [Test]
        public async Task DeleteImage_WithEnabledSD_ShouldMarkAsDeleted()
        {
            var context = CreateContext();
            context.SoftDeleting = true;
            var svc = GetImagesService(context);
            (_, _, var image) = CreateEntities(context);

            await svc.DeleteImage(image.Id);

            var imageEntity = await context.Images.FindAsync(image.Id);
            imageEntity.Should().NotBeNull();
            imageEntity.Deleted.Should().BeTrue();

            var recordImage = context.RecordImages.IgnoreQueryFilters().SingleOrDefault(recImage => recImage.ImageId == image.Id);
            recordImage.Should().NotBeNull();
            recordImage?.Deleted.Should().BeTrue();
        }

        [Test]
        public async Task DeleteImage_WithDisabledSD_ShouldDeleteRecords()
        {
            var context = CreateContext();
            context.SoftDeleting = false;
            var svc = GetImagesService(context);
            (_, _, var image) = CreateEntities(context);

            await svc.DeleteImage(image.Id);

            var imageEntity = await context.Images.FindAsync(image.Id);
            imageEntity.Should().BeNull();

            var recordImage = context.RecordImages.IgnoreQueryFilters().FirstOrDefault(recImage => recImage.ImageId == image.Id);
            recordImage.Should().BeNull();
        }

        [Test]
        public async Task FetchImageById_ShouldReturnImageWithTempImage()
        {
            var context = CreateContext();
            var image = await CreateImageWithTempImage(context);
            var svc = GetImagesService(context);

            var img = await svc.FetchImageById(image.Id);

            img.TempImage.Should().NotBeNull();
            img.TempImage?.Modification.Should().Be(image.TempImage?.Modification);
            img.TempImage?.Data.Should().BeEquivalentTo(image.TempImage?.Data);
        }
    }
}
