using GymRatService.Common.Models;

namespace GymRatService.BLL.Core.Interfaces
{
    public interface IPersonalizedSplitsService
    {
        Task<UserWeeklySplit> GetUserWeeklySplitByUserIdAsync(string userId);
        Task<UserWeeklySplit> SaveUserWeeklySplitAsync(UserWeeklySplit personalizedSplit);
    }
}
