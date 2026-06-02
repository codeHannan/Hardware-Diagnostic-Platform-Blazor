namespace BenchRig.App.Core.Models.Support;

public enum TicketStatus { Open, InProgress, Resolved, Closed }

public enum TicketPriority { Low, Medium, High }

public record SupportTicket
{
    public string Id { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Body { get; init; } = "";
    public string AuthorUid { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public TicketStatus Status { get; init; } = TicketStatus.Open;
    public TicketPriority Priority { get; init; } = TicketPriority.Medium;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

public record TicketResponse
{
    public string Id { get; init; } = "";
    public string Body { get; init; } = "";
    public string AdminUid { get; init; } = "";
    public string AdminName { get; init; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
