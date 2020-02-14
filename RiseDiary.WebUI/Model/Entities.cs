using System;
using System.Collections.Generic;
using System.Globalization;

namespace RiseDiary.Model
{
    public interface IDeletedEntity
    {
        bool Deleted { get; set; }
    }

    public class DiaryScope : IDeletedEntity
    {
        public Guid Id { get; set; }
        public string ScopeName { get; set; } = string.Empty;
        public bool Deleted { get; set; }

        public ICollection<DiaryTheme> Themes { get; private set; } = new List<DiaryTheme>();
    }

    public class DiaryTheme : IDeletedEntity
    {
        public Guid Id { get; set; }
        public Guid ScopeId { get; set; }
        public string ThemeName { get; set; } = string.Empty;
        public bool Actual { get; set; }
        public bool Deleted { get; set; }

        public DiaryScope? Scope { get; set; }
        public ICollection<DiaryRecordTheme> RecordsRefs { get; private set; } = new List<DiaryRecordTheme>();
    }

    public class DiaryRecordTheme : IDeletedEntity
    {
        public Guid ThemeId { get; set; }
        public Guid RecordId { get; set; }
        public bool Deleted { get; set; }

        public DiaryTheme? Theme { get; set; }
        public DiaryRecord? Record { get; set; }
    }

    public class DiaryImage : IDeletedEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Thumbnail { get; set; } = Array.Empty<byte>();
#pragma warning restore CA1819 // Properties should not return arrays
        public string Base64Thumbnail => Convert.ToBase64String(Thumbnail);
        public string SizeKbString => Math.Round(SizeByte / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Kb";
        public int Width { get; set; }
        public int Height { get; set; }
        public int SizeByte { get; set; }
        public bool Deleted { get; set; }

        public DiaryImageFull? FullImage { get; set; }
        public TempImage? TempImage { get; set; }
        public ICollection<DiaryRecordImage> RecordsRefs { get; private set; } = new List<DiaryRecordImage>();
    }

    public class DiaryImageFull
    {
        public Guid Id { get; set; }
        public Guid ImageId { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; } = Array.Empty<byte>();
#pragma warning restore CA1819 // Properties should not return arrays

        public DiaryImage? DiaryImage { get; set; }
    }

    public class TempImage
    {
        public Guid Id { get; set; }
        public Guid SourceImageId { get; set; }
        public string Modification { get; set; } = string.Empty;
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; set; } = Array.Empty<byte>();
#pragma warning restore CA1819 // Properties should not return arrays
        public string SizeKbString => Math.Round(SizeByte / 1024f, 2).ToString(CultureInfo.InvariantCulture) + " Kb";
        public int Width { get; set; }
        public int Height { get; set; }
        public int SizeByte { get; set; }

        public DiaryImage? DiaryImage { get; private set; }
    }

    public class DiaryRecordImage : IDeletedEntity
    {
        public Guid ImageId { get; set; }
        public Guid RecordId { get; set; }
        public bool Deleted { get; set; }

        public DiaryImage? Image { get; set; }
        public DiaryRecord? Record { get; set; }
    }

    public class DiaryRecord : IDeletedEntity
    {
        public Guid Id { get; set; }
        private DateTime _recordDate;
        public DateTime Date { get => _recordDate; set => _recordDate = value.Date; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Deleted { get; set; }

        public string RecordNameDisplay => string.IsNullOrWhiteSpace(Name) ? "[ПУСТО]" : Name;

        public string RecordTextShort
        {
            get
            {
                if (string.IsNullOrEmpty(Text)) return "[ПУСТО]";
                return Text.Length < 35 ? Text : Text.Substring(0, 35) + "[...]";
            }
        }

        public ICollection<Cogitation> Cogitations { get; private set; } = new List<Cogitation>();
        public ICollection<DiaryRecordTheme> ThemesRefs { get; private set; } = new List<DiaryRecordTheme>();
        public ICollection<DiaryRecordImage> ImagesRefs { get; private set; } = new List<DiaryRecordImage>();
    }

    public class Cogitation : IDeletedEntity
    {
        public Guid Id { get; set; }
        public Guid RecordId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool Deleted { get; set; }

        public DiaryRecord? Record { get; set; }
    }

    public static class AppSettingsKeys
    {
        public static string DatesScopeId => "ImportantDaysScopeId";
        public static string DatesDisplayRange => "ImportantDaysDisplayRange";
        public static string StartPageRecordId => "StartPageRecordId";
        public static string ImageQuality => "ImageQuality";
    }

    public class AppSetting
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }
    }
}