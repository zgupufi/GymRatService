using GymRatService.Common.DTOs.Auth;
using GymRatService.Common.Models;

namespace GymRatService.BLL.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<User> RegisterAsync(RegisterRequest registerRequest);
}