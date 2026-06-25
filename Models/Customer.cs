using System.ComponentModel.DataAnnotations;

namespace MyImagePrinting.Models;

// Stores the personal information of a registered customer.
public class Customer
{
    [Key]
    public int CustId { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DOB { get; set; }

    [Required, StringLength(10)]
    public string Gender { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNo { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    // Navigation properties connect the customer to login and orders.
    public User? User { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
