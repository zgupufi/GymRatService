using GymRatService.Common.Models;

namespace GymRatService.DAL.Core.Interfaces
{
    public interface IPersonalizedSplitsRepository
    {
        Task<UserWeeklySplit> GetUserWeeklySplitByUserIdAsync(string userId);
        Task<UserWeeklySplit> SaveUserWeeklySplitAsync(UserWeeklySplit personalizedSplit);
    }
}
