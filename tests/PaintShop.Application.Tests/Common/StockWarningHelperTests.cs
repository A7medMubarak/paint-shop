using PaintShop.Application.Common;

namespace PaintShop.Application.Tests.Common;

public class StockWarningHelperTests
{
    [Fact]
    public void WillGoNegative_WhenStockBelowRequested_ReturnsTrue()
    {
        StockWarningHelper.WillGoNegative(5m, 10m).Should().BeTrue();
    }

    [Fact]
    public void WillGoNegative_WhenStockSufficient_ReturnsFalse()
    {
        StockWarningHelper.WillGoNegative(10m, 5m).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(IsLowStockData))]
    public void IsLowStock_VariousScenarios_ReturnsExpected(decimal quantity, decimal? threshold, decimal defaultThreshold, bool expected)
    {
        StockWarningHelper.IsLowStock(quantity, threshold, defaultThreshold).Should().Be(expected);
    }

    public static IEnumerable<object?[]> IsLowStockData()
    {
        yield return [3m, (decimal?)5m, 5m, true];
        yield return [6m, (decimal?)5m, 5m, false];
        yield return [4m, null, 5m, true];
        yield return [6m, null, 5m, false];
    }
}
