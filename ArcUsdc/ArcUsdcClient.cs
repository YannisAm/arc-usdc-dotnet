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
        
        private readonly ERC20ContractService _usdc;

        public ArcUsdcClient(string privateKey)
        {
            _account = new Account(privateKey, ArcConstants.ChainId);
            _web3 = new Web3(_account, ArcConstants.RpcUrl);
            Address = _account.Address;
            _usdc = _web3.Eth.ERC20.GetContractService(ArcConstants.UsdcContractAddress);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var tokenBalance = await _usdc.BalanceOfQueryAsync(_account.Address);
            
            return UsdcUnits.FromUnits(tokenBalance);
        }

        public async Task<string> TransferAsync(string toAddress, decimal amount)
        {
            ArcUsdcValidator.EnsureAmount(amount);
            ArcUsdcValidator.EnsureAddress(toAddress);

            var rawAmount = UsdcUnits.ToUnits(amount);

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
                    var need = UsdcUnits.FromUnits(rawAmount);
                    var fee = UsdcUnits.FromUnits(feeIn6Decimals);
                    var have = UsdcUnits.FromUnits(balance);

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
            var rawAmount = UsdcUnits.ToUnits(amount);      
            
            var feeUnits = await EstimateFeeUnitsAsync(toAddress, rawAmount);
            
            // units → decimal
            return UsdcUnits.FromUnits(feeUnits);                         
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
            var feeIn6Decimals = UsdcUnits.FromNativeRoundUp(feeIn18Decimals);

            return feeIn6Decimals;
        }

        public static string GenerateKey()
        {
            return EthECKey.GenerateKey().GetPrivateKey();
        }
    }
}