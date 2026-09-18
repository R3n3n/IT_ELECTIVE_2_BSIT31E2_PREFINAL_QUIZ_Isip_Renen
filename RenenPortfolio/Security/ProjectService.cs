using System.Text.Json;
using RenenPortfolio.Models;

namespace RenenPortfolio.Services;

public interface IProjectService
{
    IReadOnlyList<Project> GetAll();
    IReadOnlyList<Project> Search(string? query);
    Project? GetBySlug(string slug);
    (Project? Previous, Project? Next) GetNeighbours(string slug);
}
public class ProjectService : IProjectService
{
    private readonly List<Project> _projects;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IWebHostEnvironment env, ILogger<ProjectService> logger)
    {
        _logger = logger;
        var path = Path.Combine(env.ContentRootPath, "Data", "projects.json");

        try
        {
            var json = File.ReadAllText(path);
            _projects = JsonSerializer.Deserialize<List<Project>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Project>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load projects from {Path}. Starting with an empty portfolio.", path);
            _projects = new List<Project>();
        }
    }

    public IReadOnlyList<Project> GetAll() => _projects;

    public IReadOnlyList<Project> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _projects;
        }

        var q = query.Trim();
        return _projects
            .Where(p =>
                p.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Summary.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Tags.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public Project? GetBySlug(string slug) =>
        _projects.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public (Project? Previous, Project? Next) GetNeighbours(string slug)
    {
        var index = _projects.FindIndex(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return (null, null);
        }

        var previous = index > 0 ? _projects[index - 1] : null;
        var next = index < _projects.Count - 1 ? _projects[index + 1] : null;
        return (previous, next);
    }
}