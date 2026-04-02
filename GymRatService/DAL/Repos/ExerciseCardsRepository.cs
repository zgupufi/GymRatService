using System.Collections.Generic;
using System.Linq; // Adăugat pentru extensia .Where()
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

        public async Task<List<ExerciseCard>> SearchExercisesAsync(string query)
        {
            // 1. Verificare de siguranță
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<ExerciseCard>();
            }

            // 2. Transformăm termenul căutat de AI în litere mici
            var lowerQuery = query.ToLower();

            // 3. Transformăm și coloanele din baza de date în litere mici (.ToLower()) 
            // înainte să aplicăm .Contains()
            return await _context.Exercises
                .Where(e => e.Name.ToLower().Contains(lowerQuery) ||
                           (e.MainMuscle != null && e.MainMuscle.ToLower().Contains(lowerQuery)))
                .ToListAsync();
        }
    }
}