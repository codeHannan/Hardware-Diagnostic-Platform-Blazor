namespace BenchRig.App.Core.Constants;

/// <summary>Centralized route string constants used by pages and navigation.</summary>
public static class AppRoutes
{
    public const string Home = "/";

    public const string Network = "/network";
    public const string Mouse = "/mouse";
    public const string Keyboard = "/keyboard";
    public const string Audio = "/audio";
    public const string Monitor = "/monitor";
    public const string Compute = "/compute";

    public const string Report = "/report";
    public const string Forum = "/forum";
    public const string ForumPost = "/forum/{postId}";
    public const string Support = "/support";
    public const string Solutions = "/solutions";
    public const string Donate = "/donate";

    public const string Login = "/login";
    public const string Register = "/register";

    public const string Admin = "/admin";
    public const string AdminTickets = "/admin/tickets";
    public const string AdminSolutions = "/admin/solutions";

    public static string ForumPostFor(string postId) => $"/forum/{postId}";
}
