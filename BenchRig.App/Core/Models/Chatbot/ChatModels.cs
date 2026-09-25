namespace BenchRig.App.Core.Models.Chatbot;

public record ChatMessage
{
    public required string Role { get; init; }    // "user" | "assistant" | "system"
    public required string Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
