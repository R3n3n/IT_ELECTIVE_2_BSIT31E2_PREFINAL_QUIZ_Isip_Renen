using System.ComponentModel.DataAnnotations;

namespace RenenPortfolio.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your username.")]
    [StringLength(64)]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    [StringLength(128)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}