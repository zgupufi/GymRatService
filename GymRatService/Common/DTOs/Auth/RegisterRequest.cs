using System.ComponentModel.DataAnnotations;
using static GymRatService.Common.Models.User;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty; 

    [Required]
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public WeightUnit PreferredWeightUnit { get; set; }
}