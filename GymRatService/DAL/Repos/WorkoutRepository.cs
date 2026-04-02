using GymRatService.Common.DTOs.Workout;
using GymRatService.Common.Models.Workout;
using GymRatService.DAL.Core;
using GymRatService.DAL.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

//
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

        public async Task<bool> DeleteWorkoutAsync(Workout workout, string userId)
        {
            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Workout> GetWorkoutByIdAndUserIdAsync(Guid workoutId, string userId)
        {
            return await _context.Workouts.Include(w => w.WorkoutExercises).FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);
        }

        public async Task<List<Workout>> GetWorkoutsByUserIdAsync(string userId)
        {
            return await _context.Workouts
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<Workout> UpdateWorkoutAsync(Workout workout)
        {
            _context.Workouts.Update(workout);
            await _context.SaveChangesAsync();
            return workout;
        }
    }
}
