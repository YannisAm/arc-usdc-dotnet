namespace ArcUsdc.Exceptions;

public class ArcUsdcException : Exception
{
    public ArcUsdcException(string message)
        : base(message)
    {
    }

    public ArcUsdcException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}