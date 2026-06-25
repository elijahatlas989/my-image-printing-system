using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyImagePrinting.Data;
using MyImagePrinting.Helpers;
using MyImagePrinting.Models;
using MyImagePrinting.ViewModels;

namespace MyImagePrinting.Controllers;

// Handles customer registration, customer login and admin login.
public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public AccountController(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Register()
    {
        // Open the customer registration form.
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Stop when the entered registration data is invalid.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var email = model.Email.Trim();

        // Do not allow duplicate usernames or email addresses.
        if (await _db.Users.AnyAsync(user => user.Username == username))
        {
            ModelState.AddModelError(nameof(model.Username), "This username is already used.");
        }

        if (await _db.Customers.AnyAsync(customer => customer.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email address is already registered.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create the customer and related login account in one operation.
        var customer = new Customer
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            DOB = model.DOB,
            Gender = model.Gender,
            PhoneNo = model.PhoneNo.Trim(),
            Address = model.Address.Trim(),
            Email = email
        };

        var user = new User
        {
            Username = username,
            PasswordHash = SecurityHelper.HashPassword(model.Password),
            Customer = customer
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Registration completed. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Open the shared login form for customers and the administrator.
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Stop when username or password is missing.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var adminUsername = _configuration["AdminLogin:Username"];
        var adminPassword = _configuration["AdminLogin:Password"];

        // Check the simple administrator account stored in appsettings.json.
        if (model.Username == adminUsername && model.Password == adminPassword)
        {
            HttpContext.Session.SetString("Role", "Admin");
            HttpContext.Session.SetString("DisplayName", "Administrator");
            return RedirectToAction("Dashboard", "Admin");
        }

        // Check a normal customer account in the database.
        var user = await _db.Users
            .Include(item => item.Customer)
            .FirstOrDefaultAsync(item => item.Username == model.Username);

        var enteredHash = SecurityHelper.HashPassword(model.Password);
        if (user == null || user.PasswordHash != enteredHash)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // Store only basic customer information in session.
        HttpContext.Session.SetString("Role", "Customer");
        HttpContext.Session.SetInt32("CustomerId", user.CustId);
        HttpContext.Session.SetString("DisplayName", user.Customer.FirstName);

        return RedirectToAction("UploadPhotos", "Order");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        // Remove all customer/admin login information from session.
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
