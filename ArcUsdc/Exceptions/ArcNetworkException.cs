namespace ArcUsdc.Exceptions;

public class ArcNetworkException : ArcUsdcException
{
    public ArcNetworkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}