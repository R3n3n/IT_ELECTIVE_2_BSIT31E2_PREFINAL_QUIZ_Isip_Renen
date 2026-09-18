using Microsoft.AspNetCore.Mvc;
using RenenPortfolio.Models;
using RenenPortfolio.Services;

namespace RenenPortfolio.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectService _projects;
    private readonly ICommentService _comments;

    public ProjectsController(IProjectService projects, ICommentService comments)
    {
        _projects = projects;
        _comments = comments;
    }

    [HttpGet]
    public IActionResult Index(string? q = null)
    {
        var matches = _projects.Search(q);

        var sections = matches
            .GroupBy(p => p.Category)
            .Select(g => new TableOfContentsSection
            {
                Category = g.Key,
                Projects = g.ToList()
            })
            .ToList();

        return View(new TableOfContentsViewModel
        {
            Sections = sections,
            CommentCounts = _comments.GetCounts(),
            Query = q,
            TotalProjects = _projects.GetAll().Count
        });
    }

    [HttpGet]
    public IActionResult Details(string id)
    {
        var project = _projects.GetBySlug(id);
        if (project is null)
        {
            return NotFound();
        }

        return View(BuildDetail(project, new Comment { ProjectSlug = project.Slug }));
    }

    [HttpPost]
    public IActionResult AddComment([Bind(Prefix = "NewComment")] Comment newComment)
    {
        var project = _projects.GetBySlug(newComment.ProjectSlug ?? string.Empty);
        if (project is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Details", BuildDetail(project, newComment));
        }

        _comments.Add(new Comment
        {
            Id = Guid.NewGuid(),
            ProjectSlug = project.Slug,
            Author = newComment.Author.Trim(),
            Body = newComment.Body.Trim(),
            PostedAt = DateTimeOffset.UtcNow
        });

        TempData["CommentPosted"] = "Comment posted.";
        return RedirectToAction(nameof(Details), new { id = project.Slug });
    }

    [HttpPost]
    public IActionResult DeleteComment(Guid id, string projectSlug)
    {
        if (_comments.Delete(id))
        {
            TempData["CommentPosted"] = "Comment deleted.";
        }

        return RedirectToAction(nameof(Details), new { id = projectSlug });
    }
    private ProjectDetailViewModel BuildDetail(Project project, Comment newComment)
    {
        var (previous, next) = _projects.GetNeighbours(project.Slug);

        return new ProjectDetailViewModel
        {
            Project = project,
            Comments = _comments.GetForProject(project.Slug),
            NewComment = newComment,
            Previous = previous,
            Next = next,
            AllProjects = _projects.GetAll(),
            CommentCounts = _comments.GetCounts()
        };
    }
}