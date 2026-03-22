using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.Models;
using GymRatService.DAL.Core.Interfaces;

namespace GymRatService.BLL.Services
{
    public class PersonalizedSplitsService:IPersonalizedSplitsService
    {
        private readonly IPersonalizedSplitsRepository _personalizedSplitsRepository;
        public PersonalizedSplitsService(IPersonalizedSplitsRepository personalizedSplitsRepository)
        {
            _personalizedSplitsRepository = personalizedSplitsRepository;
        }

        public async Task<UserWeeklySplit> GetUserWeeklySplitByUserIdAsync(string userId)
        {
            var weeklySplit = await _personalizedSplitsRepository.GetUserWeeklySplitByUserIdAsync(userId);  

            if (weeklySplit == null)
            {
                weeklySplit = new UserWeeklySplit
                {
                    UserId = userId,
                    MondayWorkoutId = null,
                    TuesdayWorkoutId = null,
                    WednesdayWorkoutId = null,
                    ThursdayWorkoutId = null,
                    FridayWorkoutId = null,
                    SaturdayWorkoutId = null,
                    SundayWorkoutId = null
                };
            }
            return weeklySplit;
        }

        public async Task<UserWeeklySplit> SaveUserWeeklySplitAsync(UserWeeklySplit personalizedSplit)
        {
            var savedSplit = await _personalizedSplitsRepository.SaveUserWeeklySplitAsync(personalizedSplit);
            return savedSplit;
        }
    }
}
