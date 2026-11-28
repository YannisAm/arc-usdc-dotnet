# arc-usdc-dotnet

![CI](https://github.com/YannisAm/arc-usdc-dotnet/actions/workflows/ci.yml/badge.svg)

A .NET library for sending and receiving USDC on [Arc](https://arc.network), Circle's stablecoin-native blockchain.

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
- Unit tests with CI on every pull request

## Quick example

```csharp
var key = ArcUsdcClient.GenerateKey();
var client = new ArcUsdcClient(key);

Console.WriteLine($"Address: {client.Address}");

decimal balance = await client.GetBalanceAsync();
Console.WriteLine($"Balance: {balance} USDC");

string txHash = await client.TransferAsync("0xRecipientAddress", 1.5m);

decimal fee = await client.EstimateTransferFeeAsync("0xRecipientAddress", 1.5m);
Console.WriteLine($"Estimated network fee: {fee} USDC");

var client = new ArcUsdcClient(key, new ArcUsdcClientOptions
{
    Network = ArcNetwork.Testnet,                  // which Arc network to use
    RpcUrl = "https://your-own-rpc-provider.com",  // optional: your own RPC endpoint
    RequireChecksum = true                         // optional: reject addresses without a checksum
});

Console.WriteLine($"Network: {client.Network.Name}");
```

Get free testnet USDC from the [Circle Faucet](https://faucet.circle.com) (choose **Arc Testnet**).

## Roadmap

- [x] Address validation
- [x] Amount validation
- [x] Unit tests for validation and unit conversion
- [ ] Clear error types
- [x] Transaction status and failure handling
- [x] Network fee estimation
- [x] Testnet / Mainnet configuration
- [x] Optional strict checksum mode
- [ ] Incoming payment detection
- [ ] Idempotency (no duplicate payments)
- [ ] ASP.NET Core integration (separate `ArcUsdc.AspNetCore` package)
- [ ] NuGet package

## Built with

- [.NET 10](https://dotnet.microsoft.com)
- [Nethereum](https://nethereum.com)