# arc-usdc-dotnet

A .NET library for sending and receiving USDC on [Arc](https://arc.network), Circle's stablecoin-native blockchain.

> ⚠️ **Early development.** Testnet only. Not ready for production or real funds.

## Why

Arc has official SDKs for Node.js and Python, but nothing for .NET.
This library aims to let .NET developers accept and send USDC payments
without dealing with the underlying blockchain details.

## Current features

- Get the USDC balance of a wallet
- Send USDC to another address
- Basic validation (amount, address, balance)

## Quick example

```csharp
var key = ArcUsdcClient.GenerateKey();
var client = new ArcUsdcClient(key);

Console.WriteLine($"Address: {client.Address}");

decimal balance = await client.GetBalanceAsync();
Console.WriteLine($"Balance: {balance} USDC");

string txHash = await client.TransferAsync("0xRecipientAddress", 1.5m);
```

Get free testnet USDC from the [Circle Faucet](https://faucet.circle.com) (choose **Arc Testnet**).

## Roadmap

- [x] Address validation
- [x] Amount validation
- [ ] Clear error types
- [x] Transaction status and failure handling
- [x] Network fee estimation
- [ ] Testnet / Mainnet configuration
- [ ] Incoming payment detection
- [ ] Idempotency (no duplicate payments)
- [ ] ASP.NET Core integration (separate `ArcUsdc.AspNetCore` package)
- [ ] NuGet package

## Built with

- [.NET 10](https://dotnet.microsoft.com)
- [Nethereum](https://nethereum.com)