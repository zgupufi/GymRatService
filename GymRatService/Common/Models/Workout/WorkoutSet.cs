namespace GymRatService.Common.Models.Workout
{
    public class WorkoutSet
    {
        public Guid Id { get; set; }

        // Referința către exercițiul din antrenamentul curent
        public Guid WorkoutExerciseId { get; set; }
        public WorkoutExercise WorkoutExercise { get; set; }

        public int SetNumber { get; set; }
        public double WeightKg { get; set; } 
        public int Reps { get; set; } 

        public bool IsCompleted { get; set; } = false; // Dacă a bifat-o sau nu
    }

}
