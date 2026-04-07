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

            var existingExercisesPool = workout.WorkoutExercises.ToList();
            int currentOrder = 0;

            foreach (var exerciseDto in updateWorkoutDTO.Exercises)
            {
                var existingEx = existingExercisesPool.FirstOrDefault(e => e.ExerciseCardId == exerciseDto.Id);
                
                if (existingEx != null)
                {
                    existingExercisesPool.Remove(existingEx);
                    existingEx.OrderIndex = currentOrder++;

                    if (existingEx.Sets == null) existingEx.Sets = new List<WorkoutSet>();
                    
                    var currentSetsCount = existingEx.Sets.Count;
                    if (exerciseDto.TargetSets > currentSetsCount)
                    {
                        // Add new empty sets to reach target
                        for (int i = currentSetsCount + 1; i <= exerciseDto.TargetSets; i++)
                        {
                            existingEx.Sets.Add(new WorkoutSet
                            {
                                SetNumber = i,
                                WeightKg = 0,
                                Reps = 0,
                                IsCompleted = false
                            });
                        }
                    }
                    else if (exerciseDto.TargetSets < currentSetsCount)
                    {
                        // Remove excess sets from the end
                        var setsToRemove = existingEx.Sets.OrderByDescending(s => s.SetNumber).Take(currentSetsCount - exerciseDto.TargetSets).ToList();
                        foreach (var s in setsToRemove)
                        {
                            existingEx.Sets.Remove(s);
                        }
                    }
                }
                else
                {
                    // Create completely new exercise
                    var newExercise = new WorkoutExercise
                    {
                        ExerciseCardId = exerciseDto.Id,
                        OrderIndex = currentOrder++,
                        Sets = new List<WorkoutSet>()
                    };

                    for (int i = 1; i <= exerciseDto.TargetSets; i++)
                    {
                        newExercise.Sets.Add(new WorkoutSet
                        {
                            SetNumber = i,
                            WeightKg = 0,
                            Reps = 0,
                            IsCompleted = false
                        });
                    }
                    workout.WorkoutExercises.Add(newExercise);
                }
            }

            // Remove any exercises that were deleted by the user/bot
            foreach (var oldEx in existingExercisesPool)
            {
                workout.WorkoutExercises.Remove(oldEx);
            }

            await _workoutRepository.UpdateWorkoutAsync(workout);
            return workout;
        }

        public async Task<Object> GetFormattedWorkoutsByUserIdAsync(string userId)
        {
            var workoutsDb = await _workoutRepository.GetWorkoutsByUserIdAsync(userId);

            var formattedWorkouts = workoutsDb
                .OrderByDescending(w => w.Date)
                .Take(5)
                .Select(w => new
                {
                    id = w.Id,
                    name = w.Name,
                    dateCreated = w.Date,
                    exercises = (w.WorkoutExercises ?? new List<GymRatService.Common.Models.Workout.WorkoutExercise>())
                        .OrderBy(we => we.OrderIndex)
                        .Select(we => new
                        {
                            id = we.ExerciseCardId,
                            name = we.ExerciseCard?.Name ?? "Unknown Exercise",
                            muscleGroup = we.ExerciseCard?.MainMuscle ?? "",
                            targetSets = we.Sets != null ? we.Sets.Count : 0
                        }).ToList()
                }).ToList();

            return formattedWorkouts;
        }
        public async Task<object> GetWorkoutDetailsAsync(Guid workoutId, string userId)
        {
            var workout = await _workoutRepository.GetWorkoutByIdAndUserIdAsync(workoutId, userId);
            if (workout == null) return new { error = "Workout not found." };

            return new
            {
                id = workout.Id,
                name = workout.Name,
                exercises = workout.WorkoutExercises.OrderBy(we => we.OrderIndex).Select(we => new
                {
                    id = we.ExerciseCardId,
                    name = we.ExerciseCard.Name,
                    main_muscle = we.ExerciseCard?.MainMuscle ?? "",
                    equipment = we.ExerciseCard?.Equipment ?? "",
                    targetSets = we.Sets != null ? we.Sets.Count : 0
                }).ToList()
            };
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

            // Returnăm o listă extrem de concisă pentru ca AI-ul să o înțeleagă ușor (fără imagini, id-uri enervante sau descrieri lungi)
            var formattedExercises = exercisesDb.Take(7).Select(e => new 
            {
                id = e.Id,
                name = e.Name,
                muscleGroup = e.MainMuscle ?? ""
                // Scoatem 'equipment' și alte câmpuri care nu sunt absolut vitale pentru GymRat Coach (economisim tokeni!)
            }).ToList();

            return formattedExercises;
        }
    }
}