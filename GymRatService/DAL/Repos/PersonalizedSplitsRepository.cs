using GymRatService.Common.Models;
using GymRatService.DAL.Core;
using GymRatService.DAL.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymRatService.DAL.Repos
{

    public class PersonalizedSplitsRepository:IPersonalizedSplitsRepository
    {
        private readonly DBContext _context;

        public PersonalizedSplitsRepository(DBContext Context)
        {
            _context = Context;
        }

        public async Task<UserWeeklySplit> GetUserWeeklySplitByUserIdAsync(string userId)
        {
            return _context.UserWeeklySplits.FirstOrDefault(x => x.UserId == userId);
        }

        public async Task<UserWeeklySplit> SaveUserWeeklySplitAsync(UserWeeklySplit personalizedSplit)
        {
            var existingSplit = await _context.UserWeeklySplits.FirstOrDefaultAsync(s => s.UserId == personalizedSplit.UserId);

            if (existingSplit == null)
            {
                var split = new UserWeeklySplit
                {
                    UserId = personalizedSplit.UserId,
                    MondayWorkoutId = personalizedSplit.MondayWorkoutId,
                    TuesdayWorkoutId = personalizedSplit.TuesdayWorkoutId,
                    WednesdayWorkoutId = personalizedSplit.WednesdayWorkoutId,
                    ThursdayWorkoutId = personalizedSplit.ThursdayWorkoutId,
                    FridayWorkoutId = personalizedSplit.FridayWorkoutId,
                    SaturdayWorkoutId = personalizedSplit.SaturdayWorkoutId,
                    SundayWorkoutId = personalizedSplit.SundayWorkoutId
                };
                await _context.UserWeeklySplits.AddAsync(split);
            }
            else
            {
                existingSplit.MondayWorkoutId = personalizedSplit.MondayWorkoutId;
                existingSplit.TuesdayWorkoutId = personalizedSplit.TuesdayWorkoutId;
                existingSplit.WednesdayWorkoutId = personalizedSplit.WednesdayWorkoutId;
                existingSplit.ThursdayWorkoutId = personalizedSplit.ThursdayWorkoutId;
                existingSplit.FridayWorkoutId = personalizedSplit.FridayWorkoutId;
                existingSplit.SaturdayWorkoutId = personalizedSplit.SaturdayWorkoutId;
                existingSplit.SundayWorkoutId = personalizedSplit.SundayWorkoutId;

                 _context.UserWeeklySplits.Update(existingSplit);
            }
            await _context.SaveChangesAsync();
            return personalizedSplit;

        }
    }
}
