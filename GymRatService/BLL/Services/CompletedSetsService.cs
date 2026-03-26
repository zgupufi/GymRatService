using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs.CompletedSets;
using GymRatService.Common.Models;
using GymRatService.DAL.Core.Interfaces;

namespace GymRatService.BLL.Services
{
    public class CompletedSetsService : ICompletedSetsService
    {
        private readonly ICompletedSetsRepository _completedSetsRepository;
        public CompletedSetsService(ICompletedSetsRepository completedSetsRepository)
        {
            _completedSetsRepository = completedSetsRepository;
        }

        public Task<List<CompletedSet>> GetExerciseHistoryAsync(int exerciseCardId, string userId)
        {
            var exerciseHistory = _completedSetsRepository.GetExerciseHistoryAsync(exerciseCardId, userId); 
            return exerciseHistory;
        }

        public async Task<List<CompletedSet>> GetRecentSetsByWorkoutIdAsync(Guid workoutId, string userId)
        {
           return await _completedSetsRepository.GetRecentSetsByWorkoutIdAsync(workoutId, userId);
           
        }

        public async Task SaveCompletedSetsAsync(List<CompletedSet> completedSets)
        {
            await _completedSetsRepository.SaveCompletedSetsAsync(completedSets);
        }
    }
}
