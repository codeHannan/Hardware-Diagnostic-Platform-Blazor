namespace BenchRig.App.Core.Interfaces;

/// <summary>
/// Serializable Firestore query descriptor passed to the JS interop layer.
/// All fields are optional; the JS side applies only those that are set.
/// </summary>
public record QueryFilter
{
    public string? WhereField { get; init; }
    public string? WhereOp { get; init; }       // "==", ">", "<", "array-contains", ...
    public object? WhereValue { get; init; }
    public string? OrderByField { get; init; }
    public string? OrderDir { get; init; }       // "asc" | "desc"
    public int? LimitCount { get; init; }
}
