using GymRatService.Common.Models;

namespace GymRatService.DAL.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?>FindUserByEmailAsync(string email);
        Task<User> RegisterUserAsync(User user);
    }
}
