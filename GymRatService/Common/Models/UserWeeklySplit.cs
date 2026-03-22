using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GymRatService.Common.Models
{
    public class UserWeeklySplit
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string? MondayWorkoutId { get; set; }
        public string?  TuesdayWorkoutId { get; set; }
        public string? WednesdayWorkoutId { get; set; }
        public string? ThursdayWorkoutId { get; set; }
        public string? FridayWorkoutId { get; set; }
        public string? SaturdayWorkoutId { get; set; }
        public string? SundayWorkoutId { get; set; }
    }
}
