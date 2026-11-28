namespace ArcUsdc.Exceptions;

public class TransactionFailedException : ArcUsdcException
{
    public string TransactionHash { get; }

    public TransactionFailedException(string transactionHash)
        : base($"Transaction {transactionHash} was mined but failed.")
    {
        TransactionHash = transactionHash;
    }
}