using GymRatService.Common.Models;

namespace GymRatService.BLL.Core.Interfaces
{
    public interface IExercisesService
    {
        Task<List<Exercise>> GetExercisesAsync();
    }
}
