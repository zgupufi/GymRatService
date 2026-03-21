using GymRatService.Common.Models;
using GymRatService.DAL.Core;
using GymRatService.DAL.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymRatService.DAL.Repos
{
    public class CompletedSetsRepository : ICompletedSetsRepository
    {
        private readonly DBContext _context;

        public CompletedSetsRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<List<CompletedSet>> GetRecentSetsByWorkoutIdAsync(Guid workoutId, string userId)
        {
            var exerciseIds = await _context.WorkoutExercises.Where(we => we.WorkoutId == workoutId).Select(we => we.ExerciseCardId).ToListAsync();
            var allRecentSets = new List<CompletedSet>();

            foreach (var exId in exerciseIds)
            {
                var latestDate = await _context.CompletedSets.Where(cs => cs.UserId == userId && cs.ExerciseCardId == exId).MaxAsync(cs => (DateTime?)cs.CompletedAt.Date);
                if (latestDate != null)
                {
                    var recentSetsForExercise = await _context.CompletedSets.Where(cs => cs.UserId == userId&& cs.ExerciseCardId == exId && cs.CompletedAt.Date == latestDate)
                        .OrderBy(cs => cs.SetNumber).ToListAsync();

                    allRecentSets.AddRange(recentSetsForExercise);
                }
            }

            return allRecentSets;
        }

        public async Task SaveCompletedSetsAsync(List<CompletedSet> completedSets)
        {
            _context.CompletedSets.AddRange(completedSets);
            await _context.SaveChangesAsync();
        }
    }
}
