using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyImagePrinting.Models;

// Stores one uploaded photo and its selected printing options.
public class OrderDetail
{
    [Key]
    public int DetailId { get; set; }

    public int OrderId { get; set; }

    [Required, StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    public int PrintSizeId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    // This calculated value is not stored as a separate database column.
    [NotMapped]
    public decimal LineTotal => Price * Quantity;

    public Order Order { get; set; } = null!;
    public PrintSize PrintSize { get; set; } = null!;
}
