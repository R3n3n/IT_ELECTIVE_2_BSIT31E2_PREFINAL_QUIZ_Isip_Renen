using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenenPortfolio.Models;
using RenenPortfolio.Services;

namespace RenenPortfolio.Controllers;

public class HomeController : Controller
{
    private readonly IProjectService _projects;

    public HomeController(IProjectService projects) => _projects = projects;

    [HttpGet]
    public IActionResult Index() => View(_projects.GetAll());

    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Error(int? statusCode = null)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? 500
        };

        (model.Title, model.Detail) = model.StatusCode switch
        {
            404 => ("No page here",
                    "That project does not exist. Pick one from the table of contents."),
            403 => ("Not allowed",
                    "You are signed in, but this page is not available to you."),
            _ => ("Something went wrong",
                    "The page could not be loaded. Try again from the table of contents.")
        };

        Response.StatusCode = model.StatusCode;
        return View(model);
    }
}