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

        public async Task<Workout> UpdateWorkoutAsync(Guid workoutId, string userId, UpdateWorkoutDTO updateWorkoutDTO)
        {
            var workout = await _workoutRepository.GetWorkoutByIdAndUserIdAsync(workoutId, userId);
            if (workout == null) return null;

            workout.Name = updateWorkoutDTO.Name;
            workout.WorkoutExercises.Clear();

            foreach (var exerciseDto in updateWorkoutDTO.Exercises)
            {
                var newExercise = new WorkoutExercise
                {
                    ExerciseCardId = exerciseDto.Id,
                    Sets = new List<WorkoutSet>()
                };

                for (int i = 1; i <= exerciseDto.TargetSets; i++)
                {
                    var newSet = new WorkoutSet
                    {
                        SetNumber = i,
                        WeightKg = 0,
                        Reps = 0,
                        IsCompleted = false
                    };

                    // 3. Adaugi seria generată în obiectul pentru baza de date! (NU în DTO)
                    newExercise.Sets.Add(newSet);
                }

                // 4. Adaugi exercițiul plin de serii în antrenamentul mare
                workout.WorkoutExercises.Add(newExercise);
            }

            await _workoutRepository.UpdateWorkoutAsync(workout);
            return workout;
        }

        public async Task<Object> GetFormattedWorkoutsByUserIdAsync(string userId)
        {
            var workoutsDb = await _workoutRepository.GetWorkoutsByUserIdAsync(userId);

            var formattedWorkouts = workoutsDb.Select(w => new
            {
                id = w.Id,
                name = w.Name,
                dateCreated = w.Date,
                exercises = w.WorkoutExercises.Select(we => new
                {
                    id = we.ExerciseCardId,
                    name = we.ExerciseCard.Name,
                    main_muscle = we.ExerciseCard?.MainMuscle ?? "",
                    equipment = we.ExerciseCard?.Equipment ?? "",
                    targetSets = we.Sets != null ? we.Sets.Count : 0,
                    uniqueId = Guid.NewGuid().ToString()
                }).ToList()
            }).ToList();

            return formattedWorkouts;
        }

        public async Task<bool> DeleteWorkoutAsync(Guid workoutId, string userId)
        {
            var workout = await _workoutRepository.GetWorkoutByIdAndUserIdAsync(workoutId, userId);

            if (workout == null)
                return false;
            await _workoutRepository.DeleteWorkoutAsync(workout,userId);
            return true;

        }
    }
}
