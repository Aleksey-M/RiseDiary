using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RiseDiary.Model;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RiseDiary.IntegratedTests.Services;

internal class ImagesServiceTests : TestFixtureBase
{

    [Test]
    public async Task AddImage_ShouldNotThrowException()
    {
        var svc = GetImagesService();

        var imageId = await svc.AddImage(TestFile);

        imageId.Should().NotBeEmpty();
    }


    [Test]
    public async Task AddImage_WithBiggestSizeRestriction_ShouldReduseSize()
    {
        var context = CreateContext();
        var svc = GetImagesService(context);

        var imageId = await svc.AddImage(TestFile, newBiggestDimensionSize: 250);

        imageId.Should().NotBeEmpty();
        var image = await context.Images.SingleAsync(i => i.Id == imageId);
        image.Height.Should().BeLessOrEqualTo(250);
        image.Width.Should().BeLessOrEqualTo(250);
    }

    [Test]
    public async Task FetchImageById_WithNotExistingId_ShouldThrowException()
    {
        var svc = GetImagesService();
        Func<Task> action = async () => _ = await svc.FetchImageById(Guid.NewGuid());

        await action.Should().ThrowAsync<ImageNotFoundException>();
    }

    [Test]
    public async Task FetchImageById_ShouldReturnAddedImage()
    {
        var svc = GetImagesService();
        var imageId = await svc.AddImage(TestFile, "Test image name");

        var image = await svc.FetchImageById(imageId);

        image.Should().NotBeNull();
        image.Name.Should().Be("Test image name");
        image.CreateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
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
    public async Task GetImagesCount_WithFilter_ShouldReturnZero()
    {
        var svc = GetImagesService();

        int count = await svc.GetImagesCount("Filter by name");

        count.Should().Be(0);
    }

    [Test]
    public async Task GetImagesCount_WithFilter_ShouldReturnOne()
    {
        var svc = GetImagesService();
        for (int i = 0; i < 3; i++)
        {
            await svc.AddImage(TestFile, $"Test image {i}");
        }

        int count = await svc.GetImagesCount("ImaGe 1");

        count.Should().Be(1);
    }

    [Test]
    public async Task GetImagesCount_WithRecordId_ShouldExcludeAlreadyAddedImage()
    {
        var (recordId, imagesIds, context) = await CreateRecordAnd4Images();
        var imagesService = GetImagesService(context);
        var recordsImagesService = GetRecordsImagesService(context);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);

        int count = await imagesService.GetImagesCount(recordId: recordId);

        count.Should().Be(3);
        imagesIds.Should().HaveCount(4);
    }

    [Test]
    public async Task GetImagesCount_WithRecordIdAndAllAddedImages_ShouldReturnZero()
    {
        var (recordId, imagesIds, context) = await CreateRecordAnd4Images();
        var imagesService = GetImagesService(context);
        var recordsImagesService = GetRecordsImagesService(context);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[0]);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[2]);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[3]);

        int count = await imagesService.GetImagesCount(recordId: recordId);

        count.Should().Be(0);
        imagesIds.Should().HaveCount(4);
    }

    [Test]
    public async Task GetImagesCount_WithFilterAndRecordId_ShouldReturnFilteredAndWithoutAlreadyAddedImage()
    {
        var (recordId, imagesIds, context) = await CreateRecordAnd4Images();
        var imagesService = GetImagesService(context);
        var recordsImagesService = GetRecordsImagesService(context);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[0]);

        int count = await imagesService.GetImagesCount(imageNameFilter: "1", recordId: recordId);

        count.Should().Be(1);
        imagesIds.Should().HaveCount(4);
    }

    [Test]
    public async Task GetImagesCount_WithoutMatch_ShouldReturnZero()
    {
        var svc = GetImagesService();
        for (int i = 0; i < 3; i++)
        {
            await svc.AddImage(TestFile, $"Test image {i}");
        }

        int count = await svc.GetImagesCount("ImaGe 4");

        count.Should().Be(0);
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
        string newName = Guid.NewGuid().ToString();

        await svc.UpdateImage(id, newName);
        var imgSaved = await context.Images.SingleOrDefaultAsync(i => i.Id == id);

        imgSaved.Should().NotBeNull();
        imgSaved.Name.Should().Be(newName);
        imgSaved.ModifyDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
    }

    [Test]
    public async Task FetchImageSet_ShouldReturn3LastImages()
    {
        var context = CreateContext();
        var svc = GetImagesService(context);
        for (int i = 0; i < 10; i++)
        {
            await svc.AddImage(TestFile, $"Test image {i}");
        }

        var page = await svc.FetchImageSet(7, 5);

        var page2 = await context.Images.OrderByDescending(i => i.CreateDate).Skip(7).Take(5).ToListAsync();
        page.Should().NotBeNull();
        page.Should().HaveCount(3);
        page.Should().BeEquivalentTo(page2);
    }

    [Test]
    public async Task FetchImageSet_WithFilter_ShouldReturn1LastImage()
    {
        var context = CreateContext();
        var svc = GetImagesService(context);
        for (int i = 0; i < 10; i++)
        {
            await svc.AddImage(TestFile, $"Test image {i}");
        }

        var page = await svc.FetchImageSet(0, 5, "IMAGE 7");

        var page2 = await context.Images
            .Include(x => x.FullImage)
            .Where(x => x.Name.ToLower().Trim().Contains("IMAGE 7".ToLower()))
            .OrderByDescending(i => i.CreateDate)
            .Skip(0)
            .Take(5)
            .ToListAsync();

        page.Should().NotBeNull();
        page.Should().HaveCount(1);
        page.Should().BeEquivalentTo(page2);
    }

    [Test]
    public async Task FetchImageSet_WithFilter_ShouldReturnSecondPage()
    {
        var context = CreateContext();
        var svc = GetImagesService(context);
        for (int i = 0; i < 30; i++)
        {
            await svc.AddImage(TestFile, $"Test image {i}");
        }

        var page = await svc.FetchImageSet(10, 10, "2");

        var page2 = await context.Images
            .Include(x => x.FullImage)
            .Where(x => x.Name.Contains("2"))
            .OrderByDescending(i => i.CreateDate)
            .Skip(10)
            .Take(10)
            .ToListAsync();

        page.Should().NotBeNull();
        page.Should().HaveCount(2);
        page.Select(x => x.Name).ToList().Should().BeEquivalentTo(new List<string> { "Test image 12", "Test image 2" });
        page.Should().BeEquivalentTo(page2);
    }

    [Test]
    public async Task FetchImageSet_WithRecordId_ShouldExcludeAlreadyAddedImage()
    {
        var (recordId, imagesIds, context) = await CreateRecordAnd4Images();
        var imagesService = GetImagesService(context);
        var recordsImagesService = GetRecordsImagesService(context);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);

        var page = await imagesService.FetchImageSet(0, 4, recordId: recordId);

        page.Should().NotBeNull();
        page.Should().HaveCount(3);
        page.Select(i => i.Id).Should().BeEquivalentTo(imagesIds.Where(i => i != imagesIds[1]));
    }

    [Test]
    public async Task FetchImageSet_WithRecordIdAndAllAddedImages_ShouldReturnEmptyList()
    {
        var (recordId, imagesIds, context) = await CreateRecordAnd4Images();
        var imagesService = GetImagesService(context);
        var recordsImagesService = GetRecordsImagesService(context);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[0]);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[1]);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[2]);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[3]);

        var page = await imagesService.FetchImageSet(0, 4, recordId: recordId);

        page.Should().NotBeNull();
        page.Should().HaveCount(0);
        imagesIds.Should().HaveCount(4);
    }

    [Test]
    public async Task FetchImageSet_WithFilterAndRecordId_ShouldReturnFilteredAndWithoutAlreadyAddedImage()
    {
        var (recordId, imagesIds, context) = await CreateRecordAnd4Images();
        var imagesService = GetImagesService(context);
        var recordsImagesService = GetRecordsImagesService(context);
        await recordsImagesService.AddRecordImage(recordId, imagesIds[0]);

        var page = await imagesService.FetchImageSet(0, 4, imageNameFilter: "1", recordId: recordId);

        page.Should().NotBeNull();
        page.Should().HaveCount(1);
        page.Select(i => i.Id).Should().BeEquivalentTo(new Guid[] { imagesIds[2] });
        imagesIds.Should().HaveCount(4);
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
    public async Task DeleteImage_WithDisabledSD_ShouldDeleteImageWithRecordImage()
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
    public async Task DeleteImage_ShouldBeReorderedAllRecordsImages()
    {
        var context = CreateContext();
        var (rec1Id, rec2Id, rec3Id, imgId) = await CreateRecordsWithLinkedImage(context);
        var svc = GetImagesService(context);

        await svc.DeleteImage(imgId);

        var rec1ImagesOrder = await context.RecordImages.Where(x => x.RecordId == rec1Id).Select(x => x.Order).ToListAsync();
        rec1ImagesOrder.Should().BeEquivalentTo(new int[] { 1, 2, 3 });

        var rec2ImagesOrder = await context.RecordImages.Where(x => x.RecordId == rec2Id).Select(x => x.Order).ToListAsync();
        rec2ImagesOrder.Should().BeEquivalentTo(new int[] { 1, 2, 3 });

        var rec3ImagesOrder = await context.RecordImages.Where(x => x.RecordId == rec3Id).Select(x => x.Order).ToListAsync();
        rec3ImagesOrder.Should().BeEquivalentTo(new int[] { 1, 2, 3 });
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
