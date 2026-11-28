using System.Numerics;

namespace ArcUsdc;

public sealed class ArcNetwork
{
    public string Name { get; }
    public BigInteger ChainId { get; }
    public string RpcUrl { get; }
    public string UsdcContractAddress { get; }

    private ArcNetwork(string name, BigInteger chainId, string rpcUrl, string usdcContractAddress)
    {
        Name = name;
        ChainId = chainId;
        RpcUrl = rpcUrl;
        UsdcContractAddress = usdcContractAddress;
    }

    public static readonly ArcNetwork Testnet = new(
        name: "Arc Testnet",
        chainId: 5042002,
        rpcUrl: "https://rpc.testnet.arc.io",
        usdcContractAddress: "0x3600000000000000000000000000000000000000");
}