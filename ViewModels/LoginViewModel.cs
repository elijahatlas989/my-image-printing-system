using System.ComponentModel.DataAnnotations;

namespace MyImagePrinting.ViewModels;

// Collects the username and password from the login page.
public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
