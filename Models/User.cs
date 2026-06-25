using System.ComponentModel.DataAnnotations;

namespace MyImagePrinting.Models;

// Stores the login information for one customer.
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public int CustId { get; set; }

    // One user account belongs to one customer.
    public Customer Customer { get; set; } = null!;
}
