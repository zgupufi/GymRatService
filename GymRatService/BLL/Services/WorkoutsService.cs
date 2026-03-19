using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs.Workout;
using GymRatService.Common.Models.Workout;
using GymRatService.DAL.Core.Interfaces;

namespace GymRatService.BLL.Services
{
    public class WorkoutsService : IWorkoutsService
    {
        private readonly IWorkoutRepository _workoutRepository;
        public WorkoutsService(IWorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }
        public async Task<Workout> CreateWorkoutAsync(string userId, CreateWorkoutDTO workoutDTO)
        {
            var workout = new Workout
            {
                Name = workoutDTO.Name,
                UserId = userId,
                Date = DateTime.UtcNow
            };
            return await _workoutRepository.CreateWorkoutAsync(workout);
        }
    }
}
