using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RiseDiary.Model
{
    public sealed class RecordsFilter
    {
        public string? FilterName { get; set; }

        private DateTime? _recordDateFrom;
        private DateTime? _recordDateTo;

        public DateTime? FromDate
        {
            get => _recordDateFrom;
            set => _recordDateFrom = value > _recordDateTo ? null : value?.Date;
        }

        public DateTime? ToDate
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

            return string.IsNullOrWhiteSpace(filter.FilterName) &&
            filter.FromDate == null &&
            filter.ToDate == null &&
            filter.Themes.Count == 0;
        }

        private List<Guid> _themes = new List<Guid>();

        public ReadOnlyCollection<Guid> Themes => new ReadOnlyCollection<Guid>(_themes);

        public void AddThemeId(Guid rtid)
        {
            if (!_themes.Contains(rtid))
            {
                _themes.Add(rtid);
            }
        }

        public void AddThemeId(IEnumerable<Guid> idsList)
        {
            _themes = _themes.Union(idsList).ToList();
        }

        public void RemoveThemeId(Guid id)
        {
            if (_themes.Contains(id))
            {
                _themes.Remove(id);
            }
        }

        public void RemoveThemeId(IEnumerable<Guid> idsList)
        {
            _themes = _themes.Except(idsList).ToList();
        }

        public bool IsEmptyTypeFilter => Themes.Count == 0;

        public bool CombineThemes { get; set; }

        public Dictionary<string, string?> GetValuesDict() => new Dictionary<string, string?>()
        {
            { nameof(FilterName), FilterName },
            { nameof(FromDate), FromDate?.ToString("yyyy-MM-dd") },
            { nameof(ToDate), ToDate?.ToString("yyyy-MM-dd") },
           // { nameof(PageNo), PageNo.ToString() },
           // { nameof(PageSize), PageSize.ToString() },
            { nameof(CombineThemes), CombineThemes ? "true" : null },
            { nameof(Themes), Themes.Count > 0 ? string.Join(",", Themes) : null }
        };
    }
}
