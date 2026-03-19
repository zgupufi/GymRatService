namespace GymRatService.Common.Models.Workout
{
    public class Workout
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } // Legătura cu Userul

        // Lista de exerciții care aparțin DOAR de acest antrenament
        public List<WorkoutExercise> WorkoutExercises { get; set; } = new();
    }

}
