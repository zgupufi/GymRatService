using System.Linq;
using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.Models;
using GymRatService.DAL.Core;

namespace GymRatService.BLL.Services
{
    public class ExercisesService : IExercisesService
    {
        private readonly IExercisesHandler _handler;
        public ExercisesService(IExercisesHandler handler) 
        {
            _handler = handler;
        }
        public async Task<List<Exercise>> GetExercisesAsync()
        {
            var exercises = await _handler.GetExercisesAsync().ConfigureAwait(false);

            if (exercises is null)
            {
                return null;
            }

            if (exercises is List<Exercise> list)
            {
                return list;
            }

            return exercises.ToList();
        }
    }
}
