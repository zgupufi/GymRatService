namespace GymRatService.Common.DTOs.Workout
{
    public class UpdateWorkoutExerciseDTO
    {
        public int Id { get; set; }

        // FOARTE IMPORTANT: Trebuie să fie un "int" simplu numit TargetSets, 
        // pentru că React trimite JSON: { "id": 14, "targetSets": 4 }
        public int TargetSets { get; set; }
    }
}
