using System.ComponentModel.DataAnnotations;

namespace RenenPortfolio.Models;
public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProjectSlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a name so Renen knows who wrote this.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Names are between 2 and 60 characters.")]
    [Display(Name = "Your name")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "Write a comment before posting.")]
    [StringLength(1000, MinimumLength = 2, ErrorMessage = "Comments are between 2 and 1000 characters.")]
    [Display(Name = "Comment")]
    public string Body { get; set; } = string.Empty;

    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;
}