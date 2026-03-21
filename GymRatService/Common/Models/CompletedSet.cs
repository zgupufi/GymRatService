namespace GymRatService.Common.Models
{
    public class CompletedSet
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }            
        public Guid WorkoutId { get; set; }             
        public int ExerciseCardId { get; set; }          // Ce exercițiu
        public int SetNumber { get; set; }               
        public double WeightKg { get; set; }             
        public int Reps { get; set; }                    
        public DateTime CompletedAt { get; set; }        
    }

}
