using System.Collections.Generic;
using System.Threading.Tasks;
using GymRatService.Common.Models;
using GymRatService.DAL.Core;
using Microsoft.EntityFrameworkCore;

namespace GymRatService.DAL.Repos
{
    public class ExercisesHandler : IExercisesHandler
    {
        private readonly DBContext _context;

        public ExercisesHandler(DBContext context)
        {
            _context = context;
        }

        public async Task<List<Exercise>> GetExercisesAsync()
        {
            return await _context.Exercises.ToListAsync();
        }
    }
}
