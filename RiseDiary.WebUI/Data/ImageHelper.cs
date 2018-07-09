﻿using RiseDiary.Model;
using SkiaSharp;
using System;
using System.IO;

namespace RiseDiary.WebUI.Data
{
    public static class ImageHelper
    {
        public const int ThumbnailSize = 150;

        public static byte[] ScaleImage(byte[] data, int maxSizePx = ThumbnailSize)
        {
            using (var bitmap = SKBitmap.Decode(data))
            {
                int width, height;
                if(bitmap.Width >= bitmap.Height)
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
                using (var thumbnail = bitmap.Resize(imageInfo, SKBitmapResizeMethod.Lanczos3))
                using (var img = SKImage.FromBitmap(thumbnail))
                using (var jpeg = img.Encode(SKEncodedImageFormat.Jpeg, 90))
                using(var memoryStream = new MemoryStream())
                {
                    jpeg.AsStream().CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public static byte[] CropImage(byte[] data, int left, int top, int width, int height)
        {
            (int actualWidth, int actualHeight) = ImageSize(data);
            width = width > actualWidth ? actualWidth : width;
            height = height > actualHeight ? actualHeight : height;
            top = top >= 0 ? top : 0;
            top = top < actualHeight ? top : 0;
            left = left >= 0 ? left : 0;
            left = left < actualWidth ? left : 0;

            using (var bitmap = SKBitmap.Decode(data))
            using (var img = SKImage.FromBitmap(bitmap))
            using (var cropped = img.Subset(SKRectI.Create(left, top, width, height)))
            using (var jpeg = cropped.Encode(SKEncodedImageFormat.Jpeg, 90))
            using (var memoryStream = new MemoryStream())
            {
                jpeg.AsStream().CopyTo(memoryStream);
                return memoryStream.ToArray();
            }            
        }

        public static (int width, int height) ImageSize(byte[] data)
        {
            using (var bitmap = SKBitmap.Decode(data))
            {
                return (bitmap.Width, bitmap.Height);
            }
        }

        public static TempImage ScaleImage(DiaryImage image, byte[] fullImage, int maxSizePx)
        {
            var result = ScaleImage(fullImage, maxSizePx);
            var temp = new TempImage
            {
                SourceImageId = image.Id,
                Modification = "Сжатие изображения",
                Data = result,
                SizeByte = result.Length
            };
            (temp.Width, temp.Height) = ImageSize(result);
            return temp;
        }

        public static TempImage CropImage(DiaryImage image, byte[] fullImage, int left, int top, int width, int height)
        {            
            var result = CropImage(fullImage, left, top, width, height);
            var temp = new TempImage
            {
                SourceImageId = image.Id,
                Modification = "Обрезка изображения",
                Data = result,
                SizeByte = result.Length
            };
            (temp.Width, temp.Height) = ImageSize(result);
            return temp;
        }

        public static TempImage ReplaceImage(DiaryImage image, byte[] fullImage)
        {
            var temp = new TempImage
            {
                SourceImageId = image.Id,
                Modification = "Замена изображения",
                Data = fullImage,
                SizeByte = fullImage.Length
            };
            (temp.Width, temp.Height) = ImageSize(fullImage);
            return temp;
        }
    }
}
