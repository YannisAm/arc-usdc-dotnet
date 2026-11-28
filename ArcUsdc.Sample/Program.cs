using ArcUsdc;
using ArcUsdc.Exceptions;

// ------------------------------------------------------------------
// ArcUsdc sample
// Runs on Arc Testnet with test USDC. Never use this wallet for real funds.
// ------------------------------------------------------------------

const string KeyFile = "wallet.key";
const string Recipient = "0x3C3380cdFb94dFEEaA41cAD9F58254AE380d752D";
const decimal Amount = 0.01m;

var client = new ArcUsdcClient(LoadOrCreateKey(KeyFile));

Console.WriteLine($"Network: {client.Network.Name}");
Console.WriteLine($"Address: {client.Address}");

try
{
    var balance = await client.GetBalanceAsync();
    Console.WriteLine($"Balance: {balance} USDC");

    if (balance == 0)
    {
        Console.WriteLine("Get free testnet USDC at https://faucet.circle.com (choose Arc Testnet) and run again.");
        return;
    }

    var fee = await client.EstimateTransferFeeAsync(Recipient, Amount);
    Console.Write($"Send {Amount} USDC to {Recipient} (max network fee: {fee} USDC)? [y/N] ");

    if (Console.ReadLine()?.Trim().ToLowerInvariant() != "y")
    {
        Console.WriteLine("Cancelled.");
        return;
    }

    var txHash = await client.TransferAsync(Recipient, Amount);
    Console.WriteLine($"Sent. Transaction: {txHash}");
}
catch (InsufficientBalanceException ex)
{
    Console.WriteLine($"Not enough USDC. Missing: {ex.Missing} USDC");
}
catch (TransactionFailedException ex)
{
    Console.WriteLine($"Transaction {ex.TransactionHash} was mined but failed.");
}
catch (ArcNetworkException ex)
{
    Console.WriteLine($"Network problem: {ex.Message}");
}
catch (ArcUsdcException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Demo only: a real application should store keys in a secret manager, not in a plain file.
static string LoadOrCreateKey(string path)
{
    if (File.Exists(path))
        return File.ReadAllText(path).Trim();

    var key = ArcUsdcClient.GenerateKey();
    File.WriteAllText(path, key);
    Console.WriteLine($"Created a new test wallet ({path}).");
    return key;
}