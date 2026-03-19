using System.Linq;
using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.Models;
using GymRatService.DAL.Core.Interfaces;

namespace GymRatService.BLL.Services
{
    public class ExercisesService : IExercisesService
    {
        private readonly IExerciseCardsRepository _handler;
        public ExercisesService(IExerciseCardsRepository handler) 
        {
            _handler = handler;
        }
        public async Task<List<ExerciseCard>> GetExercisesAsync()
        {
            var exercises = await _handler.GetExercisesAsync().ConfigureAwait(false);

            if (exercises is null)
            {
                return null;
            }

            if (exercises is List<ExerciseCard> list)
            {
                return list;
            }

            return exercises.ToList();
        }
    }
}
