using ArcUsdc;
using ArcUsdc.Exceptions;

const string KeyFile = "wallet.key";

string ecKey = String.Empty;

if (File.Exists(KeyFile))
{
    ecKey = File.ReadAllText(KeyFile);
}
else
{
    ecKey = ArcUsdcClient.GenerateKey();
    File.WriteAllText(KeyFile, ecKey);
}

ArcUsdcClient client = new ArcUsdcClient(ecKey);

var balance = await client.GetBalanceAsync();
var adress = client.Address;

//var receipt = await client.TransferAsync("0x3C3380cdFb94dFEEaA41cAD9F58254AE380d752D", 1m);
//var receipt = await client.TransferAsync("hello", 20.01m);

var clientTest= new ArcUsdcClient(ecKey, new ArcUsdcClientOptions
{
    Network = ArcNetwork.Testnet,
    RequireChecksum = true
});

Console.WriteLine($"Network: {clientTest.Network.Name}");

Console.WriteLine($"Balance: {balance} USDC");
Console.WriteLine($"Address: {adress}");
//Console.WriteLine($"Transaction hash: {receipt}");


var lowercaseAddress = "0x3c3380cdfb94dfeeaa41cad9f58254ae380d752d";

var fee = await client.EstimateTransferFeeAsync(lowercaseAddress, 1m);
Console.WriteLine($"Default client accepted lowercase address. Fee: {fee} USDC");

try
{
    await clientTest.EstimateTransferFeeAsync(lowercaseAddress, 1m);
    Console.WriteLine("❌ Strict client accepted it. Something is wrong.");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"✅ Strict client rejected it: {ex.Message}");
}

try
{
    await client.TransferAsync("0x3C3380cdFb94dFEEaA41cAD9F58254AE380d752D", balance);
}
catch (InsufficientBalanceException ex)
{
    Console.WriteLine($"Missing: {ex.Missing} USDC");
}

var offlineClient = new ArcUsdcClient(ecKey, new ArcUsdcClientOptions
{
    RpcUrl = "https://rpc.does-not-exist.invalid"
});

try
{
    await offlineClient.GetBalanceAsync();
    Console.WriteLine("❌ Expected a network error.");
}
catch (ArcNetworkException ex)
{
    Console.WriteLine($"Network error caught: {ex.Message}");
    Console.WriteLine($"   Inner: {ex.InnerException?.GetType().Name}");
}