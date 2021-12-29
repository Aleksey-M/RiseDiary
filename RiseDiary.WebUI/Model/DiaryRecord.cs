using System;
using System.Collections.Generic;

namespace RiseDiary.Model
{
    public class DiaryRecord : IDeletedEntity
    {
        public Guid Id { get; set; }

        private DateTime _recordDate;

        public DateTime Date { get => _recordDate; set => _recordDate = value.Date; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        public string Name { get; set; } = "";

        public string Text { get; set; } = "";

        public bool Deleted { get; set; }

        public ICollection<Cogitation> Cogitations { get; private set; } = null!;

        public ICollection<DiaryRecordTheme> ThemesRefs { get; private set; } = null!;

        public ICollection<DiaryRecordImage> ImagesRefs { get; private set; } = null!;
    }

    public static class DiaryRecordExtensions
    {
        public static string GetRecordNameDisplay(this DiaryRecord rec) => string.IsNullOrWhiteSpace(rec.Name) ? "[ПУСТО]" : rec.Name;

        public static string GetRecordNameDisplay(this string recName) => string.IsNullOrWhiteSpace(recName) ? "[ПУСТО]" : recName;

        public static string GetRecordTextShort(this DiaryRecord rec)
        {
            if (string.IsNullOrEmpty(rec.Text)) return "[ПУСТО]";
            return rec.Text.Length < 35 ? rec.Text : string.Concat(rec.Text.AsSpan(0, 35), "[...]");
        }
    }

    public class Cogitation : IDeletedEntity
    {
        public Guid Id { get; set; }

        public Guid RecordId { get; set; }

        public DateTime Date { get; set; }

        public string Text { get; set; } = "";

        public bool Deleted { get; set; }

        public DiaryRecord? Record { get; set; }
    }

    public class DiaryRecordTheme : IDeletedEntity
    {
        public Guid ThemeId { get; set; }

        public Guid RecordId { get; set; }

        public bool Deleted { get; set; }

        public DiaryTheme? Theme { get; set; }

        public DiaryRecord? Record { get; set; }
    }

    public class DiaryRecordImage : IDeletedEntity
    {
        public Guid ImageId { get; set; }

        public Guid RecordId { get; set; }

        public bool Deleted { get; set; }

        public DiaryImage? Image { get; set; }

        public DiaryRecord? Record { get; set; }
    }
}
