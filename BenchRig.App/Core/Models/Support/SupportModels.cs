namespace BenchRig.App.Core.Models.Support;

public enum TicketStatus { Open, InProgress, Resolved }

public record SupportTicket
{
    public string Id { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Body { get; init; } = "";
    public string AuthorUid { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public TicketStatus Status { get; init; } = TicketStatus.Open;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

public record TicketResponse
{
    public string Id { get; init; } = "";
    public string Body { get; init; } = "";
    public string AuthorUid { get; init; } = "";
    public string AuthorName { get; init; } = "";
    /// <summary>True for a support/admin reply, false for the ticket owner's reply.</summary>
    public bool IsStaff { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
