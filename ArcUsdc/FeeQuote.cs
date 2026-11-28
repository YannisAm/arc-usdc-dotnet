using System.Numerics;

namespace ArcUsdc;

internal sealed record FeeQuote(
    BigInteger GasLimit,
    BigInteger MaxFeePerGas,
    BigInteger MaxPriorityFeePerGas)
{
    public BigInteger MaxCostUnits =>
        UsdcUnits.FromNativeRoundUp(GasLimit * MaxFeePerGas);
}