namespace RenenPortfolio.Models;

public class ProjectDetailViewModel
{
    public Project Project { get; set; } = new();
    public IReadOnlyList<Comment> Comments { get; set; } = Array.Empty<Comment>();
    public Comment NewComment { get; set; } = new();
    public Project? Previous { get; set; }
    public Project? Next { get; set; }
    public IReadOnlyList<Project> AllProjects { get; set; } = Array.Empty<Project>();
    public IReadOnlyDictionary<string, int> CommentCounts { get; set; } = new Dictionary<string, int>();
}