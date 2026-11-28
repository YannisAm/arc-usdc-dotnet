using Nethereum.Util;

namespace ArcUsdc;

internal static class ArcUsdcValidator
{
    internal static void EnsureAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Transfer amount must be greater than zero.");
        
        if (decimal.Round(amount, 6) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "USDC supports at most 6 decimal places.");
    }
    
    internal static void EnsureAddress(string toAddress, string usdcContractAddress, bool requireChecksum = false)
    {
        if (string.IsNullOrWhiteSpace(toAddress))
            throw new ArgumentException("Recipient address cannot be null or empty.", nameof(toAddress));
            
        if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(toAddress))
            throw new ArgumentException("Recipient address must be '0x' followed by 40 hexadecimal characters.", nameof(toAddress));

        // string after 0x for the address
        string hexPart = toAddress.Substring(2);

        bool hasUpper = hexPart.Any(char.IsUpper);
        bool hasLower = hexPart.Any(char.IsLower);
        bool isMixedCase = hasUpper && hasLower;

        if ((requireChecksum || isMixedCase) && !AddressUtil.Current.IsChecksumAddress(toAddress))
            throw new ArgumentException("Recipient address has an invalid checksum.", nameof(toAddress));
        
        if (AddressUtil.ZERO_ADDRESS.Equals(toAddress))
            throw new ArgumentException("Recipient address cannot be the zero address; funds sent there are lost.", nameof(toAddress));
        
        if (string.Equals(usdcContractAddress, toAddress, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Recipient address cannot be the USDC contract address; funds sent there are lost.", nameof(toAddress));
    }
}