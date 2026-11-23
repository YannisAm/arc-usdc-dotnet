using Nethereum.ABI.FunctionEncoding;
using Nethereum.JsonRpc.Client;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts.Standards.ERC20;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using System.Numerics;

namespace ArcUsdc
{
    public class ArcUsdcClient
    {
        public string Address { get; }

        private readonly Account _account;
        private readonly Web3 _web3;
        private const string _web3Url = "https://rpc.testnet.arc.network";
        private readonly ERC20ContractService _usdc;

        public ArcUsdcClient(string privateKey)
        {
            _account = new Account(privateKey, chainId: 5042002);
            _web3 = new Web3(_account, _web3Url);
            Address = _account.Address;
            _usdc = _web3.Eth.ERC20.GetContractService(ArcConstants.UsdcContractAddress);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var tokenBalance = await _usdc.BalanceOfQueryAsync(_account.Address);
            return Web3.Convert.FromWei(tokenBalance, 6); // USDC has 6 decimal places
        }

        public async Task<string> TransferAsync(string toAddress, decimal amount)
        {
            ArcUsdcValidator.EnsureAmount(amount);
            ArcUsdcValidator.EnsureAddress(toAddress);
            
            var rawAmount = Web3.Convert.ToWei(amount, 6);

            try
            {
                var balance = await _usdc.BalanceOfQueryAsync(_account.Address);
                
                if (balance < rawAmount)
                {
                    throw new InvalidOperationException("Insufficient balance for the transfer.");
                }

                var feeIn6Decimals = await EstimateFeeUnitsAsync(toAddress, rawAmount);

                if (balance < rawAmount + feeIn6Decimals)
                {
                    var need = Web3.Convert.FromWei(rawAmount, 6);
                    var fee = Web3.Convert.FromWei(feeIn6Decimals, 6);
                    var have = Web3.Convert.FromWei(balance, 6);

                    throw new InvalidOperationException(
                        $"Insufficient balance: need {need} USDC plus ~{fee} USDC network fee, but only {have} USDC is available.");
                }

                var receipt = await _usdc.TransferRequestAndWaitForReceiptAsync(toAddress, rawAmount);
                
                if (receipt.Status?.Value != 1)
                    throw new InvalidOperationException($"Transaction {receipt.TransactionHash} was mined but failed.");
                
                return receipt.TransactionHash;
            }
            catch (SmartContractRevertException ex)
            {
                throw new InvalidOperationException($"Error during transfer: {ex.Message}", ex);
            }
            catch (RpcResponseException ex)
            {
                throw new InvalidOperationException($"Unexpected error during transfer: {ex.Message}", ex);
            }
        }

        public async Task<decimal> EstimateTransferFeeAsync(string toAddress, decimal amount)
        {
            ArcUsdcValidator.EnsureAmount(amount);
            ArcUsdcValidator.EnsureAddress(toAddress);

            // decimal → units
            var rawAmount = Web3.Convert.ToWei(amount, 6);      
            
            var feeUnits = await EstimateFeeUnitsAsync(toAddress, rawAmount);
            
            // units → decimal
            return Web3.Convert.FromWei(feeUnits, 6);                         
        }

        private async Task<BigInteger> EstimateFeeUnitsAsync(string toAddress, BigInteger rawAmount)
        {
            var transferFunction = new TransferFunction
            {
                To = toAddress,
                Value = rawAmount,
                FromAddress = _account.Address
            };
            
            // how much gas is needed
            var gasEstimate = await _usdc.ContractHandler.EstimateGasAsync(transferFunction);
            // how each gas unit costs
            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
                
            // total gas cost
            var feeIn18Decimals = gasEstimate.Value * gasPrice.Value;
                
            // from 18digit to 6digit with round up
            var divisor = BigInteger.Pow(10, 12);
            var feeIn6Decimals = (feeIn18Decimals + divisor - 1) / divisor;

            return feeIn6Decimals;
        }

        public static string GenerateKey()
        {
            return EthECKey.GenerateKey().GetPrivateKey();
        }
    }
}