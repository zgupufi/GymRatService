using GymRatService.Common.DTOs.Workout;
using GymRatService.Common.Models.Workout;

namespace GymRatService.DAL.Core.Interfaces
{
    public interface IWorkoutRepository
    {
        Task<Workout> CreateWorkoutAsync(Workout workout);
        Task<Workout> UpdateWorkoutAsync(Workout workout);
        Task<Workout> GetWorkoutByIdAndUserIdAsync(Guid workoutId, string userId);
        Task<List<Workout>> GetWorkoutsByUserIdAsync(string userId);
        Task<bool> DeleteWorkoutAsync(Workout workout, string userId);
    }
}
