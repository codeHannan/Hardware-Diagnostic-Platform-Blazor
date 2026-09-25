namespace BenchRig.App.Core.Models.Auth;

public enum UserRole { Anonymous, User, Admin }

/// <summary>Authenticated user profile (mirrors Firestore users/{uid}).</summary>
public record UserProfile
{
    public required string Uid { get; init; }
    public string Email { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string? PhotoUrl { get; init; }
    public UserRole Role { get; init; } = UserRole.User;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; init; } = DateTime.UtcNow;

    /// <summary>Sign-in provider id from Firebase ("password", "google.com", …).</summary>
    public string ProviderId { get; init; } = "password";

    public bool IsAdmin => Role == UserRole.Admin;

    /// <summary>True for email/password accounts (can change their password).</summary>
    public bool IsPasswordProvider => string.Equals(ProviderId, "password", StringComparison.OrdinalIgnoreCase);
}
