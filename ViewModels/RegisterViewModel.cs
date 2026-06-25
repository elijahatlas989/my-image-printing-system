using System.ComponentModel.DataAnnotations;

namespace MyImagePrinting.ViewModels;

// Collects customer and login information on one registration form.
public class RegisterViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DOB { get; set; } = DateTime.Today.AddYears(-18);

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNo { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 4)]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
