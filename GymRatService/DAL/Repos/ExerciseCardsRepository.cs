using System.Collections.Generic;
using System.Threading.Tasks;
using GymRatService.Common.Models;
using GymRatService.DAL.Core;
using GymRatService.DAL.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymRatService.DAL.Repos
{
    public class ExerciseCardsRepository : IExerciseCardsRepository
    {
        private readonly DBContext _context;

        public ExerciseCardsRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<List<ExerciseCard>> GetExercisesAsync()
        {
            return await _context.Exercises.ToListAsync();
        }
    }
}
