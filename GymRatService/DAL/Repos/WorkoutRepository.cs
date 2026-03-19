using GymRatService.Common.Models.Workout;
using GymRatService.DAL.Core;
using GymRatService.DAL.Core.Interfaces;

namespace GymRatService.DAL.Repos
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly DBContext _context;
        public WorkoutRepository(DBContext context)
        {
            _context = context;
        }
        public async Task<Workout> CreateWorkoutAsync(Workout workout)
        {
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();
            return workout;
        }
    }
}
