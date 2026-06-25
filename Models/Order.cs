using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyImagePrinting.Models;

// Stores the main purchase order created by a customer.
public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int CustId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Required, StringLength(50)]
    public string Status { get; set; } = "Draft";

    [StringLength(100)]
    public string FolderName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? ShippingAddress { get; set; }

    // Navigation properties connect the order to its customer, photos and payment.
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public Payment? Payment { get; set; }
}
