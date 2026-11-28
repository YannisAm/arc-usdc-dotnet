namespace ArcUsdc;

public sealed class ArcUsdcClientOptions
{
    public ArcNetwork Network { get; set; } = ArcNetwork.Testnet;

    public string? RpcUrl { get; set; }

    public bool RequireChecksum { get; set; } = false;
}