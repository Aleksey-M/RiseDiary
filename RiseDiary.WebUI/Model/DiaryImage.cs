using System;
using System.Collections.Generic;
using System.Globalization;

namespace RiseDiary.Model
{
    public sealed class DiaryImage : IDeletedEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        public byte[] Thumbnail { get; set; } = Array.Empty<byte>();

        public int Width { get; set; }

        public int Height { get; set; }

        public int SizeByte { get; set; }

        public bool Deleted { get; set; }

        public DateTime? Taken { get; set; }

        public string? CameraModel { get; set; }

        public DiaryImageFull? FullImage { get; set; }

        public TempImage? TempImage { get; set; }

        public ICollection<DiaryRecordImage> RecordsRefs { get; private set; } = null!;
    }

    public sealed class DiaryImageFull
    {
        public Guid Id { get; set; }

        public Guid ImageId { get; set; }

        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    public sealed class TempImage
    {
        public Guid Id { get; set; }

        public Guid SourceImageId { get; set; }

        public string Modification { get; set; } = "";

        public byte[] Data { get; set; } = Array.Empty<byte>();   
        
        public int Width { get; set; }

        public int Height { get; set; }

        public int SizeByte { get; set; }
    }

    public static class ImageExtensions
    {
        public static int GetBiggestImageDimm(this DiaryImage image) => image.Width > image.Height ? image.Width : image.Height;

        public static string GetBase64Thumbnail(this DiaryImage image) => Convert.ToBase64String(image.Thumbnail);

        public static string GetSizeKbString(this DiaryImage image) => Math.Round(image.SizeByte / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Kb";

        public static string GetSizeKbString(this TempImage tempImage) => Math.Round(tempImage.SizeByte / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Kb";
    }

    public sealed class ScaledImagePreview
    {
        public double Coefficient { get; }

        public byte[] Image { get; }

        public ScaledImagePreview(double coefficient, byte[] image)
        {
            Coefficient = coefficient;
            Image = image;
        }
    }

}
