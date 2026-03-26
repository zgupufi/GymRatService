using GymRatService.Common.Models;

namespace GymRatService.DAL.Core.Interfaces
{
    public interface ICompletedSetsRepository
    {
        Task SaveCompletedSetsAsync(List<CompletedSet> completedSets);
        Task<List<CompletedSet>> GetRecentSetsByWorkoutIdAsync(Guid workoutId, string userId);
        Task<List<CompletedSet>> GetExerciseHistoryAsync(int exerciseCardId, string userId);
    }
}
