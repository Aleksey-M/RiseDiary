using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace RiseDiary.Model
{
#pragma warning disable CA1303 // Do not pass literals as localized parameters
    public class DatesRange
    {       
        public DatesRange(DateTime today, int daysRange)
        {
            Today = today.Date;
            FromDate = Today.AddDays(-daysRange);
            ToDate = Today.AddDays(daysRange);

            ThrowIfRangeIsMoreThanOneYear();

            if (ToDate.Year > FromDate.Year) AtTheJunctionOfYears = true;
        }

        public DatesRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate) throw new ArgumentException("'from' date can't be less then 'to' date");

            FromDate = fromDate.Date;
            ToDate = toDate.Date;
            Today = FromDate.AddDays((ToDate - FromDate).Days / 2);

            ThrowIfRangeIsMoreThanOneYear();

            if (ToDate.Year > FromDate.Year) AtTheJunctionOfYears = true;
        }

        private void ThrowIfRangeIsMoreThanOneYear()
        {
            var testYear = new DateTime(Today.Year, 1, 1); // неправильно учитываются высокосные года
            var range = ToDate - FromDate;

            if((testYear + range).Year > Today.Year) throw new ArgumentException("Dates range is toо big (more then one year)");
        }

        public static DatesRange ForAllYear(int Year = 0)
        {
            if (Year == 0) Year = DateTime.Now.Year;
            return new DatesRange(new DateTime(Year, 1, 1), new DateTime(Year, 12, 31));
        }

        public DateTime FromDate { get; }
        public DateTime ToDate { get; }
        public DateTime Today { get; }
        public bool AtTheJunctionOfYears { get; }

        public bool IsDateInRange(DateTime date)
        {
            var transferredDate = GetTransferredDate(date);
            return transferredDate <= ToDate && transferredDate >= FromDate;
        }

        public DateTime GetTransferredDate(DateTime customDate)
        {
            if (AtTheJunctionOfYears)
            {
                if(Enumerable.Range(FromDate.Month, 12).Contains(customDate.Month))
                    return new DateTime(FromDate.Year, customDate.Month, customDate.Day);

                return new DateTime(ToDate.Year, customDate.Month, customDate.Day);
            }
            else
            {
                return new DateTime(FromDate.Year, customDate.Month, customDate.Day);
            }
        }

        public IEnumerable<DateItem> AllRangeDates => Enumerable
                    .Range(0, (ToDate - FromDate).Days)
                    .Select(i => new DateItem(this, FromDate.AddDays(i)));
        
    }

    public struct DateItem : IEquatable<DateItem>
    {
        public DateItem(DatesRange datesRange, DateTime date)
        {
            if (date == null) throw new ArgumentNullException(nameof(date));
            DatesRange = datesRange ?? throw new ArgumentNullException(nameof(datesRange));
            Date = date;
            Name = string.Empty;
            Text = string.Empty;
            Theme = string.Empty;
            Id = Guid.NewGuid();
        }

        public DateItem(DatesRange datesRange, Guid id, string theme, DateTime date, string name, string text)
        {
            if (date == null) throw new ArgumentNullException(nameof(date));
            DatesRange = datesRange ?? throw new ArgumentNullException(nameof(datesRange));

            Id = id;
            Theme = theme;
            Date = date;
            Name = name;
            Text = text;
        }

        public Guid Id { get; }
        public string Theme { get; }
        public DateTime Date { get; }
        public DatesRange DatesRange { get; }
        public string Name { get; }
        public string Text { get; }

        private static string GetWeekDayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Пн",
                DayOfWeek.Tuesday => "Вт",
                DayOfWeek.Wednesday => "Ср",
                DayOfWeek.Thursday => "Чт",
                DayOfWeek.Friday => "Пт",
                DayOfWeek.Saturday => "Сб",
                DayOfWeek.Sunday => "Вс",
                _ => "Пн",
            };
        }

        public DateTime TransferredDate => DatesRange.GetTransferredDate(Date);
        public string DisplayDate => TransferredDate.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture) + " " + DateItem.GetWeekDayName(TransferredDate.DayOfWeek);
        public bool IsWeekday => string.IsNullOrEmpty(Name);
        public bool IsToday => DatesRange.Today == TransferredDate.Date;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (obj.GetType() == typeof(DateItem))
                return Equals((DateItem)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals([AllowNull] DateItem other)
        {
            return Id == other.Id;
        }

        public static bool operator ==(DateItem left, DateItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DateItem left, DateItem right)
        {
            return !(left == right);
        }
    }

    public struct CalendarRecordItem : IEquatable<CalendarRecordItem>
    {
        public CalendarRecordItem(Guid id, string name, DateTime date)
        {
            Id = id;
            Name = name;
            Date = date;
        }
        public Guid Id { get; }
        public string Name { get; }
        public DateTime Date { get; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (obj.GetType() == typeof(CalendarRecordItem))
                return Equals((CalendarRecordItem)obj);
            else
                return false;
        }

        public bool Equals([AllowNull] CalendarRecordItem other)
        {
            return Id == other.Id && Name == other.Name && Date == other.Date;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(CalendarRecordItem left, CalendarRecordItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CalendarRecordItem left, CalendarRecordItem right)
        {
            return !(left == right);
        }
    }
}
