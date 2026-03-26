using GymRatService.Common.DTOs.CompletedSets;
using GymRatService.Common.Models;

namespace GymRatService.BLL.Core.Interfaces
{
    public interface ICompletedSetsService
    {
        Task SaveCompletedSetsAsync(List<CompletedSet> completedSets);
        Task<List<CompletedSet>> GetRecentSetsByWorkoutIdAsync(Guid workoutId, string userId);
        Task<List<CompletedSet>> GetExerciseHistoryAsync(int exerciseCardId, string userId);
    }
}
