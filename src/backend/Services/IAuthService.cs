using BizCrm.Backend.DTOs;
using BizCrm.Backend.Models;

namespace BizCrm.Backend.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UserDto?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<bool> ResetPasswordAsync(string email);
    Task LogoutAsync(string userId);
    string GenerateJwtToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
}