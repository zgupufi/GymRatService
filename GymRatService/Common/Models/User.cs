using System.ComponentModel.DataAnnotations;

namespace GymRatService.Common.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [Required]

        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; }

        public WeightUnit PreferredWeightUnit { get; set; } = WeightUnit.Kilograms;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public enum WeightUnit
        {
            Kilograms,
            Pounds
        }

    }
}
