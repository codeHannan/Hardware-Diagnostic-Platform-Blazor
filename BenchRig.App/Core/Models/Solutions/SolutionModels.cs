namespace BenchRig.App.Core.Models.Solutions;

public enum ArticleCategory { Network, Mouse, Keyboard, Audio, Monitor, Compute, General }

public record SolutionArticle
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Body { get; init; } = "";
    public ArticleCategory Category { get; init; } = ArticleCategory.General;
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string AuthorUid { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public bool IsPublished { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}
