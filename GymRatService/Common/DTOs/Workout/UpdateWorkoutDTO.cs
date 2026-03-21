namespace GymRatService.Common.DTOs.Workout
{
    public class UpdateWorkoutDTO
    {
        public string Name { get; set; }
        public List<UpdateWorkoutExerciseDTO> Exercises { get; set; } = new();
    }
}

