using GymRatService.Common.Models;

namespace GymRatService.DAL.Core
{
    public interface IQueryHandler
    {
        Task<User?>FindUserByEmailAsync(string email);
        Task<User> RegisterUserAsync(User user);
    }
}
