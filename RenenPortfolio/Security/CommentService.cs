using System.Text.Json;
using RenenPortfolio.Models;

namespace RenenPortfolio.Services;

public interface ICommentService
{
    IReadOnlyList<Comment> GetForProject(string projectSlug);
    IReadOnlyDictionary<string, int> GetCounts();
    void Add(Comment comment);
    bool Delete(Guid id);
}

public class CommentService : ICommentService
{
    private readonly object _gate = new();
    private readonly List<Comment> _comments = new();
    private readonly string _storePath;
    private readonly ILogger<CommentService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public CommentService(IWebHostEnvironment env, ILogger<CommentService> logger)
    {
        _logger = logger;

        var folder = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(folder);
        _storePath = Path.Combine(folder, "comments.json");

        if (File.Exists(_storePath))
        {
            try
            {
                var loaded = JsonSerializer.Deserialize<List<Comment>>(File.ReadAllText(_storePath), JsonOptions);
                if (loaded is not null)
                {
                    _comments.AddRange(loaded);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not read {Path}. Starting with no saved comments.", _storePath);
            }
        }
    }

    public IReadOnlyList<Comment> GetForProject(string projectSlug)
    {
        lock (_gate)
        {
            return _comments
                .Where(c => string.Equals(c.ProjectSlug, projectSlug, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.PostedAt)
                .ToList();
        }
    }

    public IReadOnlyDictionary<string, int> GetCounts()
    {
        lock (_gate)
        {
            return _comments
                .GroupBy(c => c.ProjectSlug, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Add(Comment comment)
    {
        lock (_gate)
        {
            _comments.Add(comment);
            Persist();
        }
    }

    public bool Delete(Guid id)
    {
        lock (_gate)
        {
            var removed = _comments.RemoveAll(c => c.Id == id) > 0;
            if (removed)
            {
                Persist();
            }
            return removed;
        }
    }
    private void Persist()
    {
        try
        {
            File.WriteAllText(_storePath, JsonSerializer.Serialize(_comments, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not write comments to {Path}.", _storePath);
        }
    }
}