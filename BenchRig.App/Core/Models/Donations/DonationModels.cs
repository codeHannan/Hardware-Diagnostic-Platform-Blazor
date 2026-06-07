namespace BenchRig.App.Core.Models.Donations;

/// <summary>A single donation amount backed by a Stripe Payment Link.</summary>
public record DonationTier
{
    public int Cents { get; init; }
    public string Label { get; init; } = "";
    public string Url { get; init; } = "";
    /// <summary>False when the link is still a placeholder (so the UI won't open a broken tab).</summary>
    public bool Configured { get; init; }
}

/// <summary>Donation configuration surfaced from <c>stripe-interop.js</c>.</summary>
public record DonationOptions
{
    public string Currency { get; init; } = "usd";
    public IReadOnlyList<DonationTier> OneTime { get; init; } = [];
    public IReadOnlyList<DonationTier> Monthly { get; init; } = [];
    public DonationTier? Custom { get; init; }

    public bool OneTimeConfigured { get; init; }
    public bool MonthlyConfigured { get; init; }
    /// <summary>True when at least one real (non-placeholder) link exists.</summary>
    public bool Configured { get; init; }
}
