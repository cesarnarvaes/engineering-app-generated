using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BizCrm.Backend.DTOs;
using BizCrm.Backend.Models;
using BizCrm.Backend.Services;
using System.Security.Claims;

namespace BizCrm.Backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Login
        auth.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            
            if (result == null)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Invalid email or password"));
            }
            
            return Results.Ok(new ApiResponse<LoginResponse>(result, true, "Login successful"));
        }).AllowAnonymous();

        // Register
        auth.MapPost("/register", [Authorize(Roles = "systemadmin")] async (RegisterRequest request, IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(request);
            
            if (result == null)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Registration failed"));
            }
            
            return Results.Created("", new ApiResponse<UserDto>(result, true, "User registered successfully"));
        });

        // Refresh token
        auth.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request.RefreshToken);
            
            if (result == null)
            {
                return Results.Unauthorized();
            }
            
            return Results.Ok(new ApiResponse<LoginResponse>(result, true, "Token refreshed successfully"));
        }).AllowAnonymous();

        // Change password
        auth.MapPost("/change-password", [Authorize] async (ChangePasswordRequest request, IAuthService authService, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Results.Unauthorized();
            }
            
            var success = await authService.ChangePasswordAsync(userId, request);
            
            if (!success)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Failed to change password"));
            }
            
            return Results.Ok(new ApiResponse<object?>(null, true, "Password changed successfully"));
        });

        // Reset password
        auth.MapPost("/reset-password", async (ResetPasswordRequest request, IAuthService authService) =>
        {
            var success = await authService.ResetPasswordAsync(request.Email);
            
            // Always return success to prevent email enumeration
            return Results.Ok(new ApiResponse<object?>(null, true, "If the email exists, a password reset link has been sent"));
        }).AllowAnonymous();

        // Logout
        auth.MapPost("/logout", [Authorize] async (IAuthService authService, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await authService.LogoutAsync(userId);
            }
            
            return Results.Ok(new ApiResponse<object?>(null, true, "Logged out successfully"));
        });

        // Get current user info
        auth.MapGet("/me", [Authorize] async (ClaimsPrincipal user, UserManager<ApplicationUser> userManager) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Results.Unauthorized();
            }
            
            var currentUser = await userManager.FindByIdAsync(userId);
            if (currentUser == null)
            {
                return Results.NotFound();
            }
            
            var roles = await userManager.GetRolesAsync(currentUser);
            
            var userDto = new UserDto(
                currentUser.Id,
                currentUser.Email!,
                currentUser.FirstName ?? "",
                currentUser.LastName ?? "",
                currentUser.FullName,
                roles.ToArray(),
                currentUser.Department,
                currentUser.JobTitle,
                currentUser.IsActive,
                currentUser.CreatedAt,
                currentUser.LastLoginAt
            );
            
            return Results.Ok(new ApiResponse<UserDto>(userDto, true));
        });

        // Check if email exists
        auth.MapGet("/check-email/{email}", [Authorize(Roles = "systemadmin")] async (string email, UserManager<ApplicationUser> userManager) =>
        {
            var exists = await userManager.FindByEmailAsync(email) != null;
            return Results.Ok(new ApiResponse<bool>(exists, true));
        });
    }
}