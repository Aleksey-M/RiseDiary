using System;
using System.Collections.Generic;

namespace RiseDiary.Model
{
    public class DiaryScope
    {
        public int Id { get; set; }
        public string ScopeName { get; set; }
        public bool Deleted { get; set; }

        public ICollection<DiaryTheme> Themes { get; private set; }
    }

    public class DiaryTheme
    {
        public int Id { get; set; }
        public int ScopeId { get; set; }
        public string ThemeName { get; set; }
        public bool Actual { get; set; }
        public bool Deleted { get; set; }

        public DiaryScope Scope { get; private set; }
        public ICollection<DiaryRecordTheme> RecordsRefs { get; private set; }
    }

    public class DiaryRecordTheme
    {
        public int ThemeId { get; set; }
        public int RecordId { get; set; }
        public bool Deleted { get; set; }

        public DiaryTheme Theme { get; private set; }
        public DiaryRecord Record { get; private set; }
    }
        
    public class DiaryThemeJoined
    {
        public int Id { get; set; }
        public int? ScopeId { get; set; }
        public string ThemeName { get; set; }        
        public string ScopeName { get; set; }
        public bool Actual { get; set; }
    }
    
    public class DiaryImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Base64Thumbnail => Convert.ToBase64String(Thumbnail);
        public string SizeKbString => Math.Round(SizeByte / 1024f, 2).ToString() + " Kb";
        public int Width { get; set; }
        public int Height { get; set; }
        public int SizeByte { get; set; }
        public bool Deleted { get; set; }

        public DiaryImageFull FullImage { get; private set; }
        public TempImage TempImage { get; private set; }
        public ICollection<DiaryRecordImage> RecordsRefs { get; private set; }
    }

    public class DiaryImageFull
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }

        public DiaryImage DiaryImage { get; private set; }
    }

    public class TempImage
    {
        public int Id { get; set; }
        public int SourceImageId { get; set; }
        public string Modification { get; set; }
        public byte[] Data { get; set; }
        public string SizeKbString => Math.Round(SizeByte / 1024f, 2).ToString() + " Kb";
        public int Width { get; set; }
        public int Height { get; set; }
        public int SizeByte { get; set; }

        public DiaryImage DiaryImage { get; private set; }
    }

    public class DiaryRecordImage
    {
        public int ImageId { get; set; }
        public int RecordId { get; set; }
        public bool Deleted { get; set; }

        public DiaryImage DiaryImage { get; private set; }
        public DiaryRecord DiaryRecord { get; private set; }
    }

    public class DiaryImageEqualityComparerById : IEqualityComparer<DiaryImage>
    {
        public bool Equals(DiaryImage x, DiaryImage y) => x.Id == y.Id;
        public int GetHashCode(DiaryImage obj) => obj.Id.GetHashCode();
    }

    public class DiaryRecord
    {
        public int Id { get; set; }
        private DateTime _recordDate;
        public DateTime Date { get => _recordDate; set => _recordDate = value.Date; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public bool Deleted { get; set; }

        public string RecordNameDisplay => string.IsNullOrWhiteSpace(Name) ? "[ПУСТО]" : Name;
        public string RecordTextShort {
            get
            {
                if (string.IsNullOrEmpty(Text)) return "[ПУСТО]";
                return Text.Length < 35 ? Text : Text.Substring(0, 35)+"[...]";
            }
        }

        public ICollection<Cogitation> Cogitations { get; private set; }
        public ICollection<DiaryRecordTheme> ThemesRefs { get; private set; }
        public ICollection<DiaryRecordImage> ImagesRefs { get; private set; }
    }

    public class Cogitation
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public bool Deleted { get; set; }

        public DiaryRecord Record { get; private set; }
    }

    public static class AppSettingsKeys
    {
        public static string DatesScopeId => "ImportantDaysScopeId";
        public static string DatesDisplayRange => "ImportantDaysDisplayRange";
    }

    public class AppSetting
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime ModifiedDate { get; set; }
    }    
}
