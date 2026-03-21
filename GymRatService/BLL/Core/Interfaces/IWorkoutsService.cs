using GymRatService.Common.DTOs.Workout;
using GymRatService.Common.Models.Workout;

namespace GymRatService.BLL.Core.Interfaces
{
    public interface IWorkoutsService
    {
        Task<Workout> CreateWorkoutAsync(string userId,CreateWorkoutDTO workoutDTO);
        Task<Workout> UpdateWorkoutAsync(Guid workoutId,string userId,UpdateWorkoutDTO updateWorkoutDTO);   
        Task<Object> GetFormattedWorkoutsByUserIdAsync(string userId);
        Task<bool> DeleteWorkoutAsync(Guid workoutId, string userId);
    }
}
