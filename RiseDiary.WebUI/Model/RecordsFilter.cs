using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RiseDiary.Model
{
    public sealed class RecordsFilter
    {
        public string? RecordNameFilter { get; set; }

        private DateTime? _recordDateFrom;
        private DateTime? _recordDateTo;

        public DateTime? RecordDateFrom
        {
            get => _recordDateFrom;
            set => _recordDateFrom = value > _recordDateTo ? null : value?.Date;
        }

        public DateTime? RecordDateTo
        {
            get => _recordDateTo;
            set => _recordDateTo = value < _recordDateFrom ? null : value?.Date;
        }

        public int PageNo { get; set; }
        public int PageSize { get; set; } = 20;
        public static RecordsFilter Empty => new RecordsFilter();

        public static bool IsEmpty(RecordsFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            return string.IsNullOrWhiteSpace(filter.RecordNameFilter) &&
            filter.RecordDateFrom == null &&
            filter.RecordDateTo == null &&
            filter.RecordThemeIds.Count == 0;
        }

        public ReadOnlyCollection<Guid> RecordThemeIds { get; private set; } = new ReadOnlyCollection<Guid>(new List<Guid>());

        public void AddThemeId(Guid rtid)
        {
            if (!RecordThemeIds.Contains(rtid))
            {
                var list = new List<Guid>(RecordThemeIds)
                {
                    rtid
                };
                RecordThemeIds = new ReadOnlyCollection<Guid>(list);
            }
        }

        public void AddThemeId(IEnumerable<Guid> idsList)
        {
            if (idsList.Any(i => !RecordThemeIds.Contains(i)))
            {
                RecordThemeIds = new ReadOnlyCollection<Guid>(RecordThemeIds.Union(idsList).ToList());
            }
        }

        public void RemoveThemeId(Guid id)
        {
            if (RecordThemeIds.Contains(id))
            {
                RecordThemeIds = new ReadOnlyCollection<Guid>(RecordThemeIds.Where(i => i != id).ToList());
            }
        }

        public void RemoveThemeId(IEnumerable<Guid> idsList)
        {
            var foundIds = RecordThemeIds.Intersect(idsList);
            if (foundIds.Any())
            {
                RecordThemeIds = new ReadOnlyCollection<Guid>(RecordThemeIds.Except(idsList).ToList());
            }
        }

        public bool IsEmptyTypeFilter => RecordThemeIds.Count == 0;

        public bool CombineThemes { get; set; }
    }
}
