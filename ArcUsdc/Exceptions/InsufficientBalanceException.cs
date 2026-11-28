namespace ArcUsdc.Exceptions;

public class InsufficientBalanceException : ArcUsdcException
{
    public decimal Amount { get; }
    public decimal EstimatedFee { get; }
    public decimal Available { get; }
    public decimal Missing => Amount + EstimatedFee - Available;

    public InsufficientBalanceException(decimal amount, decimal estimatedFee, decimal available)
        : base($"Insufficient balance: need {amount} USDC plus up to ~{estimatedFee} USDC network fee, " +
               $"but only {available} USDC is available.")
    {
        Amount = amount;
        EstimatedFee = estimatedFee;
        Available = available;
    }
}