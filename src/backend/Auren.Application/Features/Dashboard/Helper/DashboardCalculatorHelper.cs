using System.Globalization;

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

        public static string GetColorFromPercent(decimal percent, double alpha = 1)
        {
            percent = Math.Clamp(percent, 0.0m, 100.0m);

            var r = (int)Math.Round(255 - (percent * 2.55m));
            var g = (int)Math.Round(percent * 2.55m);

            return $"rgba({r}, {g}, 0, {alpha.ToString(CultureInfo.InvariantCulture)})";
        }
    }
}
