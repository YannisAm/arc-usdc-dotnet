# arc-usdc-dotnet

![CI](https://github.com/YannisAm/arc-usdc-dotnet/actions/workflows/ci.yml/badge.svg)

A .NET library for sending and receiving USDC on [Arc](https://www.arc.io), Circle's stablecoin-native blockchain.

> ⚠️ **Early development.** Testnet only. Not ready for production or real funds.

## Why

Arc has official SDKs for Node.js and Python, but nothing for .NET.
This library aims to let .NET developers accept and send USDC payments
without dealing with the underlying blockchain details.

## Current features

- Get the USDC balance of a wallet
- Send USDC to another address
- Input validation (address format, checksum, zero address, amount precision)
- Network fee estimation before sending
- Protection against sending more than balance + network fee
- Detection of transactions that were mined but failed
- Typed exceptions (`InsufficientBalanceException`, `TransactionFailedException`, `ArcNetworkException`)
- Configurable options (network, custom RPC URL, strict checksum mode)
- Unit tests with CI on every pull request
- Get the maximum sendable amount (balance minus maximum network fee)

## Quick example

```csharp
var key = ArcUsdcClient.GenerateKey();
var client = new ArcUsdcClient(key);

Console.WriteLine($"Address: {client.Address}");

decimal balance = await client.GetBalanceAsync();
Console.WriteLine($"Balance: {balance} USDC");

decimal fee = await client.EstimateTransferFeeAsync("0xRecipientAddress", 1.5m);
Console.WriteLine($"Estimated network fee: {fee} USDC");

string txHash = await client.TransferAsync("0xRecipientAddress", 1.5m);
Console.WriteLine($"Transaction: {txHash}");
```
Fee estimates return the maximum network fee. The actual fee charged is usually lower.

Get free testnet USDC from the [Circle Faucet](https://faucet.circle.com) (choose **Arc Testnet**).

## Configuration

By default the client uses **Arc Testnet**. You can change this with `ArcUsdcClientOptions`:

```csharp
var client = new ArcUsdcClient(key, new ArcUsdcClientOptions
{
    Network = ArcNetwork.Testnet,                  // which Arc network to use
    RpcUrl = "https://your-own-rpc-provider.com",  // optional: your own RPC endpoint
    RequireChecksum = true                         // optional: reject addresses without a checksum
});

Console.WriteLine($"Network: {client.Network.Name}");
```

| Option | Default | Description |
|---|---|---|
| `Network` | `ArcNetwork.Testnet` | The Arc network to connect to |
| `RpcUrl` | the network's public RPC | Use your own RPC provider instead |
| `RequireChecksum` | `false` | Only accept checksummed (mixed-case) addresses |

## Error handling

Every failure has its own exception type, so you can react to each case:

```csharp
try
{
    await client.TransferAsync("0xRecipientAddress", 10m);
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
```

Invalid input (bad address, zero or negative amount, more than 6 decimals)
throws `ArgumentException` before anything is sent to the network.

## Roadmap

- [x] Address validation
- [x] Amount validation
- [x] Unit tests for validation and unit conversion
- [x] Clear error types
- [x] Transaction status and failure handling
- [x] Network fee estimation
- [x] Network configuration (`ArcUsdcClientOptions`)
- [x] Optional strict checksum mode
- [x] Mainnet support (experimental)
- [x] Get max sendable amount
- [ ] TransferRejectedException
- [ ] Incoming payment detection
- [ ] Idempotency (no duplicate payments)
- [ ] ASP.NET Core integration (separate `ArcUsdc.AspNetCore` package)
- [ ] NuGet package

## Built with

- [.NET 10](https://dotnet.microsoft.com)
- [Nethereum](https://nethereum.com)