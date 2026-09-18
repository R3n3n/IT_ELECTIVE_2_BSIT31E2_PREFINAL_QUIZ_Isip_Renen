namespace RenenPortfolio.Models;

public class TableOfContentsViewModel
{
    public IReadOnlyList<TableOfContentsSection> Sections { get; set; } = Array.Empty<TableOfContentsSection>();
    public IReadOnlyDictionary<string, int> CommentCounts { get; set; } = new Dictionary<string, int>();
    public string? Query { get; set; }
    public int TotalProjects { get; set; }
}

public class TableOfContentsSection
{
    public string Category { get; set; } = string.Empty;
    public IReadOnlyList<Project> Projects { get; set; } = Array.Empty<Project>();
}