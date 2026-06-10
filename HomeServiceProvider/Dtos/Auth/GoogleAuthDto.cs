using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Auth;

public class GoogleAuthDto
{
    // The ID token that React gets from Google Sign-In
    [Required]
    public string IdToken { get; set; } = string.Empty;

    // Customer (default) or Provider — user chooses on your signup screen
    public string Role { get; set; } = "Customer";
}