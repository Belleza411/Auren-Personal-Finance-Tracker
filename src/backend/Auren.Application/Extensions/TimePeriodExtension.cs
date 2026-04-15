using Auren.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Extensions
{
    public static class TimePeriodExtension
    {
        extension(TimePeriod? timePeriod)
        {
            public (DateTime start, DateTime end) GetTimePeriodRange()
            {
                return timePeriod switch
                {
                    TimePeriod.Last3Months => DateTime.Today.GetLast3MonthRange(),
                    TimePeriod.Last6Months => DateTime.Today.GetLast6MonthRange(),
                    TimePeriod.ThisYear => DateTime.Today.GetThisYearRange(),
                    TimePeriod.LastMonth => DateTime.Today.GetLastMonthRange(),
                    TimePeriod.ThisMonth => DateTime.Today.GetCurrentMonthRange(),
                    TimePeriod.AllTime => (DateTime.MinValue, DateTime.Today),
                    _ => (DateTime.MinValue, DateTime.Today)
                };
            }

            public (DateTime start, DateTime end) GetPreviousTimePeriodRange()
            {
                return timePeriod switch
                {
                    TimePeriod.Last3Months => DateTime.Today.AddMonths(-3).GetLast3MonthRange(),
                    TimePeriod.Last6Months => DateTime.Today.AddMonths(-6).GetLast6MonthRange(),
                    TimePeriod.ThisYear => DateTime.Today.AddYears(-1).GetThisYearRange(),
                    TimePeriod.LastMonth => DateTime.Today.AddMonths(-2).GetLastMonthRange(),
                    TimePeriod.ThisMonth => DateTime.Today.GetLastMonthRange(),
                    TimePeriod.AllTime => (DateTime.MinValue, DateTime.Today),
                    _ => (DateTime.MinValue, DateTime.Today)
                };
            }
        }
    }
}
