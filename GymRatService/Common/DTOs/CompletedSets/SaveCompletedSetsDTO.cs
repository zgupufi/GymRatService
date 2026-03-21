namespace GymRatService.Common.DTOs.CompletedSets
{
    public class SaveCompletedSetsDTO
    {
        public Guid WorkoutId { get; set; }
        public int ExerciseCardId { get; set; }
        public List<CompletedSetItemDTO> Sets { get; set; }
    }

    public class CompletedSetItemDTO
    {
        public int SetNumber { get; set; }
        public double WeightKg { get; set; }
        public int Reps { get; set; }
    }

}
