namespace BenchRig.App.Core.Models.Forum;

public record ForumPost
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Body { get; init; } = "";
    public string AuthorUid { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public IReadOnlyList<string> Tags { get; init; } = [];
    public double AvgRating { get; init; }
    public int RatingCount { get; init; }
    public int CommentCount { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

public record ForumComment
{
    public string Id { get; init; } = "";
    public string Body { get; init; } = "";
    public string AuthorUid { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public string? ParentCommentId { get; init; }
    public int Depth { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record PostRating
{
    public int Stars { get; init; }
    public string UserId { get; init; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
