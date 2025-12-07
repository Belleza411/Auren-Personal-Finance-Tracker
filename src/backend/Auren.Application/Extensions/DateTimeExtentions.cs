namespace Auren.Application.Extensions
{
	public static class DateTimeExtentions
	{
        public static (DateTime start, DateTime end) GetLastMonthRange(this DateTime date)
        {
            var firstDayOfCurrentMonth = new DateTime(date.Year, date.Month, 1);
            var firstDayOfLastMonth = firstDayOfCurrentMonth.AddMonths(-1);
            var lastDayOfLastMonth = firstDayOfCurrentMonth.AddDays(-1);

            return (firstDayOfLastMonth, lastDayOfLastMonth);
        }

        public static (DateTime start, DateTime end) GetCurrentMonthRange(this DateTime date)
        {
            var firstDayOfCurrentMonth = new DateTime(date.Year, date.Month, 1);
            return (firstDayOfCurrentMonth, date);
        }

        public static (DateTime start, DateTime end) GetLast3MonthRange(this DateTime date)
        {
            var firstDayOfCurrentMonth = new DateTime(date.Year, date.Month, 1);
            var firstDayOfLast3Months = firstDayOfCurrentMonth.AddMonths(-3);
            var lastDayOfLast3Months = firstDayOfCurrentMonth.AddDays(-1);

            return (firstDayOfLast3Months, lastDayOfLast3Months);
        }

        public static (DateTime start, DateTime end) GetLast6MonthRange(this DateTime date)
        {
            var firstDayOfCurrentMonth = new DateTime(date.Year, date.Month, 1);
            var firstDayOfLast6Months = firstDayOfCurrentMonth.AddMonths(-6);
            var lastDayOfLast6Months = firstDayOfCurrentMonth.AddDays(-1);

            return (firstDayOfLast6Months, lastDayOfLast6Months);
        }

        public static (DateTime start, DateTime end) GetThisYearRange(this DateTime date)
        {
            var firstDayOfThisYear = new DateTime(date.Year, 1, 1);
            return (firstDayOfThisYear, date);
        }
    }
}
