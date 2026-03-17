using GymRatService.Common.Models;

namespace GymRatService.DAL.Core
{
    public interface IExercisesHandler
    {
        Task<List<Exercise>>GetExercisesAsync();
    }
}
