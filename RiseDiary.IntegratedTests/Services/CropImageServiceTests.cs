using System;
using System.Drawing;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using static System.Convert;

namespace RiseDiary.IntegratedTests.Services;

[TestFixture]
internal class CropImageServiceTests : TestFixtureBase
{
    [Test]
    public async Task CreateScaledImagePreview_ImageDoesNotExists_ShouldThrowException()
    {
        var context = CreateContext();
        var svc = GetCropImageService(context);
        Func<Task> action = async () => await svc.CreateScaledImagePreview(Guid.NewGuid());

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task CreateScaledImagePreview_ImageWidthLessThanMaxScaledWidth_ShouldReturnAsIs()
    {
        var context = CreateContext();
        var svc = GetCropImageService(context);
        var imageId = CreateImage(context, FullImage_512X341);
        var image = GetTestImage(FullImage_512X341);

        var scaledImage = await svc.CreateScaledImagePreview(imageId);

        scaledImage.Coefficient.Should().Be(1.0);
        scaledImage.Image.Should().BeEquivalentTo(image.FullImage?.Data);
    }

    [Test]
    public async Task CreateScaledImagePreview_ImageHeightLessThanMaxScaledHeight_ShouldReturnAsIs()
    {
        var context = CreateContext();
        var svc = GetCropImageService(context);
        var imageId = CreateImage(context, FullImage_341X512);
        var image = GetTestImage(FullImage_341X512);

        var scaledImage = await svc.CreateScaledImagePreview(imageId);

        scaledImage.Coefficient.Should().Be(1.0);
        scaledImage.Image.Should().BeEquivalentTo(image.FullImage?.Data);
    }

    [Test]
    public async Task CreateScaledImagePreview_ImageWidthMoreThanMaxScaledWidth_ShouldScaleDown()
    {
        var context = CreateContext();
        var svc = GetCropImageService(context);
        var imageId = CreateImage(context, FullImage_1587X544);
        var image = GetTestImage(FullImage_341X512);
        var (_, h) = ImageHelper.GetImageSize(image.FullImage!.Data);

        var scaledImage = await svc.CreateScaledImagePreview(imageId);

        scaledImage.Coefficient.Should().BeGreaterThan(1.0);
        var (scaledW, scaledH) = ImageHelper.GetImageSize(scaledImage.Image);
        scaledW.Should().Be(800);
        scaledH.Should().BeLessThan(h);
    }

    [Test]
    public async Task CreateScaledImagePreview_ImageHeightMoreThanMaxScaledHeight_ShouldScaleDown()
    {
        var context = CreateContext();
        var svc = GetCropImageService(context);
        var imageId = CreateImage(context, FullImage_597X1300);
        var image = GetTestImage(FullImage_597X1300);
        var (w, _) = ImageHelper.GetImageSize(image.FullImage!.Data);

        var scaledImage = await svc.CreateScaledImagePreview(imageId);

        scaledImage.Coefficient.Should().BeGreaterThan(1.0);
        var (scaledW, scaledH) = ImageHelper.GetImageSize(scaledImage.Image);
        scaledH.Should().Be(600);
        scaledW.Should().BeLessThan(w);
    }

    [Test]
    public async Task CropImage_ForImageWithChanges_ShouldThrowException()
    {
        var context = CreateContext();
        var svc = GetImagesEditService(context);
        var imgWithTemp = await CreateImageWithTempImage(context);

        Func<Task> action = async () => await svc.ReplaceImage(TestFile, imgWithTemp.Id);

        await action.Should().ThrowAsync<Exception>();
    }

    [Test]
    public async Task CropImage_ShouldAddTempImage()
    {
        var context = CreateContext();
        var svc = GetCropImageService(context);
        var imageId = CreateImage(context, FullImage_1280X814);
        var rectangle = new Rectangle(20, 30, 350, 250);
        double coef = 1.2;

        await svc.CropImage(imageId, rectangle, coef);

        var tmpImg = await context.TempImages.SingleOrDefaultAsync(i => i.SourceImageId == imageId);
        tmpImg.Should().NotBeNull();
        tmpImg.Modification.Should().Be("Обрезка изображения");
        tmpImg.Width.Should().Be(ToInt32(rectangle.Width * coef));
        tmpImg.Height.Should().Be(ToInt32(rectangle.Height * coef));
        var (scaledW, scaledH) = ImageHelper.GetImageSize(tmpImg.Data);
        scaledW.Should().Be(ToInt32(rectangle.Width * coef));
        scaledH.Should().Be(ToInt32(rectangle.Height * coef));
    }
}
