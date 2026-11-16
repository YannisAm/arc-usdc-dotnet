using ArcUsdc;

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

var receipt = await client.TransferAsync("0x3C3380cdFb94dFEEaA41cAD9F58254AE380d752D", 20.01m);

Console.WriteLine($"Balance: {balance} USDC");
Console.WriteLine($"Address: {adress}");
// Console.WriteLine($"Transaction hash: {receipt}");