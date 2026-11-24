using System.Numerics;
using Nethereum.Web3;

namespace ArcUsdc;

internal static class UsdcUnits
{
    public const int Decimals = 6;
    public const int NativeDecimals = 18;

    public static BigInteger ToUnits(decimal amount)
    {
        return Web3.Convert.ToWei(amount, Decimals);
    }

    public static decimal FromUnits(BigInteger units)
    {
        return Web3.Convert.FromWei(units, Decimals);
    }

    public static BigInteger FromNativeRoundUp(BigInteger nativeAmount)
    {
        var divisor = BigInteger.Pow(10, NativeDecimals - Decimals); // 18 − 6 = 12
        return (nativeAmount + divisor - 1) / divisor;
    }
}