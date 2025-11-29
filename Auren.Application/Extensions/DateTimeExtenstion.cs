namespace Auren.Application.Extensions
{
	public static class DateTimeExtenstion
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
    }
}
