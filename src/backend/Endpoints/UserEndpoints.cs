using Microsoft.AspNetCore.Authorization;
using BizCrm.Backend.DTOs;
using BizCrm.Backend.Services;
using System.Security.Claims;

namespace BizCrm.Backend.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users")
            .RequireAuthorization()
            .WithTags("User Management")
            .WithOpenApi();

        // Get users with pagination and filtering
        users.MapGet("/", [Authorize(Roles = "manager,systemadmin")] async (
            IUserService userService,
            int page = 1,
            int pageSize = 10,
            string? search = null,
            string? role = null) =>
        {
            var result = await userService.GetUsersAsync(page, pageSize, search, role);
            return Results.Ok(result);
        });

        // Get user by ID
        users.MapGet("/{userId}", [Authorize] async (string userId, IUserService userService, ClaimsPrincipal currentUser) =>
        {
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRoles = currentUser.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            
            // Users can view their own profile, managers+ can view anyone
            if (currentUserId != userId && !currentUserRoles.Intersect(new[] { "manager", "systemadmin" }).Any())
            {
                return Results.Forbid();
            }
            
            var user = await userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return Results.NotFound(new ApiResponse<object?>(null, false, "User not found"));
            }
            
            return Results.Ok(new ApiResponse<UserDto>(user, true));
        });

        // Update user
        users.MapPut("/{userId}", [Authorize] async (
            string userId,
            UpdateUserRequest request,
            IUserService userService,
            ClaimsPrincipal currentUser) =>
        {
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRoles = currentUser.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            
            // Users can update their own profile, managers+ can update anyone
            if (currentUserId != userId && !currentUserRoles.Intersect(new[] { "manager", "systemadmin" }).Any())
            {
                return Results.Forbid();
            }
            
            var user = await userService.UpdateUserAsync(userId, request);
            
            if (user == null)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Failed to update user"));
            }
            
            return Results.Ok(new ApiResponse<UserDto>(user, true, "User updated successfully"));
        });

        // Deactivate user
        users.MapPost("/{userId}/deactivate", [Authorize(Roles = "systemadmin")] async (string userId, IUserService userService) =>
        {
            var success = await userService.DeactivateUserAsync(userId);
            
            if (!success)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Failed to deactivate user"));
            }
            
            return Results.Ok(new ApiResponse<object?>(null, true, "User deactivated successfully"));
        });

        // Activate user
        users.MapPost("/{userId}/activate", [Authorize(Roles = "systemadmin")] async (string userId, IUserService userService) =>
        {
            var success = await userService.ActivateUserAsync(userId);
            
            if (!success)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Failed to activate user"));
            }
            
            return Results.Ok(new ApiResponse<object?>(null, true, "User activated successfully"));
        });

        // Get user roles
        users.MapGet("/{userId}/roles", [Authorize(Roles = "manager,systemadmin")] async (string userId, IUserService userService) =>
        {
            var roles = await userService.GetUserRolesAsync(userId);
            return Results.Ok(new ApiResponse<string[]>(roles, true));
        });

        // Assign role to user
        users.MapPost("/{userId}/roles/{roleName}", [Authorize(Roles = "systemadmin")] async (
            string userId,
            string roleName,
            IUserService userService) =>
        {
            var success = await userService.AssignRoleAsync(userId, roleName);
            
            if (!success)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Failed to assign role"));
            }
            
            return Results.Ok(new ApiResponse<object?>(null, true, $"Role '{roleName}' assigned successfully"));
        });

        // Remove role from user
        users.MapDelete("/{userId}/roles/{roleName}", [Authorize(Roles = "systemadmin")] async (
            string userId,
            string roleName,
            IUserService userService) =>
        {
            var success = await userService.RemoveRoleAsync(userId, roleName);
            
            if (!success)
            {
                return Results.BadRequest(new ApiResponse<object?>(null, false, "Failed to remove role"));
            }
            
            return Results.Ok(new ApiResponse<object?>(null, true, $"Role '{roleName}' removed successfully"));
        });

        // Get available roles
        users.MapGet("/roles/available", [Authorize(Roles = "systemadmin")] async (IUserService userService) =>
        {
            var roles = await userService.GetAvailableRolesAsync();
            return Results.Ok(new ApiResponse<string[]>(roles, true));
        });
    }
}