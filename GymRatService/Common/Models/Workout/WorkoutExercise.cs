namespace GymRatService.Common.Models.Workout
{
    public class WorkoutExercise
    {
        public Guid Id { get; set; }

        // 1. Prima referință: De care antrenament aparține?
        public Guid WorkoutId { get; set; }
        public Workout Workout { get; set; }

        // 2. A doua referință: Care este exercițiul global? (ex: Id-ul pentru "Împins la piept")
        public int ExerciseCardId { get; set; }
        public ExerciseCard ExerciseCard { get; set; }

        // Lista cu seriile (sets), greutatea și repetările pentru ACEST exercițiu 
        public int OrderIndex { get; set; }
        public List<WorkoutSet> Sets { get; set; } = new();

    }

}
