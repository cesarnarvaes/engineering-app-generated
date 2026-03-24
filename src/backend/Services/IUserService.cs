using BizCrm.Backend.DTOs;

namespace BizCrm.Backend.Services;

public interface IUserService
{
    Task<PagedResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? role = null);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> ActivateUserAsync(string userId);
    Task<bool> AssignRoleAsync(string userId, string roleName);
    Task<bool> RemoveRoleAsync(string userId, string roleName);
    Task<string[]> GetUserRolesAsync(string userId);
    Task<string[]> GetAvailableRolesAsync();
}

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Department,
    string? JobTitle
);