using System.ComponentModel.DataAnnotations;

namespace MyImagePrinting.ViewModels;

// Collects payment method and shipping address for one order.
public class PaymentViewModel
{
    public int OrderId { get; set; }

    [Required]
    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; } = "Credit Card";

    [Required, StringLength(250)]
    [Display(Name = "Shipping Address")]
    public string ShippingAddress { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Card Number")]
    public string? CardNumber { get; set; }
}
