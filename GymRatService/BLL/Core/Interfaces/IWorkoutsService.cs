using GymRatService.Common.DTOs.Workout;
using GymRatService.Common.Models.Workout;

namespace GymRatService.BLL.Core.Interfaces
{
    public interface IWorkoutsService
    {
        Task<Workout> CreateWorkoutAsync(string userId,CreateWorkoutDTO workoutDTO);
    }
}
