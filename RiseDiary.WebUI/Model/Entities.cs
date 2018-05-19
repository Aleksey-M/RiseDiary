using System;
using System.Collections.Generic;

namespace RiseDiary.Model
{
    public class DiaryScope
    {
        public int Id { get; set; }
        public string ScopeName { get; set; }

        public virtual ICollection<DiaryTheme> Themes { get; set; }
    }

    public class DiaryTheme
    {
        public int Id { get; set; }
        public int DiaryScopeId { get; set; }
        public string ThemeName { get; set; }
    }

    public class DiaryRecordTheme
    {
        public int ThemeId { get; set; }
        public int RecordId { get; set; }
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
    }

    public class DiaryRecordImage
    {
        public int ImageId { get; set; }
        public int RecordId { get; set; }
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
    }
}
