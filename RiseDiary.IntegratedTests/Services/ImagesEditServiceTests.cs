using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class ImagesEditServiceTests : TestFixtureBase
{
    [Test]
    public async Task FetchChangedImage_ForUnchangedImage_ShouldThrowException()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgId = CreateImage(context);

        Func<Task> action = async () => _ = await svc.FetchChangedImage(imgId);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task FetchChangedImage_ForImageWithChanges_ShouldReturnChangedImageInfo()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var img = await CreateImageWithTempImage(context);

        var tempImage = await svc.FetchChangedImage(img.Id);

        tempImage.Should().NotBeNull();
        tempImage.Should().BeEquivalentTo(img.TempImage);
    }

    [Test]
    public async Task ImageHasChanges_ShouldReturnBool()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);
        var unchangedImgId = CreateImage(context);

        var hasChanged1 = await svc.ImageHasChanges(imgWithTemp.Id);
        var hasChanged2 = await svc.ImageHasChanges(unchangedImgId);

        hasChanged1.Should().BeTrue();
        hasChanged2.Should().BeFalse();
    }

    [Test]
    public async Task ReplaceImage_ForImageWithChanges_ShouldThrowException()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);

        Func<Task> action = async () => await svc.ReplaceImage(TestFile, imgWithTemp.Id);

        await action.Should().ThrowAsync<Exception>();
    }

    [Test]
    public async Task ReplaceImage_ShouldAddTempImage()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgId = CreateImage(context);

        await svc.ReplaceImage(TestFile, imgId, @"image/jpeg");

        var tmpImg = await context.TempImages.SingleOrDefaultAsync(i => i.SourceImageId == imgId);
        tmpImg.Should().NotBeNull();
        tmpImg.Modification.Should().Be("Замена изображения");
    }

    [Test]
    public async Task ReduceImageSize_ForImageWithChanges_ShouldThrowException()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);

        Func<Task> action = async () => await svc.ReduceImageSize(imgWithTemp.Id, 250);

        await action.Should().ThrowAsync<Exception>();
    }

    [Test]
    public async Task ReduceImageSize_ShouldAddTempImage()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgId = CreateImage(context);

        await svc.ReduceImageSize(imgId, 250);

        var tmpImg = await context.TempImages.SingleOrDefaultAsync(i => i.SourceImageId == imgId);
        tmpImg.Should().NotBeNull();
        tmpImg.Modification.Should().Be("Сжатие изображения");
    }

    [Test]
    public async Task ApplyChanges_OnUnchangedImage_ShouldThrowArgumentException()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgId = CreateImage(context);

        Func<Task> action = async () => await svc.ApplyChanges(imgId);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task ApplyChanges_ShouldReplaceImageData()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);
        var tmpImage = imgWithTemp.TempImage;

        await svc.ApplyChanges(imgWithTemp.Id);

        var savedImage = await context.Images.Include(i => i.FullImage).SingleOrDefaultAsync(i => i.Id == imgWithTemp.Id);
        savedImage.Should().NotBeNull();
        savedImage.Name.Should().Be(imgWithTemp.Name);
        savedImage.ModifyDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
        savedImage.CreateDate.Should().Be(imgWithTemp.CreateDate);
        savedImage.CameraModel.Should().Be(imgWithTemp.CameraModel);
        savedImage.Taken.Should().Be(imgWithTemp.Taken);
        savedImage.SizeByte.Should().Be(tmpImage?.SizeByte);
        savedImage.Height.Should().Be(tmpImage?.Height);
        savedImage.Width.Should().Be(tmpImage?.Width);
        savedImage.FullImage?.Data.Should().BeEquivalentTo(tmpImage?.Data);
    }

    [Test]
    public async Task ApplyChanges_ShouldRemoveTempImage()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);

        await svc.ApplyChanges(imgWithTemp.Id);

        bool tempImageExists = await context.TempImages.AnyAsync(i => i.SourceImageId == imgWithTemp.Id);
        tempImageExists.Should().BeFalse();
    }

    [Test]
    public async Task CreateNewImageFromChanged_ShouldCreateNewImageWithAllData()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);
        var modification = imgWithTemp!.TempImage!.Modification;
        var tempImageData = imgWithTemp!.TempImage!.Data;
        int imagesCountBefore = await context.Images.CountAsync();

        var newImageId = await svc.CreateNewImageFromChanged(imgWithTemp.Id);
        int imagesCountAfter = await context.Images.CountAsync();

        imagesCountAfter.Should().Be(imagesCountBefore + 1);
        var newImage = await context.Images.Include(i => i.FullImage).SingleOrDefaultAsync(i => i.Id == newImageId);
        newImage.Name.Should().Be($"{imgWithTemp.Name} ({modification})");
        newImage.ModifyDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
        newImage.FullImage.Should().NotBeNull();
        newImage.FullImage!.Data.Should().BeEquivalentTo(tempImageData);
    }

    [Test]
    public async Task CreateNewImageFromChanged_ShouldRemoveTempImage()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);

        var newImageId = await svc.CreateNewImageFromChanged(imgWithTemp.Id);

        bool tempImageExists = await context.TempImages.AnyAsync(i => i.SourceImageId == imgWithTemp.Id);
        tempImageExists.Should().BeFalse();
    }

    [Test]
    public async Task CreateNewImageFromChanged_ShoulNotChangeAnyImageData()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);
        var imageName = imgWithTemp.Name;
        var imageData = imgWithTemp!.FullImage!.Data;
        var imageModifyDate = imgWithTemp.ModifyDate;

        await svc.CreateNewImageFromChanged(imgWithTemp.Id);

        var image = await context.Images.Include(i => i.FullImage).SingleOrDefaultAsync(i => i.Id == imgWithTemp.Id);
        image.Name.Should().Be(imageName);
        image.FullImage!.Data.Should().BeEquivalentTo(imageData);
        image.ModifyDate.Should().Be(imageModifyDate);
    }

    [Test]
    public async Task DiscardChanges_ShoulNotChangeAnyImageData()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);
        var imageName = imgWithTemp.Name;
        var imageData = imgWithTemp!.FullImage!.Data;
        var imageModifyDate = imgWithTemp.ModifyDate;

        await svc.DiscardChanges(imgWithTemp.Id);

        var image = await context.Images.Include(i => i.FullImage).SingleOrDefaultAsync(i => i.Id == imgWithTemp.Id);
        image.Name.Should().Be(imageName);
        image.FullImage!.Data.Should().BeEquivalentTo(imageData);
        image.ModifyDate.Should().Be(imageModifyDate);
    }

    [Test]
    public async Task DiscardChanges_ShoulRemoveTempImage()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);

        await svc.DiscardChanges(imgWithTemp.Id);

        bool tempImageExists = await context.TempImages.AnyAsync(i => i.SourceImageId == imgWithTemp.Id);
        tempImageExists.Should().BeFalse();
    }

    [Test]
    public async Task RotateImage_ToLeft_ShouldAddTempImage()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgId = CreateImage(context, FullImage_512X341);

        await svc.RotateImage(imgId, Model.Turn.Left);

        var tmpImg = await context.TempImages.SingleOrDefaultAsync(i => i.SourceImageId == imgId);
        var img = await context.Images.SingleOrDefaultAsync(i => i.Id == imgId);
        img.Width.Should().Be(512);
        img.Height.Should().Be(341);
        tmpImg.Should().NotBeNull();
        tmpImg.Modification.Should().Be("Поворот на 90 градусов влево");
        tmpImg.Width.Should().Be(341);
        tmpImg.Height.Should().Be(512);
        //System.IO.File.WriteAllBytes(@"D:\file1.jpg", tmpImg.Data);
    }

    [Test]
    public async Task RotateImage_ToRight_ShouldAddTempImage()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgId = CreateImage(context, FullImage_512X341);

        await svc.RotateImage(imgId, Model.Turn.Right);

        var tmpImg = await context.TempImages.SingleOrDefaultAsync(i => i.SourceImageId == imgId);
        var img = await context.Images.SingleOrDefaultAsync(i => i.Id == imgId);
        img.Width.Should().Be(512);
        img.Height.Should().Be(341);
        tmpImg.Should().NotBeNull();
        tmpImg.Modification.Should().Be("Поворот на 90 градусов вправо");
        tmpImg.Width.Should().Be(341);
        tmpImg.Height.Should().Be(512);
        //System.IO.File.WriteAllBytes(@"D:\file2.jpg", tmpImg.Data);
    }
}
