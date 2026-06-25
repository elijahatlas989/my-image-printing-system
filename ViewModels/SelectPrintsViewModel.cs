using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyImagePrinting.ViewModels;

// Displays all uploaded photos with print size and quantity controls.
public class SelectPrintsViewModel
{
    public int OrderId { get; set; }
    public List<PrintItemViewModel> Items { get; set; } = new();
    public List<SelectListItem> PrintSizes { get; set; } = new();
}

// Represents the printing options for one uploaded JPEG photo.
public class PrintItemViewModel
{
    public int DetailId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Print Size")]
    public int PrintSizeId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
