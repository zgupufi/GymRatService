using GymRatService.Common.DTOs;
using GymRatService.Common.Models;

namespace GymRatService.BLL.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<User> RegisterAsync(RegisterRequest registerRequest);
}