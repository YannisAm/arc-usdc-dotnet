# arc-usdc-dotnet

![CI](https://github.com/YannisAm/arc-usdc-dotnet/actions/workflows/ci.yml/badge.svg)

A .NET library for sending and receiving USDC on [Arc](https://www.arc.io), Circle's stablecoin-native blockchain.

> ⚠️ **Early development.** Tested on Arc Testnet. Mainnet support is experimental. Not ready for real funds.

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
- Get the maximum sendable amount (balance minus maximum network fee)
- Unit tests with CI on every pull request


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

## Running the sample

```bash
dotnet run --project ArcUsdc.Sample
```

The sample creates a test wallet (`wallet.key`) on first run.
Fund it from the [Circle Faucet](https://faucet.circle.com) (choose **Arc Testnet**) and run again.
It asks for confirmation before sending anything.

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

## Key management

This library never stores or logs private keys. It receives the key from your
application and uses it only to sign transactions. **Where you keep the key is
your application's responsibility.**

| Environment | Recommended storage |
|---|---|
| Local development | [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or an environment variable |
| Production | A secret manager such as Azure Key Vault or AWS Secrets Manager |
| High-value production | An HSM or cloud KMS, where the key never leaves secure hardware *(planned: signer support)* |

Never commit private keys to source control. The sample stores its test key in a
plain `wallet.key` file **for demo purposes only**.

## Roadmap

### ✅ Phase 1: Core library
- [x] USDC balance and transfers
- [x] Address and amount validation (checksum, zero address, 6-decimal precision)
- [x] Network fee estimation with explicit EIP-1559 fees
- [x] Max sendable amount
- [x] Detection of failed transactions
- [x] Typed exceptions
- [x] Network configuration (testnet, experimental mainnet, custom RPC, strict checksum)
- [x] Unit tests and CI

### 🚧 Phase 2: Payments and reliability
- [ ] Incoming payment detection
- [ ] Idempotency (no duplicate payments)
- [ ] `CancellationToken` support on all async methods
- [ ] `TransferRejectedException` for blocklisted addresses
- [ ] Signer support for HSM / cloud KMS

### Phase 3: ASP.NET Core integration
- [ ] Separate `ArcUsdc.AspNetCore` package
- [ ] `services.AddArcUsdc(...)` with configuration from `appsettings.json`
- [ ] Background service for incoming payments
- [ ] Sample web API that accepts USDC payments

### Phase 4: Release
- [ ] `src` / `tests` / `samples` folder structure
- [ ] XML documentation for all public APIs
- [ ] Multi-targeting (`netstandard2.0` + `net10.0`)
- [ ] First preview release on NuGet

## Built with

- [.NET 10](https://dotnet.microsoft.com)
- [Nethereum](https://nethereum.com)