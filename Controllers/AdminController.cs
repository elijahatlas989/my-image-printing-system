using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyImagePrinting.Data;
using MyImagePrinting.Models;

namespace MyImagePrinting.Controllers;

// Provides the simple administrator dashboard required by the project.
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public AdminController(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        // Only the administrator can open this page.
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.TotalOrders = await _db.Orders.CountAsync();
        ViewBag.NewOrders = await _db.Orders.CountAsync(order => order.Status == "New");
        ViewBag.Customers = await _db.Customers.CountAsync();
        ViewBag.PrintSizes = await _db.PrintSizes.CountAsync();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        // Show all purchase requests to the administrator.
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = await _db.Orders
            .Include(order => order.Customer)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(int id)
    {
        // Show the full line details of one purchase order.
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _db.Orders
            .Include(item => item.Customer)
            .Include(item => item.Payment)
            .Include(item => item.OrderDetails)
                .ThenInclude(detail => detail.PrintSize)
            .FirstOrDefaultAsync(item => item.OrderId == id);

        return order == null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, string status)
    {
        // Only the administrator can change an order status.
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var allowedStatuses = new[] { "New", "Printing", "Shipped", "Cancelled" };
        if (!allowedStatuses.Contains(status))
        {
            TempData["Error"] = "Invalid order status.";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = status;

        // Delete the uploaded photo folder after the printed order is shipped.
        if (status == "Shipped" && !string.IsNullOrWhiteSpace(order.FolderName))
        {
            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", order.FolderName);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Order status updated.";
        return RedirectToAction(nameof(OrderDetails), new { id = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> PrintSizes()
    {
        // Show all print sizes and their database prices.
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        return View(await _db.PrintSizes.OrderBy(size => size.PrintSizeId).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPrintSize(PrintSize model)
    {
        // Validate and add a new print size.
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Enter a valid size name and price.";
            return RedirectToAction(nameof(PrintSizes));
        }

        _db.PrintSizes.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Print size added.";
        return RedirectToAction(nameof(PrintSizes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePrintSize(int id)
    {
        // Delete a print size only when no order detail is using it.
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        if (await _db.OrderDetails.AnyAsync(detail => detail.PrintSizeId == id))
        {
            TempData["Error"] = "This print size is already used in an order and cannot be deleted.";
            return RedirectToAction(nameof(PrintSizes));
        }

        var printSize = await _db.PrintSizes.FindAsync(id);
        if (printSize != null)
        {
            _db.PrintSizes.Remove(printSize);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(PrintSizes));
    }

    private bool IsAdmin()
    {
        // Check the administrator role stored in session.
        return HttpContext.Session.GetString("Role") == "Admin";
    }
}
