using Auren.Application.Features.Dashboard.Helper;
using FluentAssertions;

namespace Auren.Tests.Features.Dashboard.Helpers
{
    public class DashboardCalculatorTests
    {
        [Fact]
        public void PercentageChange_WhenBothZero_ReturnsZero()
        {
            var result = DashboardCalculatorHelper.PercentageChange(0, 0);
            result.Should().Be(0);
        }

        [Fact]
        public void PercentageChange_PreviousZeroAndCurrentNot_Returns100()
        {
            var result = DashboardCalculatorHelper.PercentageChange(100, 0);
            result.Should().Be(100);
        }

        [Fact]
        public void PercentageChange_CurrentZeroAndPreviousNot_ReturnsNegative100()
        {
            var result = DashboardCalculatorHelper.PercentageChange(0, 100);
            result.Should().Be(-100);
        }

        [Fact]
        public void PercentageChange_WhenDoubled_Reuturns100()
        {
            var result = DashboardCalculatorHelper.PercentageChange(200, 100);
            result.Should().Be(100);
        }

        [Fact]
        public void Percentage_WhenHalves_ReturnsNegative50()
        {
            var result = DashboardCalculatorHelper.PercentageChange(50, 100);
            result.Should().Be(-50);
        }

        [Fact]
        public void Percentage_WhenSame_ReturnsZero()
        {
            var result = DashboardCalculatorHelper.PercentageChange(100, 100);
            result.Should().Be(0);
        }

        [Theory]
        [InlineData(100, 50, 100)]    // doubled
        [InlineData(50, 100, -50)]    // halved
        [InlineData(0, 100, -100)]    // dropped to zero
        [InlineData(150, 100, 50)]    // increased by 50%
        [InlineData(100, 100, 0)]     // no change
        public void PercentageChange_ReturnsExpected(
            decimal current,
            decimal previous,
            decimal expected)
        {
            var result = DashboardCalculatorHelper.PercentageChange(current, previous);
            result.Should().Be(expected);
        }

        [Fact]
        public void AvgDailySpending_WhenZeroExpense_ReturnsZero()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 31);

            var result = DashboardCalculatorHelper.AverageDailySpending(0, start, end);
            result.Should().Be(0);
        }

        [Fact]
        public void AvgDailySpending_WhenOneDay_ReturnsFullExpense()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 1);

            var result = DashboardCalculatorHelper.AverageDailySpending(100, start, end);
            result.Should().Be(100);
        }

        [Fact]
        public void AvgDailySpending_WhenFullMonth_ReturnsAvgSpending()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 31);

            var result = DashboardCalculatorHelper.AverageDailySpending(200, start, end);
            result.Should().Be(6.45m);
        }

        [Fact]
        public void AvgDailySpending_RoundToTwoDecimalPlaces()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 3);

            var result = DashboardCalculatorHelper.AverageDailySpending(100, start, end);
            result.Should().Be(33.33m);
        }

        [Fact]
        public void AvgDailySpending_WhenSameDate_CountAsOneDay()
        {
            var date = new DateTime(2026, 4, 22);

            var result = DashboardCalculatorHelper.AverageDailySpending(100, date, date);
            result.Should().Be(100);
        }
    }
}
