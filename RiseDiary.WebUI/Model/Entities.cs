using System;
using System.Collections.Generic;

namespace RiseDiary.Model
{
    public class DiaryScope
    {
        public int Id { get; set; }
        public string ScopeName { get; set; }
        public bool Deleted { get; set; }
    }

    public class DiaryTheme
    {
        public int Id { get; set; }
        public int ScopeId { get; set; }
        public string ThemeName { get; set; }
        public bool Deleted { get; set; }
    }

    public class DiaryRecordTheme
    {
        public int ThemeId { get; set; }
        public int RecordId { get; set; }
        public bool Deleted { get; set; }
    }
        
    public class DiaryThemeJoined
    {
        public int Id { get; set; }
        public int? ScopeId { get; set; }
        public string ThemeName { get; set; }        
        public string ScopeName { get; set; }
    }
    
    public class DiaryImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public byte[] Data { get; set; }
        public bool Deleted { get; set; }
    }

    public class DiaryRecordImage
    {
        public int ImageId { get; set; }
        public int RecordId { get; set; }
        public bool Deleted { get; set; }
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
    }

    public class Cogitation
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public bool Deleted { get; set; }
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

    public class DateItem
    {
        public DateItem(DateTime date)
        {
            Date = date;
        }
        public DateItem(int id, string theme, DateTime date, string name, string text)
        {
            Id = id;
            Theme = theme;
            Date = date;
            Name = name;
            Text = text;
        }
        public int Id { get; }
        public string Theme { get; }
        public DateTime Date { get; }
        public string Name { get; }
        public string Text { get; }

        public DateTime ThisYearDate => new DateTime(DateTime.Now.Year, Date.Month, Date.Day);
        public string DisplayDate => ThisYearDate.ToString("yyyy.MM.dd") + " " + ThisYearDate.DayOfWeek.ToString();
        public bool IsWeekday => Name == null;
        public bool IsToday => DateTime.Now.Date == ThisYearDate.Date;

    }

}
