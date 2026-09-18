namespace RenenPortfolio.Models;
public class Project
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = "/img/thumbs/placeholder.svg";
    public string Language { get; set; } = "C#";
    public string Category { get; set; } = "Coursework";
    public List<string> Tags { get; set; } = new();
    public int Year { get; set; }
    public bool IsFork { get; set; }
}