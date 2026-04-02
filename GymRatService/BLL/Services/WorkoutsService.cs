using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs.Workout;
using GymRatService.Common.Models.Workout;
using GymRatService.DAL.Core.Interfaces;
using GymRatService.DAL.Repos;

namespace GymRatService.BLL.Services
{
    public class WorkoutsService : IWorkoutsService
    {
        private readonly IWorkoutRepository _workoutRepository;

        // 1. Am adăugat repository-ul pentru a putea căuta în baza de date cu exerciții
        private readonly IExerciseCardsRepository _exerciseCardRepository;

        // 2. L-am injectat în constructor
        public WorkoutsService(IWorkoutRepository workoutRepository, IExerciseCardsRepository exerciseCardRepository)
        {
            _workoutRepository = workoutRepository;
            _exerciseCardRepository = exerciseCardRepository;
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

            int currentOrder = 0;
            foreach (var exerciseDto in updateWorkoutDTO.Exercises)
            {
                var newExercise = new WorkoutExercise
                {
                    ExerciseCardId = exerciseDto.Id,
                    OrderIndex = currentOrder++,
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

                    newExercise.Sets.Add(newSet);
                }

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
                exercises = w.WorkoutExercises.OrderBy(we => we.OrderIndex).Select(we => new
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
            await _workoutRepository.DeleteWorkoutAsync(workout, userId);
            return true;
        }

        // 3. Am adăugat logica de Search pe care o va apela AI-ul
        public async Task<object> SearchExercisesAsync(string query)
        {
            // Apelăm baza de date pentru a găsi exerciții care conțin query-ul în nume sau mușchi
            var exercisesDb = await _exerciseCardRepository.SearchExercisesAsync(query);

            // Returnăm o listă formatată curat pentru ca AI-ul să o înțeleagă ușor
            var formattedExercises = exercisesDb.Select(e => new
            {
                id = e.Id,
                name = e.Name,
                main_muscle = e.MainMuscle ?? "",
                equipment = e.Equipment ?? ""
            }).ToList();

            return formattedExercises;
        }
    }
}