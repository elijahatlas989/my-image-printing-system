using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyImagePrinting.Models;

// Stores each available photo print size and its price.
public class PrintSize
{
    [Key]
    public int PrintSizeId { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Print Size")]
    public string SizeName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    [Range(1, 100000)]
    public decimal Price { get; set; }

    // One print size can be used by many order details.
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
