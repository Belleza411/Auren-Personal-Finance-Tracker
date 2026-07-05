using Auren.Application.Common.Interfaces;
using Auren.Domain.Enums;

namespace Auren.Application.Extensions
{
    public static class TimePeriodExtension
    {
        extension(TimePeriod timePeriod)
        {
            public (DateTime start, DateTime end) GetTimePeriodRange(IDateTimeProvider dateTime)
            {
                return timePeriod switch
                {
                    TimePeriod.Last3Months => dateTime.Today.GetLast3MonthRange(),
                    TimePeriod.Last6Months => dateTime.Today.GetLast6MonthRange(),
                    TimePeriod.ThisYear => dateTime.Today.GetThisYearRange(),
                    TimePeriod.LastMonth => dateTime.Today.GetLastMonthRange(),
                    TimePeriod.ThisMonth => dateTime.Today.GetCurrentMonthRange(),
                    TimePeriod.AllTime => (DateTime.MinValue, dateTime.Today),
                    _ => (DateTime.MinValue, dateTime.Today)
                };
            }

            public (DateTime start, DateTime end) GetPreviousTimePeriodRange(IDateTimeProvider dateTime)
            {
                return timePeriod switch
                {
                    TimePeriod.Last3Months => dateTime.Today.AddMonths(-3).GetLast3MonthRange(),
                    TimePeriod.Last6Months => dateTime.Today.AddMonths(-6).GetLast6MonthRange(),
                    TimePeriod.ThisYear => dateTime.Today.AddYears(-1).GetThisYearRange(),
                    TimePeriod.LastMonth => dateTime.Today.AddMonths(-1).GetLastMonthRange(),
                    TimePeriod.ThisMonth => dateTime.Today.GetCurrentMonthRange(),
                    TimePeriod.AllTime => (DateTime.MinValue, dateTime.Today),
                    _ => (DateTime.MinValue, dateTime.Today)
                };
            }
        }
    }
}
