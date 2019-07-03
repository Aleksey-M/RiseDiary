using System;
using System.Collections.Generic;
using System.Linq;

namespace RiseDiary.Model
{
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

    public class DateItem
    {
        public DateItem(DatesRange datesRange, DateTime date)
        {
            if (date == null) throw new ArgumentNullException(nameof(date));
            DatesRange = datesRange ?? throw new ArgumentNullException(nameof(datesRange));
            Date = date;
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

        private string GetWeekDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Пн";
                case DayOfWeek.Tuesday: return "Вт";
                case DayOfWeek.Wednesday: return "Ср";
                case DayOfWeek.Thursday: return "Чт";
                case DayOfWeek.Friday: return "Пт";
                case DayOfWeek.Saturday: return "Сб";
                case DayOfWeek.Sunday: return "Вс";
                default: return "Пн";
            }
        }

        public DateTime TransferredDate => DatesRange.GetTransferredDate(Date);
        public string DisplayDate => TransferredDate.ToString("yyyy.MM.dd") + " " + GetWeekDayName(TransferredDate.DayOfWeek);
        public bool IsWeekday => Name == null;
        public bool IsToday => DatesRange.Today == TransferredDate.Date;

    }

    public class CalendarRecordItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
