using System.Globalization;
using System.Numerics;

namespace ArcUsdc.Tests;

public class UsdcUnitsTests
{
    [Fact]
    public void ToUnits_TenAndHalf_ReturnsTenMillionFiveHundredThousand()
    {
        var result = UsdcUnits.ToUnits(10.5m);

        Assert.Equal(new BigInteger(10_500_000), result);
    }

    [Fact]
    public void ToUnits_SmallestUnit_ReturnsOne()
    {
        var result = UsdcUnits.ToUnits(0.000001m);

        Assert.Equal(BigInteger.One, result);
    }

    [Fact]
    public void FromUnits_TenMillionFiveHundredThousand_ReturnsTenAndHalf()
    {
        var result = UsdcUnits.FromUnits(new BigInteger(10_500_000));

        Assert.Equal(10.5m, result);
    }

    [Fact]
    public void FromUnits_One_ReturnsSmallestUnit()
    {
        var result = UsdcUnits.FromUnits(BigInteger.One);

        Assert.Equal(0.000001m, result);
    }

    [Theory]
    [InlineData("0.000001")]
    [InlineData("1")]
    [InlineData("10.5")]
    [InlineData("19.998571")]
    [InlineData("1000000")]
    public void ToUnitsThenFromUnits_ReturnsOriginalAmount(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        var result = UsdcUnits.FromUnits(UsdcUnits.ToUnits(amount));

        Assert.Equal(amount, result);
    }

    [Fact]
    public void FromNativeRoundUp_WithLeftover_RoundsUp()
    {
        var native = BigInteger.Parse("714340000000000");

        var result = UsdcUnits.FromNativeRoundUp(native);

        Assert.Equal(new BigInteger(715), result);
    }

    [Fact]
    public void FromNativeRoundUp_ExactAmount_DoesNotRoundUp()
    {
        var native = BigInteger.Parse("714000000000000");

        var result = UsdcUnits.FromNativeRoundUp(native);

        Assert.Equal(new BigInteger(714), result);
    }

    [Fact]
    public void FromNativeRoundUp_TinyCrumb_RoundsUpToOneUnit()
    {
        var result = UsdcUnits.FromNativeRoundUp(BigInteger.One);

        Assert.Equal(BigInteger.One, result);
    }

    [Fact]
    public void FromNativeRoundUp_Zero_ReturnsZero()
    {
        var result = UsdcUnits.FromNativeRoundUp(BigInteger.Zero);

        Assert.Equal(BigInteger.Zero, result);
    }
}