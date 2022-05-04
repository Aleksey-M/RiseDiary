using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RiseDiary.Model
{
    public sealed class RecordsFilter
    {
        public string? FilterName { get; set; }

        private DateOnly? _recordDateFrom;
        private DateOnly? _recordDateTo;

        public DateOnly? FromDate
        {
            get => _recordDateFrom;
            set => _recordDateFrom = value > _recordDateTo ? null : value;
        }

        public DateOnly? ToDate
        {
            get => _recordDateTo;
            set => _recordDateTo = value < _recordDateFrom ? null : value;
        }

        public int PageNo { get; set; }

        public int PageSize { get; set; } = 20;

        public static RecordsFilter Empty => new ();

        public static bool IsEmpty(RecordsFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);            

            return string.IsNullOrWhiteSpace(filter.FilterName) &&
            filter.FromDate == null &&
            filter.ToDate == null &&
            filter.Themes.Count == 0;
        }

        private List<Guid> _themes = new();

        public ReadOnlyCollection<Guid> Themes => new(_themes);

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

        public IEnumerable<KeyValuePair<string, string?>> GetValuesList()
        {
            var res = new List<KeyValuePair<string, string?>>()
            {
                { new KeyValuePair<string, string?>(nameof(FilterName), FilterName) },
                { new KeyValuePair<string, string?>(nameof(FromDate), FromDate?.ToString("yyyy-MM-dd")) },
                { new KeyValuePair<string, string?>(nameof(ToDate), ToDate?.ToString("yyyy-MM-dd")) },
                { new KeyValuePair<string, string?>(nameof(CombineThemes), CombineThemes ? "true" : null) },
                { new KeyValuePair<string, string?>(nameof(Themes), Themes.Count > 0 ? string.Join(",", Themes) : null) }
            };

            res.AddRange(Themes.Select(t => new KeyValuePair<string, string?>("themes", t.ToString())));

            return res;
        }
    }
}
