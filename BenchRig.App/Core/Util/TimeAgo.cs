namespace BenchRig.App.Core.Util;

/// <summary>Compact relative-time formatting ("3m ago", "2d ago") for feeds and comments.</summary>
public static class TimeAgo
{
    public static string From(DateTime when)
    {
        // Firestore writes ISO-8601 UTC ('…Z'); normalize whatever Kind we deserialized to UTC.
        var utc = when.Kind switch
        {
            DateTimeKind.Utc => when,
            DateTimeKind.Local => when.ToUniversalTime(),
            _ => DateTime.SpecifyKind(when, DateTimeKind.Utc),
        };

        var ts = DateTime.UtcNow - utc;
        if (ts < TimeSpan.Zero) ts = TimeSpan.Zero;

        if (ts.TotalSeconds < 45) return "just now";
        if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m ago";
        if (ts.TotalHours < 24) return $"{(int)ts.TotalHours}h ago";
        if (ts.TotalDays < 7) return $"{(int)ts.TotalDays}d ago";
        if (ts.TotalDays < 30) return $"{(int)(ts.TotalDays / 7)}w ago";
        if (ts.TotalDays < 365) return $"{(int)(ts.TotalDays / 30)}mo ago";
        return $"{(int)(ts.TotalDays / 365)}y ago";
    }
}
