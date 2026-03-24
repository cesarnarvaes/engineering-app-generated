using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using BizCrm.Backend.DTOs;
using BizCrm.Backend.Models;
using BizCrm.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace BizCrm.Backend.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login attempt failed: User not found or inactive for email {Email}", request.Email);
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login attempt failed: Invalid password for email {Email}", request.Email);
                return null;
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token (in production, store in database with expiry)
            // For now, we'll include it but not persist it

            var userDto = new UserDto(
                user.Id,
                user.Email!,
                user.FirstName ?? "",
                user.LastName ?? "",
                user.FullName,
                roles.ToArray(),
                user.Department,
                user.JobTitle,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt
            );

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new LoginResponse(
                token,
                refreshToken,
                DateTime.UtcNow.AddMinutes(60), // Token expires in 1 hour
                userDto
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", request.Email);
            return null;
        }
    }

    public async Task<UserDto?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                _logger.LogWarning("Registration attempt failed: User already exists with email {Email}", request.Email);
                return null;
            }

            // Validate role exists
            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                _logger.LogWarning("Registration attempt failed: Invalid role {Role}", request.Role);
                return null;
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                JobTitle = request.JobTitle,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for email {Email}: {Errors}", 
                    request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, request.Role);

            _logger.LogInformation("User {UserId} registered successfully with role {Role}", user.Id, request.Role);

            return new UserDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.FullName,
                new[] { request.Role },
                user.Department,
                user.JobTitle,
                user.IsActive,
                user.CreatedAt,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", request.Email);
            return null;
        }
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        // In a production app, you would:
        // 1. Validate the refresh token from database
        // 2. Check if it's expired
        // 3. Get the associated user
        // 4. Generate new tokens
        
        // For this demo, we'll implement a basic version
        await Task.CompletedTask;
        return null; // Implement as needed
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return true;
            }

            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that user doesn't exist
                return true;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // In production, send email with reset link
            // For demo, just log it
            _logger.LogInformation("Password reset token generated for user {UserId}: {Token}", user.Id, token);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating password reset for email {Email}", email);
            return false;
        }
    }

    public async Task LogoutAsync(string userId)
    {
        try
        {
            // In production, invalidate refresh tokens for this user
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserId} logged out", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
        }
    }

    public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatShouldBeFromKeyVault_MinimumLength32Characters";
        var key = Encoding.UTF8.GetBytes(secretKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("firstName", user.FirstName ?? ""),
            new("lastName", user.LastName ?? ""),
            new("fullName", user.FullName),
            new("department", user.Department ?? ""),
            new("jobTitle", user.JobTitle ?? "")
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
            Issuer = jwtSettings["Issuer"] ?? "BizCrmAPI",
            Audience = jwtSettings["Audience"] ?? "BizCrmClient",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}