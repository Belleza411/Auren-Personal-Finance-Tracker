namespace Auren.Application.Features.Dashboard.Helper
{
    public static class DashboardCalculatorHelper
    {
        public static decimal PercentageChange(decimal current, decimal previous)
        {
            if (previous == 0) return current == 0 ? 0 : 100;
            var change = ((current - previous) / Math.Abs(previous)) * 100;
            return Math.Round(Math.Clamp(change, -100, 100), 1, MidpointRounding.AwayFromZero);
        }

        public static decimal AverageDailySpending(decimal expense, DateTime start, DateTime end)
        {
            var days = (end - start).TotalDays + 1;
            return Math.Round(days > 0 ? expense / (decimal)days : 0, 2);
        }
    }
}
