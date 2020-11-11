using System;
using System.Collections.Generic;

namespace RiseDiary.Model
{
    public class DiaryScope : IDeletedEntity
    {
        public Guid Id { get; set; }
        public string ScopeName { get; set; } = "";
        public bool Deleted { get; set; }

        public ICollection<DiaryTheme> Themes { get; set; } = null!;
    }

    public class DiaryTheme : IDeletedEntity
    {
        public Guid Id { get; set; }
        public Guid ScopeId { get; set; }
        public string ThemeName { get; set; } = "";
        public bool Actual { get; set; }
        public bool Deleted { get; set; }

        public DiaryScope? Scope { get; set; }
        public ICollection<DiaryRecordTheme> RecordsRefs { get; set; } = null!;
    }
}
