using System.ComponentModel.DataAnnotations;

namespace MyImagePrinting.Models;

// Stores the selected payment method for one order.
public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    [Required, StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    // The full card number is protected before it is saved.
    public string? EncryptedCardNumber { get; set; }

    [StringLength(4)]
    public string? CardLastFour { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    public Order Order { get; set; } = null!;
}
