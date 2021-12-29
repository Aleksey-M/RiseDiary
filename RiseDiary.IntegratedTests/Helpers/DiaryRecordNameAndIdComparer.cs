using RiseDiary.Model;
using System;
using System.Collections.Generic;

namespace RiseDiary.IntegratedTests
{
    internal class DiaryRecordNameAndIdComparer : IEqualityComparer<DiaryRecord>
    {
        public bool Equals(DiaryRecord? x, DiaryRecord? y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            return x.Id == y.Id && x.Name == y.Name;
        }
        public int GetHashCode(DiaryRecord obj) => obj.Id.GetHashCode() + obj.Name.GetHashCode(StringComparison.InvariantCulture);
        public static DiaryRecordNameAndIdComparer Instance => new();
    }
}
