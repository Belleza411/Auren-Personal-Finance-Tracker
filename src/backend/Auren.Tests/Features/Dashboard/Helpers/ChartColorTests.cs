using Auren.Application.Features.Dashboard.Helper;
using FluentAssertions;

namespace Auren.Tests.Features.Dashboard.Helpers
{
    public class ChartColorTests
    {
        [Fact]
        public void GetColorFromPercent_WhenZero_ReturnsRed()
        {
            var result = ChartColorHelper.GetColorFromPercent(0);
            result.Should().Be("rgba(255, 0, 0, 1)");
        }

        [Fact]
        public void GetColorFromPercent_WhenHundred_ReturnsGreen()
        {
            var result = ChartColorHelper.GetColorFromPercent(100);
            result.Should().Be("rgba(0, 255, 0, 1)");
        }

        [Fact]
        public void GetColorFromPercent_WhenHalves_ReturnMidColor()
        {
            var result = ChartColorHelper.GetColorFromPercent(50);
            result.Should().Be("rgba(128, 128, 0, 1)");
        }

        [Fact]
        public void GetColorFromPercent_WhenNegative_ReturnsZero()
        {
            var result = ChartColorHelper.GetColorFromPercent(-50);
            result.Should().Be("rgba(255, 0, 0, 1)");
        }

        [Fact]
        public void GetColorFromPercent_AlphaUsesInvariantCulture()
        {
            var result = ChartColorHelper.GetColorFromPercent(0, 0.75);
            result.Should().Contain("0.75");
        }
    }
}
