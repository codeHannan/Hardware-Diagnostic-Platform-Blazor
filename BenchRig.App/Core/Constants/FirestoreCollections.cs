namespace BenchRig.App.Core.Constants;

/// <summary>Firestore collection / document path constants.</summary>
public static class FirestoreCollections
{
    public const string Users = "users";
    public const string ForumPosts = "forum_posts";
    public const string Comments = "comments";       // subcollection of forum_posts
    public const string Ratings = "ratings";         // subcollection of forum_posts
    public const string SupportTickets = "support_tickets";
    public const string Responses = "responses";     // subcollection of support_tickets
    public const string SolutionArticles = "solution_articles";
    public const string DiagnosticReports = "diagnostic_reports";
    public const string AppConfig = "app_config";
    public const string AppConfigSettingsDoc = "app_config/settings";

    public static string User(string uid) => $"{Users}/{uid}";
    public static string ForumPost(string postId) => $"{ForumPosts}/{postId}";
    public static string CommentsOf(string postId) => $"{ForumPosts}/{postId}/{Comments}";
    public static string RatingsOf(string postId) => $"{ForumPosts}/{postId}/{Ratings}";
    public static string Ticket(string ticketId) => $"{SupportTickets}/{ticketId}";
    public static string ResponsesOf(string ticketId) => $"{SupportTickets}/{ticketId}/{Responses}";
    public static string Article(string articleId) => $"{SolutionArticles}/{articleId}";
}
