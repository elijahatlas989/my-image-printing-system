using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyImagePrinting.Data;
using MyImagePrinting.Models;
using MyImagePrinting.ViewModels;

namespace MyImagePrinting.Controllers;

// Handles the complete customer order process.
public class OrderController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IDataProtector _cardProtector;

    public OrderController(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        IDataProtectionProvider protectionProvider)
    {
        _db = db;
        _environment = environment;
        _cardProtector =
            protectionProvider.CreateProtector("MyImagePrinting.CardNumber");
    }

    [HttpGet]
    public IActionResult UploadPhotos()
    {
        if (!IsCustomerLoggedIn())
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhotos(List<IFormFile> photos)
    {
        int? customerId = GetCustomerId();

        if (customerId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (photos == null || photos.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Please select at least one image.");

            return View();
        }

        if (photos.Count > 20)
        {
            ModelState.AddModelError(
                string.Empty,
                "You can upload a maximum of 20 images.");

            return View();
        }

        string[] allowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif",
            ".bmp"
        };

        foreach (IFormFile photo in photos)
        {
            string extension = Path
                .GetExtension(photo.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Only JPG, JPEG, PNG, WEBP, GIF and BMP images are allowed.");
            }

            if (photo.Length == 0 ||
                photo.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Each image must be smaller than 10 MB.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        PrintSize? defaultPrintSize = await _db.PrintSizes
            .OrderBy(size => size.PrintSizeId)
            .FirstOrDefaultAsync();

        // Create basic print sizes when the table is empty.
        if (defaultPrintSize == null)
        {
            _db.PrintSizes.AddRange(
                new PrintSize
                {
                    SizeName = "4 x 6",
                    Price = 50.00m
                },
                new PrintSize
                {
                    SizeName = "5 x 7",
                    Price = 80.00m
                },
                new PrintSize
                {
                    SizeName = "8 x 10",
                    Price = 150.00m
                },
                new PrintSize
                {
                    SizeName = "10 x 12",
                    Price = 250.00m
                });

            await _db.SaveChangesAsync();

            defaultPrintSize = await _db.PrintSizes
                .OrderBy(size => size.PrintSizeId)
                .FirstAsync();
        }

        Order order = new()
        {
            CustId = customerId.Value,
            OrderDate = DateTime.Now,
            Status = "Draft",
            TotalAmount = 0
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        order.FolderName = $"folder_{order.OrderId:D4}";

        string orderFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            order.FolderName);

        Directory.CreateDirectory(orderFolder);

        try
        {
            foreach (IFormFile photo in photos)
            {
                string extension = Path
                    .GetExtension(photo.FileName)
                    .ToLowerInvariant();

                string storedFileName =
                    $"{Guid.NewGuid():N}{extension}";

                string fullPath = Path.Combine(
                    orderFolder,
                    storedFileName);

                await using FileStream stream =
                    new(fullPath, FileMode.Create);

                await photo.CopyToAsync(stream);

                order.OrderDetails.Add(new OrderDetail
                {
                    OriginalFileName =
                        Path.GetFileName(photo.FileName),

                    StoredFileName = storedFileName,
                    PrintSizeId = defaultPrintSize.PrintSizeId,
                    Quantity = 1,
                    Price = defaultPrintSize.Price
                });
            }

            order.TotalAmount = order.OrderDetails
                .Sum(detail => detail.Price * detail.Quantity);

            await _db.SaveChangesAsync();
        }
        catch
        {
            if (Directory.Exists(orderFolder))
            {
                Directory.Delete(orderFolder, true);
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            throw;
        }

        // IMPORTANT: After upload, open size and quantity selection.
        return RedirectToAction(
            nameof(SelectPrints),
            new { orderId = order.OrderId });
    }

    [HttpGet]
    public async Task<IActionResult> SelectPrints(int orderId)
    {
        Order? order = await GetCustomerOrder(orderId);

        if (order == null)
        {
            return NotFound();
        }

        SelectPrintsViewModel model =
            await BuildSelectPrintsModel(order);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectPrints(
        SelectPrintsViewModel model)
    {
        Order? order = await GetCustomerOrder(model.OrderId);

        if (order == null)
        {
            return NotFound();
        }

        if (model.Items == null || model.Items.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "No photographs were found in this order.");
        }

        Dictionary<int, OrderDetail> details =
            order.OrderDetails.ToDictionary(
                detail => detail.DetailId);

        Dictionary<int, PrintSize> printSizes =
            await _db.PrintSizes.ToDictionaryAsync(
                size => size.PrintSizeId);

        foreach (PrintItemViewModel item in model.Items)
        {
            if (!details.TryGetValue(
                item.DetailId,
                out OrderDetail? detail))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An invalid photograph was submitted.");

                continue;
            }

            if (!printSizes.TryGetValue(
                item.PrintSizeId,
                out PrintSize? printSize))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please select a valid print size.");

                continue;
            }

            if (item.Quantity < 1 || item.Quantity > 100)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Quantity must be between 1 and 100.");

                continue;
            }

            detail.PrintSizeId = printSize.PrintSizeId;
            detail.Quantity = item.Quantity;
            detail.Price = printSize.Price;
        }

        if (!ModelState.IsValid)
        {
            SelectPrintsViewModel invalidModel =
                await BuildSelectPrintsModel(order);

            return View(invalidModel);
        }

        // Calculate all line totals and the final grand total.
        order.TotalAmount = order.OrderDetails
            .Sum(detail => detail.Price * detail.Quantity);

        order.Status = "Pending Payment";

        await _db.SaveChangesAsync();

        return RedirectToAction(
            nameof(Summary),
            new { orderId = order.OrderId });
    }

    [HttpGet]
    public async Task<IActionResult> Summary(int orderId)
    {
        Order? order = await GetCustomerOrder(orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> Payment(int orderId)
    {
        Order? order = await GetCustomerOrder(orderId);

        if (order == null)
        {
            return NotFound();
        }

        Customer? customer =
            await _db.Customers.FindAsync(order.CustId);

        PaymentViewModel model = new()
        {
            OrderId = order.OrderId,
            PaymentMethod = "Credit Card",
            ShippingAddress = customer?.Address ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(
        PaymentViewModel model)
    {
        Order? order = await GetCustomerOrder(model.OrderId);

        if (order == null)
        {
            return NotFound();
        }

        string paymentMethod =
            model.PaymentMethod?.Trim() ?? string.Empty;

        string? encryptedCardNumber = null;
        string? lastFour = null;

        if (paymentMethod == "Credit Card")
        {
            string digits = new(
                (model.CardNumber ?? string.Empty)
                .Where(char.IsDigit)
                .ToArray());

            if (digits.Length != 16)
            {
                ModelState.AddModelError(
                    nameof(model.CardNumber),
                    "Enter a 16-digit demo card number.");
            }
            else
            {
                encryptedCardNumber =
                    _cardProtector.Protect(digits);

                lastFour = digits[^4..];
            }
        }
        else if (paymentMethod != "Direct Payment")
        {
            ModelState.AddModelError(
                nameof(model.PaymentMethod),
                "Please select a valid payment method.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Payment? existingPayment =
            await _db.Payments.FirstOrDefaultAsync(
                payment => payment.OrderId == order.OrderId);

        if (existingPayment == null)
        {
            _db.Payments.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = paymentMethod,
                EncryptedCardNumber = encryptedCardNumber,
                CardLastFour = lastFour,
                PaymentDate = DateTime.Now
            });
        }
        else
        {
            existingPayment.PaymentMethod = paymentMethod;
            existingPayment.EncryptedCardNumber =
                encryptedCardNumber;

            existingPayment.CardLastFour = lastFour;
            existingPayment.PaymentDate = DateTime.Now;
        }

        order.ShippingAddress =
            model.ShippingAddress.Trim();

        order.Status = "New";

        await _db.SaveChangesAsync();

        return RedirectToAction(
            nameof(Confirmation),
            new { orderId = order.OrderId });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int orderId)
    {
        Order? order = await GetCustomerOrder(orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        int? customerId = GetCustomerId();

        if (customerId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        List<Order> orders = await _db.Orders
            .Where(order => order.CustId == customerId.Value)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    private bool IsCustomerLoggedIn()
    {
        return HttpContext.Session.GetString("Role")
               == "Customer"
               && GetCustomerId() != null;
    }

    private int? GetCustomerId()
    {
        return HttpContext.Session.GetInt32("CustomerId");
    }

    private async Task<Order?> GetCustomerOrder(int orderId)
    {
        int? customerId = GetCustomerId();

        if (customerId == null)
        {
            return null;
        }

        return await _db.Orders
            .Include(order => order.Customer)
            .Include(order => order.Payment)
            .Include(order => order.OrderDetails)
                .ThenInclude(detail => detail.PrintSize)
            .FirstOrDefaultAsync(order =>
                order.OrderId == orderId
                && order.CustId == customerId.Value);
    }

    private async Task<SelectPrintsViewModel>
        BuildSelectPrintsModel(Order order)
    {
        List<PrintSize> sizes = await _db.PrintSizes
            .OrderBy(size => size.Price)
            .ToListAsync();

        return new SelectPrintsViewModel
        {
            OrderId = order.OrderId,

            PrintSizes = sizes
                .Select(size => new SelectListItem
                {
                    Value = size.PrintSizeId.ToString(),
                    Text =
                        $"{size.SizeName} — Rs. {size.Price:N2}"
                })
                .ToList(),

            Items = order.OrderDetails
                .Select(detail =>
                    new PrintItemViewModel
                    {
                        DetailId = detail.DetailId,
                        OriginalFileName =
                            detail.OriginalFileName,

                        ImageUrl =
                            $"/uploads/{order.FolderName}/{detail.StoredFileName}",

                        PrintSizeId =
                            detail.PrintSizeId,

                        Quantity = detail.Quantity
                    })
                .ToList()
        };
    }
}
