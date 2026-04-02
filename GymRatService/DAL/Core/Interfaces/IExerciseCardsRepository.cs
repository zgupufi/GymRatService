using GymRatService.Common.Models;

namespace GymRatService.DAL.Core.Interfaces
{
    public interface IExerciseCardsRepository
    {
        Task<List<ExerciseCard>>GetExercisesAsync();
        Task<List<ExerciseCard>> SearchExercisesAsync(string query);

    }
}
