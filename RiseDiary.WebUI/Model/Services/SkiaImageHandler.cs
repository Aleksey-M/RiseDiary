using ExifLibrary;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;

namespace RiseDiary.Model.Services
{
    internal abstract class SkiaImageHandler
    {
        protected byte[] ScaleImage(byte[] data, int imageQuality, int maxSizePx)
        {
            using var bitmap = SKBitmap.Decode(data);
            if (bitmap.ColorType != SKImageInfo.PlatformColorType)
            {
                bitmap.CopyTo(bitmap, SKImageInfo.PlatformColorType);
            }
            int width, height;
            if (bitmap.Width >= bitmap.Height)
            {
                width = maxSizePx;
                height = Convert.ToInt32(bitmap.Height / (double)bitmap.Width * maxSizePx);
            }
            else
            {
                height = maxSizePx;
                width = Convert.ToInt32(bitmap.Width / (double)bitmap.Height * maxSizePx);
            }
            var imageInfo = new SKImageInfo(width, height);
            using var thumbnail = bitmap.Resize(imageInfo, SKFilterQuality.Medium);
            using var img = SKImage.FromBitmap(thumbnail);
            using var jpeg = img.Encode(SKEncodedImageFormat.Jpeg, imageQuality);
            using var memoryStream = new MemoryStream();
            jpeg.AsStream().CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        protected byte[] CropImage(byte[] data, int left, int top, int width, int height, int imageQuality)
        {
            (int actualWidth, int actualHeight) = GetImageSize(data);
            width = width > actualWidth ? actualWidth : width;
            height = height > actualHeight ? actualHeight : height;
            top = top >= 0 ? top : 0;
            top = top < actualHeight ? top : 0;
            left = left >= 0 ? left : 0;
            left = left < actualWidth ? left : 0;

            using var bitmap = SKBitmap.Decode(data);
            using var img = SKImage.FromBitmap(bitmap);
            using var cropped = img.Subset(SKRectI.Create(left, top, width, height));
            using var jpeg = cropped.Encode(SKEncodedImageFormat.Jpeg, imageQuality);
            using var memoryStream = new MemoryStream();
            jpeg.AsStream().CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        protected (int width, int height) GetImageSize(byte[] data)
        {
            using var bitmap = SKBitmap.Decode(data);
            return (bitmap.Width, bitmap.Height);
        }

        protected (DateTime? taken, string? cameraModel) GetMetadataFromPhoto(byte[] image)
        {
            ImageFile file;
            using var stream = new MemoryStream(image);
            try
            {
                file = ImageFile.FromStream(stream);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return (null, null);
            }
            var prop = file.Properties.FirstOrDefault(p => p.Tag == ExifTag.DateTimeOriginal);
            DateTime? taken = prop != null ? (DateTime)prop.Value : (DateTime?)null;

            var model = file.Properties.FirstOrDefault(p => p.Tag == ExifTag.Model)?.Value?.ToString() ?? "";
            var make = file.Properties.FirstOrDefault(p => p.Tag == ExifTag.Make)?.Value.ToString() ?? "";
            string? cameraModel = model.Contains(make, StringComparison.OrdinalIgnoreCase) ? model : make + " " + model;
            cameraModel = string.IsNullOrWhiteSpace(cameraModel) ? null : cameraModel.Trim();

            return (taken, cameraModel);
        }

        protected byte[] RotateImage(byte[] image, float degrees, int imageQuality)
        {
            using var bitmap = SKBitmap.Decode(image);
            var rotated = new SKBitmap(width: bitmap.Height, height: bitmap.Width);

            using (var surface = new SKCanvas(rotated))
            {
                var (dx, dy) = degrees switch
                {
                    > 0 and <= 45 => (0, 0),
                    > 45 and <= 135 => (rotated.Width, 0),
                    > 135 and <= 225 => (rotated.Width, rotated.Height),
                    > 225 and <= 315 => (0, rotated.Height),
                    _ => (0, 0)
                };

                surface.Translate(dx, dy);
                surface.RotateDegrees(degrees);
                surface.DrawBitmap(bitmap, 0, 0);
            }           

            using var jpeg = rotated.Encode(SKEncodedImageFormat.Jpeg, imageQuality);
            using var memoryStream = new MemoryStream();
            jpeg.AsStream().CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
