using Nethereum.ABI.FunctionEncoding;
using Nethereum.JsonRpc.Client;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts.Standards.ERC20;

namespace ArcUsdc
{
    public class ArcUsdcClient
    {
        public string Address { get; }

        private readonly Account _account;
        private readonly Web3 _web3;
        private const string _web3Url = "https://rpc.testnet.arc.network";
        private const string _usdcContractAddress = "0x3600000000000000000000000000000000000000"; // USDC contract address on
        private readonly ERC20ContractService _usdc;

        public ArcUsdcClient(string privateKey)
        {
            _account = new Account(privateKey, chainId: 5042002);
            _web3 = new Web3(_account, _web3Url);
            Address = _account.Address;
            _usdc = _web3.Eth.ERC20.GetContractService(_usdcContractAddress);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var tokenBalance = await _usdc.BalanceOfQueryAsync(_account.Address);
            return Web3.Convert.FromWei(tokenBalance, 6); // USDC has 6 decimal places
        }

        public async Task<string> TransferAsync(string toAddress, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Transfer amount must be greater than zero.", nameof(amount));

            if (string.IsNullOrWhiteSpace(toAddress))
                throw new ArgumentException("Recipient address cannot be null or empty.", nameof(toAddress));

            var rawAmount = Web3.Convert.ToWei(amount, 6);

            try
            {
                var balance = await _usdc.BalanceOfQueryAsync(_account.Address);
                
                if (balance < rawAmount)
                {
                    throw new InvalidOperationException("Insufficient balance for the transfer.");
                }

                var receipt = await _usdc.TransferRequestAndWaitForReceiptAsync(toAddress, rawAmount);
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

        public static string GenerateKey()
        {
            return EthECKey.GenerateKey().GetPrivateKey();
        }
    }
}