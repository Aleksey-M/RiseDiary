//using RiseDiary.WebUI.Data;
//using SkiaSharp;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Globalization;
//using System.IO;
//using System.Linq;

//#pragma warning disable CA1303 // Do not pass literals as localized parameters
//namespace RiseDiary.Model
//{
//    public interface IDeletedEntity
//    {
//        bool Deleted { get; set; }
//    }

//    public class DiaryScope : IDeletedEntity
//    {
//        public DiaryScope(Guid id, string scopeName)
//        {
//            Id = id;
//            ScopeName = scopeName;
//            Themes = new List<DiaryTheme>();
//        }

//        public Guid Id { get; private set; }
//        public string ScopeName { get; set; }
//        public bool Deleted { get; set; }
//        public ICollection<DiaryTheme> Themes { get; private set; }
//    }

//    public class DiaryTheme : IDeletedEntity
//    {
//        public DiaryTheme(Guid id, Guid scopeId, string themeName, bool actual = false)
//        {
//            Id = id;
//            ScopeId = scopeId;
//            ThemeName = themeName;
//            Actual = actual;
//            RecordsRefs = new List<DiaryRecordTheme>();
//        }

//        public DiaryTheme(Guid id, DiaryScope scope, string themeName, bool actual = false)
//        {
//            Id = id;
//            Scope = scope;            
//            ThemeName = themeName;
//            Actual = actual;
//            RecordsRefs = new List<DiaryRecordTheme>();
//        }

//        public Guid Id { get; private set; }
//        public string ThemeName { get; set; }
//        public bool Actual { get; set; }
//        public bool Deleted { get; set; }

//        public Guid ScopeId { get; private set; }
//        public DiaryScope? Scope { get; private set; }
//        public ICollection<DiaryRecordTheme> RecordsRefs { get; private set; }
//    }

//    public class DiaryRecordTheme : IDeletedEntity
//    {
//        public DiaryRecordTheme(Guid themeId, Guid recordId)
//        {
//            ThemeId = themeId;
//            RecordId = recordId;
//        }

//        public DiaryRecordTheme(DiaryTheme theme, DiaryRecord record)
//        {
//            Theme = theme;            
//            Record = record;
//        }

//        public Guid ThemeId { get; private set; }
//        public Guid RecordId { get; private set; }
//        public bool Deleted { get; set; }

//        public DiaryTheme? Theme { get; private set; }
//        public DiaryRecord? Record { get; private set; }
//    }

//    public class DiaryImage : IDeletedEntity
//    {
//        //private DiaryImage(Guid id, string name, IEnumerable<byte> thumbnail) 
//        //{
//        //    Id = id;
//        //    Name = name;
//        //    Thumbnail = thumbnail;
//        //    RecordsRefs = new List<DiaryRecordImage>();
//        //}

//        //public DiaryImage(Guid id, string name, DateTime createDate, DateTime modifyDate, IEnumerable<byte> thumbnail, int width, int height, int sizeByte, bool deleted)
//        //{
//        //    Id = id;
//        //    Name = name;
//        //    CreateDate = createDate;
//        //    ModifyDate = modifyDate;
//        //    Thumbnail = thumbnail;
//        //    Width = width;
//        //    Height = height;
//        //    SizeByte = sizeByte;
//        //    Deleted = deleted;
//        //    RecordsRefs = new List<DiaryRecordImage>();
//        //}        

//        public Guid Id { get; private set; }
//        public string Name { get; set; }
//        public DateTime CreateDate { get; private set; }
//        public DateTime ModifyDate { get; private set; }
//        //private byte[] _thumbnail;        
//        public int Width { get; private set; }
//        public int Height { get; private set; }
//        public int SizeByte { get; private set; }
//        public bool Deleted { get; set; }

//        public DiaryImageFull? FullImage { get; private set; }
//        public TempImage? TempImage { get; private set; }
//        public ICollection<DiaryRecordImage> RecordsRefs { get; private set; }

//        public IEnumerable<byte> Thumbnail { get; private set; }
//        public string Base64Thumbnail => Convert.ToBase64String(Thumbnail.ToArray());
//        public string SizeKbString => Math.Round(SizeByte / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Kb";

//        public const int ThumbnailSize = 150;

//        public static List<byte> ScaleImage(List<byte> data, int maxSizePx = ThumbnailSize)
//        {
//            if (data == null) throw new ArgumentNullException(nameof(data));

//            using var bitmap = SKBitmap.Decode(data.ToArray());
//            if (bitmap.ColorType != SKImageInfo.PlatformColorType)
//            {
//                bitmap.CopyTo(bitmap, SKImageInfo.PlatformColorType);
//            }
//            int width, height;
//            if (bitmap.Width >= bitmap.Height)
//            {
//                width = maxSizePx;
//                height = Convert.ToInt32(bitmap.Height / (double)bitmap.Width * maxSizePx);
//            }
//            else
//            {
//                height = maxSizePx;
//                width = Convert.ToInt32(bitmap.Width / (double)bitmap.Height * maxSizePx);
//            }

//            var imageInfo = new SKImageInfo(width, height);
//            using var thumbnail = bitmap.Resize(imageInfo, SKFilterQuality.Medium);
//            using var img = SKImage.FromBitmap(thumbnail);
//            using var jpeg = img.Encode(SKEncodedImageFormat.Jpeg, 90);
//            using var memoryStream = new MemoryStream();
//            jpeg.AsStream().CopyTo(memoryStream);
//            return memoryStream.ToArray().ToList();
//        }

//        public static List<byte> CropImage(List<byte> data, int left, int top, int width, int height)
//        {
//            if (data == null) throw new ArgumentNullException(nameof(data));

//            (int actualWidth, int actualHeight) = ImageSize(data);
//            width = width > actualWidth ? actualWidth : width;
//            height = height > actualHeight ? actualHeight : height;
//            top = top >= 0 ? top : 0;
//            top = top < actualHeight ? top : 0;
//            left = left >= 0 ? left : 0;
//            left = left < actualWidth ? left : 0;

//            using var bitmap = SKBitmap.Decode(data.ToArray());
//            using var img = SKImage.FromBitmap(bitmap);
//            using var cropped = img.Subset(SKRectI.Create(left, top, width, height));
//            using var jpeg = cropped.Encode(SKEncodedImageFormat.Jpeg, 90);
//            using var memoryStream = new MemoryStream();
//            jpeg.AsStream().CopyTo(memoryStream);
//            return memoryStream.ToArray().ToList();
//        }

//        public static (int width, int height) ImageSize(List<byte> data)
//        {
//            if (data == null) throw new ArgumentNullException(nameof(data));

//            using var bitmap = SKBitmap.Decode(data.ToArray());
//            return (bitmap.Width, bitmap.Height);
//        }

//        //public static DiaryImage Create(string imageName, List<byte> fullSizeImageData)
//        //{
//        //    if (string.IsNullOrWhiteSpace(imageName)) throw new ArgumentException("Image Name should not be empty!");
//        //    if (fullSizeImageData == null || fullSizeImageData.Count == 0) throw new ArgumentException("Can't create image without data");

//        //    var newImageId = Guid.NewGuid();
//        //    var image = new DiaryImage(newImageId, imageName, ScaleImage(fullSizeImageData.ToList()).ToArray())
//        //    {
//        //        CreateDate = DateTime.Now,
//        //        ModifyDate = DateTime.Now,
//        //        SizeByte = fullSizeImageData.Count,
//        //        FullImage = new DiaryImageFull(Guid.NewGuid(), newImageId, fullSizeImageData.ToList())
//        //    };

//        //    (image.Width, image.Height) = ImageSize(fullSizeImageData);

//        //    return image;
//        //}

//        public void MakeScailing(int maxSizePx)
//        {
//            if (TempImage != null) throw new InvalidOperationException("Previous updating of the image is not finished");
//            if (maxSizePx < 100) throw new ArgumentException("New max size is too small:" + maxSizePx);
//            if (FullImage == null) throw new InvalidOperationException("Full image is not loaded");

//            var result = ScaleImage(FullImage.Data.ToList(), maxSizePx).ToList();
//            var (w, h) = ImageSize(result);
//            TempImage = new TempImage(Guid.NewGuid(), Id, "Сжатие изображения", result, w, h, result.Count);
//        }

//        public void MakeCropping(int left, int top, int width, int height)
//        {
//            if (TempImage != null) throw new InvalidOperationException("Previous updating of the image is not finished");
//            if (FullImage == null) throw new InvalidOperationException("Full image is not loaded");

//            var result = CropImage(FullImage.Data.ToList(), left, top, width, height);
//            var (w, h) = ImageSize(result.ToList());

//            TempImage = new TempImage(Guid.NewGuid(), Id, "Обрезка изображения", result, w, h, result.Count);
//        }

//        public void MakeReplacing(List<byte> newFullImage)
//        {
//            if (TempImage != null) throw new InvalidOperationException("Previous updating of the image is not finished");
//            if (FullImage == null) throw new InvalidOperationException("Full image is not loaded");
//            if (newFullImage == null || newFullImage.Count == 0) throw new ArgumentException("Can't create image without data");

//            var (w, h) = ImageSize(newFullImage);

//            TempImage  = new TempImage(Guid.NewGuid(), Id, "Замена изображения", newFullImage, w, h, newFullImage.Count);
//        }

//        public void ApplyChanges()
//        {
//            if (TempImage == null) return;
//            if (Deleted) throw new ArgumentException("Image is deleted. Can't update deleted image");
//            if (FullImage == null) throw new InvalidOperationException("Full image is not loaded");

//            var arr = TempImage.Data.ToList();
//            Thumbnail = ScaleImage(arr).ToArray();
//            ModifyDate = DateTime.Now;
//            SizeByte = arr.Count;
//            (Width, Height) = ImageSize(arr.ToList());

//            FullImage = new DiaryImageFull(FullImage.Id, Id, arr.ToList());
//            TempImage = null;
//        }

//        public void UpdateImageName(string imageNewName)
//        {
//            if (string.IsNullOrWhiteSpace(imageNewName)) throw new ArgumentException("Image name should be not empty");

//            Name = imageNewName;
//            ModifyDate = DateTime.Now;
//        }
//    }

//    public class DiaryImageFull
//    {
//        public DiaryImageFull(Guid id, Guid imageId, List<byte> data)
//        {
//            Id = id;
//            ImageId = imageId;
//            _data = data;          
//        }

//        public Guid Id { get; private set; }
//        public Guid ImageId { get; private set; }
//        private readonly List<byte> _data;
//        public List<byte> Data => _data.ToList();
//    }

//    public class TempImage
//    {
//        public TempImage(Guid id, Guid sourceImageId, string modification, List<byte> data, int width, int height, int sizeByte)
//        {
//            Id = id;
//            SourceImageId = sourceImageId;
//            Modification = modification;
//            _data = data;
//            Width = width;
//            Height = height;
//            SizeByte = sizeByte; 
//        }

//        public Guid Id { get; private set; }
//        public Guid SourceImageId { get; private set; }
//        public string Modification { get; private set; }
//        private readonly List<byte> _data;
//        public List<byte> Data => _data.ToList();
//        public string SizeKbString => Math.Round(SizeByte / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Kb";
//        public int Width { get; set; }
//        public int Height { get; set; }
//        public int SizeByte { get; set; }
//    }

//    public class DiaryRecordImage : IDeletedEntity
//    {
//        public Guid ImageId { get; set; }
//        public Guid RecordId { get; set; }
//        public bool Deleted { get; set; }

//        public DiaryImage? Image { get; set; }
//        public DiaryRecord? Record { get; set; }
//    }

//    public class DiaryRecord : IDeletedEntity
//    {
//        public Guid Id { get; set; }
//        private DateTime _recordDate;
//        public DateTime Date { get => _recordDate; set => _recordDate = value.Date; }
//        public DateTime CreateDate { get; set; }
//        public DateTime ModifyDate { get; set; }
//        public string Name { get; set; } = string.Empty;
//        public string Text { get; set; } = string.Empty;
//        public bool Deleted { get; set; }

//        public string RecordNameDisplay => string.IsNullOrWhiteSpace(Name) ? "[ПУСТО]" : Name;

//        public string RecordTextShort
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(Text)) return "[ПУСТО]";
//                return Text.Length < 35 ? Text : Text.Substring(0, 35) + "[...]";
//            }
//        }

//        public ICollection<Cogitation> Cogitations { get; set; } = new List<Cogitation>();
//        public ICollection<DiaryRecordTheme> ThemesRefs { get; set; } = new List<DiaryRecordTheme>();
//        public ICollection<DiaryRecordImage> ImagesRefs { get; set; } = new List<DiaryRecordImage>();
//    }

//    public class Cogitation : IDeletedEntity
//    {
//        public Guid Id { get; set; }
//        public Guid RecordId { get; set; }
//        public DateTime Date { get; set; }
//        public string Text { get; set; } = string.Empty;
//        public bool Deleted { get; set; }

//        public DiaryRecord? Record { get; set; }
//    }

//    public static class AppSettingsKeys
//    {
//        public static string DatesScopeId => "ImportantDaysScopeId";
//        public static string DatesDisplayRange => "ImportantDaysDisplayRange";
//    }

//    public class AppSetting
//    {
//        public string Key { get; set; } = string.Empty;
//        public string Value { get; set; } = string.Empty;
//        public DateTime ModifiedDate { get; set; }
//    }
//}
//#pragma warning restore CA1303 // Do not pass literals as localized parameters