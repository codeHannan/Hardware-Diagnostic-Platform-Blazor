using BenchRig.App.Core.Models.Report;
using BenchRig.App.Core.Models.Forum;
using BenchRig.App.Core.Models.Support;
using BenchRig.App.Core.Models.Chatbot;
using BenchRig.App.Services.State.Base;

namespace BenchRig.App.Services.State;

public sealed class ReportState : StateContainerBase
{
    public DiagnosticReport? Report { get; private set; }
    public bool IsExporting { get; private set; }

    public void SetReport(DiagnosticReport report) { Report = report; NotifyStateChanged(); }
    public void SetExporting(bool exporting) { IsExporting = exporting; NotifyStateChanged(); }
}

public sealed class ForumState : StateContainerBase
{
    public IReadOnlyList<ForumPost> Posts => _posts.AsReadOnly();
    public ForumPost? SelectedPost { get; private set; }
    public IReadOnlyList<ForumComment> Comments => _comments.AsReadOnly();
    public bool IsLoading { get; private set; }

    private readonly List<ForumPost> _posts = new(64);
    private readonly List<ForumComment> _comments = new(64);

    public void SetPosts(IEnumerable<ForumPost> posts) { _posts.Clear(); _posts.AddRange(posts); NotifyStateChanged(); }
    public void SelectPost(ForumPost? post) { SelectedPost = post; NotifyStateChanged(); }
    public void SetComments(IEnumerable<ForumComment> comments) { _comments.Clear(); _comments.AddRange(comments); NotifyStateChanged(); }
    public void SetLoading(bool loading) { IsLoading = loading; NotifyStateChanged(); }
}

public sealed class TicketState : StateContainerBase
{
    public IReadOnlyList<SupportTicket> MyTickets => _myTickets.AsReadOnly();
    public IReadOnlyList<SupportTicket> AdminQueue => _adminQueue.AsReadOnly();
    public SupportTicket? Selected { get; private set; }
    public IReadOnlyList<TicketResponse> Responses => _responses.AsReadOnly();

    private readonly List<SupportTicket> _myTickets = new();
    private readonly List<SupportTicket> _adminQueue = new();
    private readonly List<TicketResponse> _responses = new();

    public void SetMyTickets(IEnumerable<SupportTicket> t) { _myTickets.Clear(); _myTickets.AddRange(t); NotifyStateChanged(); }
    public void SetAdminQueue(IEnumerable<SupportTicket> t) { _adminQueue.Clear(); _adminQueue.AddRange(t); NotifyStateChanged(); }
    public void Select(SupportTicket? t) { Selected = t; NotifyStateChanged(); }
    public void SetResponses(IEnumerable<TicketResponse> r) { _responses.Clear(); _responses.AddRange(r); NotifyStateChanged(); }
}

public sealed class ChatbotState : StateContainerBase
{
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();
    public bool IsOpen { get; private set; }
    public bool IsLoading { get; private set; }

    private readonly List<ChatMessage> _messages = new(64);

    public void ToggleOpen() { IsOpen = !IsOpen; NotifyStateChanged(); }

    public void AddUserMessage(string content)
    {
        _messages.Add(new ChatMessage { Role = "user", Content = content });
        IsLoading = true;
        NotifyStateChanged();
    }

    public void AddAssistantMessage(string content)
    {
        _messages.Add(new ChatMessage { Role = "assistant", Content = content });
        IsLoading = false;
        NotifyStateChanged();
    }

    public void SetError(string errorMessage)
    {
        _messages.Add(new ChatMessage { Role = "assistant", Content = $"[!] {errorMessage}" });
        IsLoading = false;
        NotifyStateChanged();
    }
}
