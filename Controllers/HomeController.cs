using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyImagePrinting.Models;

namespace MyImagePrinting.Controllers;

// Displays the public pages of the MyImage website.
public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Show the main landing page.
        return View();
    }

    public IActionResult About()
    {
        // Show basic information about the project.
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Display a simple error page with the current request ID.
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
