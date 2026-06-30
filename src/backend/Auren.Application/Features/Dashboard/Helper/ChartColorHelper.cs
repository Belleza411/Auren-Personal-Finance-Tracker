    using System.Globalization;

namespace Auren.Application.Features.Dashboard.Helper
{
    public static class ChartColorHelper
    {
        public static string GetColorFromPercent(decimal percent, double alpha = 1)
        {
            percent = Math.Clamp(percent, 0.0m, 100.0m);

            var r = (int)Math.Round(255 - (percent * 2.55m));
            var g = (int)Math.Round(percent * 2.55m);

            return $"rgba({r}, {g}, 0, {alpha.ToString(CultureInfo.InvariantCulture)})";
        }
    }
}
