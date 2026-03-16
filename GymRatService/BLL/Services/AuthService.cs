using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs;
using GymRatService.Common.Models;
using GymRatService.DAL.Core;
using GymRatService.DAL.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GymRatService.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly IQueryHandler _queryHandler;

    public AuthService(IConfiguration config, IQueryHandler queryHandler)
    {
        _config = config;
        _queryHandler = queryHandler;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        if (request == null)
            return null;

        var user = await _queryHandler.FindUserByEmailAsync(request.Email);

        if (user == null)
        {
            return null;
        }

        bool verified;
        try
        {
            verified = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return null;
        }

        if (!verified)
        {
            return null;
        }

        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            Email = user.Email
        };
    }


    /// <summary>
    /// PasswordHash when the user registers is actually the plainPassword - add a DTO for a register request
    /// </summary>
    /// <param name="registerRequest"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<User> RegisterAsync(RegisterRequest registerRequest)
    {
        if (registerRequest == null)
            throw new ArgumentNullException(nameof(registerRequest));

        if (string.IsNullOrWhiteSpace(registerRequest.Email))
            throw new ArgumentException("Email is required.", nameof(registerRequest));

        if (string.IsNullOrWhiteSpace(registerRequest.Password))
            throw new ArgumentException("Password is required.", nameof(registerRequest));

        // Check if the user already exists
        var existing = await _queryHandler.FindUserByEmailAsync(registerRequest.Email);
        if (existing != null)
            throw new InvalidOperationException("A user with this email already exists.");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

        var userToCreate = new User
        {
            Email = registerRequest.Email,
            PasswordHash = hashedPassword,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            PreferredWeightUnit = registerRequest.PreferredWeightUnit,
            CreatedAt = DateTime.UtcNow,
        };

        return await _queryHandler.RegisterUserAsync(userToCreate);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"]
            ?? throw new InvalidOperationException("JWT Key is missing from configuration!");

        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        }),
            Expires = DateTime.UtcNow.AddHours(6), 
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}