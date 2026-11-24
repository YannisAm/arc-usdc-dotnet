using System.Globalization;

namespace ArcUsdc.Tests;

public class ArcUsdcValidatorTests
{
    private const string ChecksumAddress = "0xcEBA65587d8FCb785e5BDe94720985a483D65709";


    [Theory]
    [InlineData(ChecksumAddress)]
    [InlineData("0xceba65587d8fcb785e5bde94720985a483d65709")]
    [InlineData("0xCEBA65587D8FCB785E5BDE94720985A483D65709")]
    public void EnsureAddress_ValidAddress_DoesNotThrow(string address)
    {
        var exception = Record.Exception(() => ArcUsdcValidator.EnsureAddress(address));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0x123")]
    [InlineData("hello")]
    [InlineData("0xZZZZ65587d8fcb785e5bde94720985a483d65709")]
    public void EnsureAddress_InvalidFormat_Throws(string address)
    {
        Assert.Throws<ArgumentException>(() => ArcUsdcValidator.EnsureAddress(address));
    }

    [Fact]
    public void EnsureAddress_Null_Throws()
    {
        Assert.Throws<ArgumentException>(() => ArcUsdcValidator.EnsureAddress(null!));
    }

    [Fact]
    public void EnsureAddress_WrongChecksum_Throws()
    {
        var wrongChecksum = "0xCEBA65587d8FCb785e5BDe94720985a483D65709";

        Assert.Throws<ArgumentException>(() => ArcUsdcValidator.EnsureAddress(wrongChecksum));
    }

    [Fact]
    public void EnsureAddress_LowercaseWithRequireChecksum_Throws()
    {
        var lowercase = "0xceba65587d8fcb785e5bde94720985a483d65709";

        Assert.Throws<ArgumentException>(
            () => ArcUsdcValidator.EnsureAddress(lowercase, requireChecksum: true));
    }

    [Fact]
    public void EnsureAddress_ChecksumWithRequireChecksum_DoesNotThrow()
    {
        var exception = Record.Exception(
            () => ArcUsdcValidator.EnsureAddress(ChecksumAddress, requireChecksum: true));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureAddress_ZeroAddress_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => ArcUsdcValidator.EnsureAddress("0x0000000000000000000000000000000000000000"));
    }

    [Fact]
    public void EnsureAddress_UsdcContractAddress_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => ArcUsdcValidator.EnsureAddress(ArcConstants.UsdcContractAddress));
    }

    [Theory]
    [InlineData("0.000001")]
    [InlineData("1")]
    [InlineData("10.5")]
    [InlineData("1000000")]
    public void EnsureAmount_ValidAmount_DoesNotThrow(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        var exception = Record.Exception(() => ArcUsdcValidator.EnsureAmount(amount));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("-0.000001")]
    public void EnsureAmount_ZeroOrNegative_Throws(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        Assert.Throws<ArgumentOutOfRangeException>(() => ArcUsdcValidator.EnsureAmount(amount));
    }

    [Theory]
    [InlineData("0.0000001")]
    [InlineData("1.1234567")]
    public void EnsureAmount_MoreThanSixDecimals_Throws(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);

        Assert.Throws<ArgumentOutOfRangeException>(() => ArcUsdcValidator.EnsureAmount(amount));
    }
}