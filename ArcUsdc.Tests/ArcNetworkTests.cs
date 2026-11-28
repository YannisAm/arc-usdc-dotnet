using Nethereum.Util;

namespace ArcUsdc.Tests;

public class ArcNetworkTests
{
    public static TheoryData<ArcNetwork> AllNetworks => new()
    {
        ArcNetwork.Testnet,
        ArcNetwork.Mainnet
    };

    [Theory]
    [MemberData(nameof(AllNetworks))]
    public void Network_HasAllValuesFilled(ArcNetwork network)
    {
        Assert.False(string.IsNullOrWhiteSpace(network.Name));
        Assert.False(string.IsNullOrWhiteSpace(network.RpcUrl));
        Assert.True(network.ChainId > 0);
    }

    [Theory]
    [MemberData(nameof(AllNetworks))]
    public void Network_UsdcAddressIsValid(ArcNetwork network)
    {
        Assert.True(AddressUtil.Current.IsValidEthereumAddressHexFormat(network.UsdcContractAddress));
    }

    [Theory]
    [MemberData(nameof(AllNetworks))]
    public void Network_RpcUrlUsesHttps(ArcNetwork network)
    {
        Assert.StartsWith("https://", network.RpcUrl);
    }

    [Fact]
    public void TestnetAndMainnet_HaveDifferentChainIds()
    {
        Assert.NotEqual(ArcNetwork.Testnet.ChainId, ArcNetwork.Mainnet.ChainId);
    }

    [Fact]
    public void TestnetAndMainnet_HaveDifferentRpcUrls()
    {
        Assert.NotEqual(ArcNetwork.Testnet.RpcUrl, ArcNetwork.Mainnet.RpcUrl);
    }
}