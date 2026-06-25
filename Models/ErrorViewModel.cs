namespace MyImagePrinting.Models;

// Provides a small model for the error page.
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
