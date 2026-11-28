using Nethereum.ABI.FunctionEncoding;
using Nethereum.JsonRpc.Client;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts.Standards.ERC20;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using System.Numerics;
using ArcUsdc.Exceptions;

namespace ArcUsdc
{
    public class ArcUsdcClient
    {
        public ArcNetwork Network { get; }
        
        public string Address { get; }

        private readonly Account _account;
        private readonly Web3 _web3;
        private readonly ERC20ContractService _usdc;
        private readonly bool _requireChecksum;

        public ArcUsdcClient(string privateKey, ArcUsdcClientOptions? options = null)
        {
            options ??= new ArcUsdcClientOptions();

            Network = options.Network
                      ?? throw new ArgumentNullException(nameof(options), "Options.Network cannot be null.");

            _requireChecksum = options.RequireChecksum;

            var rpcUrl = options.RpcUrl ?? Network.RpcUrl;
            
            _account = new Account(privateKey, Network.ChainId);
            _web3 = new Web3(_account, rpcUrl);
            Address = _account.Address;
            _usdc = _web3.Eth.ERC20.GetContractService(Network.UsdcContractAddress);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            return await ExecuteAsync(async () =>
            {
                var tokenBalance = await _usdc.BalanceOfQueryAsync(_account.Address);
                return UsdcUnits.FromUnits(tokenBalance);
            });
        }

        public async Task<string> TransferAsync(string toAddress, decimal amount)
        {
            ArcUsdcValidator.EnsureAmount(amount);
            ArcUsdcValidator.EnsureAddress(toAddress, Network.UsdcContractAddress, _requireChecksum);

            var rawAmount = UsdcUnits.ToUnits(amount);

            return await ExecuteAsync(async () =>
            {
                var balance = await _usdc.BalanceOfQueryAsync(_account.Address);

                if (balance < rawAmount)
                    throw new InsufficientBalanceException(amount, 0m, UsdcUnits.FromUnits(balance));

                var quote = await QuoteFeeAsync(toAddress, rawAmount);

                if (balance < rawAmount + quote.MaxCostUnits)
                    throw new InsufficientBalanceException(
                        amount,
                        UsdcUnits.FromUnits(quote.MaxCostUnits),
                        UsdcUnits.FromUnits(balance));

                var transferFunction = new TransferFunction
                {
                    To = toAddress,
                    Value = rawAmount,
                    Gas = quote.GasLimit,
                    MaxFeePerGas = quote.MaxFeePerGas,
                    MaxPriorityFeePerGas = quote.MaxPriorityFeePerGas
                };

                var receipt = await _usdc.ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction);

                if (receipt.Status?.Value != 1)
                    throw new TransactionFailedException(receipt.TransactionHash);

                return receipt.TransactionHash;
            });
        }

        public async Task<decimal> EstimateTransferFeeAsync(string toAddress, decimal amount)
        {
            ArcUsdcValidator.EnsureAmount(amount);
            ArcUsdcValidator.EnsureAddress(toAddress, Network.UsdcContractAddress, _requireChecksum);

            var rawAmount = UsdcUnits.ToUnits(amount);

            return await ExecuteAsync(async () =>
            {
                var quote = await QuoteFeeAsync(toAddress, rawAmount);
                return UsdcUnits.FromUnits(quote.MaxCostUnits);
            });
        }

        public static string GenerateKey()
        {
            return EthECKey.GenerateKey().GetPrivateKey();
        }
        
        public async Task<decimal> GetMaxSendableAmountAsync(string toAddress)
        {
            ArcUsdcValidator.EnsureAddress(toAddress, Network.UsdcContractAddress, _requireChecksum);

            return await ExecuteAsync(async () =>
            {
                var balance = await _usdc.BalanceOfQueryAsync(_account.Address);

                if (balance.IsZero)
                    return 0m;

                var quote = await QuoteFeeAsync(toAddress, balance);

                var maxUnits = balance - quote.MaxCostUnits;

                if (maxUnits <= 0)
                    return 0m;

                return UsdcUnits.FromUnits(maxUnits);
            });
        }
        
        private static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            try
            {
                return await operation();
            }
            catch (SmartContractRevertException ex)
            {
                throw new ArcUsdcException($"The USDC contract rejected the operation: {ex.Message}", ex);
            }
            catch (RpcResponseException ex)
            {
                throw new ArcNetworkException("The Arc network returned an error.", ex);
            }
            catch (RpcClientTimeoutException ex)
            {
                throw new ArcNetworkException("The Arc network did not respond in time.", ex);
            }
            catch (RpcClientUnknownException ex)
            {
                throw new ArcNetworkException("Could not connect to the Arc network.", ex);
            }
        }
        
        private async Task<FeeQuote> QuoteFeeAsync(string toAddress, BigInteger rawAmount)
        {
            var transferFunction = new TransferFunction
            {
                To = toAddress,
                Value = rawAmount,
                FromAddress = _account.Address
            };

            var gasEstimate = await _usdc.ContractHandler.EstimateGasAsync(transferFunction);

            var fee = await _web3.FeeSuggestion.GetSimpleFeeSuggestionStrategy().SuggestFeeAsync();

            if (fee.MaxFeePerGas is null || fee.MaxPriorityFeePerGas is null)
                throw new ArcUsdcException("The Arc network did not return a fee suggestion.");

            return new FeeQuote(
                gasEstimate.Value,
                fee.MaxFeePerGas.Value,
                fee.MaxPriorityFeePerGas.Value);
        }
    }
}