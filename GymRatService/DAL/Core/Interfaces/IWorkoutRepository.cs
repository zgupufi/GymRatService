using GymRatService.Common.Models.Workout;

namespace GymRatService.DAL.Core.Interfaces
{
    public interface IWorkoutRepository
    {
        Task<Workout> CreateWorkoutAsync(Workout workout);
    }
}
