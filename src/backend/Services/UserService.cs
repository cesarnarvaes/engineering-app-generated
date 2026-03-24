using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BizCrm.Backend.DTOs;
using BizCrm.Backend.Models;
using BizCrm.Backend.Data;

namespace BizCrm.Backend.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? role = null)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(u => 
                    (u.FirstName != null && u.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(lowerSearchTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(lowerSearchTerm)) ||
                    (u.Department != null && u.Department.ToLower().Contains(lowerSearchTerm)));
            }

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => userIds.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto(
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
                ));
            }

            return new PagedResponse<UserDto>(
                userDtos,
                page,
                pageSize,
                totalCount,
                totalPages
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return new PagedResponse<UserDto>(Enumerable.Empty<UserDto>(), page, pageSize, 0, 0, false, "Error retrieving users");
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto(
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto?> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Department = request.Department;
            user.JobTitle = request.JobTitle;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to update user {UserId}: {Errors}", userId, 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.FullName,
                roles.ToArray(),
                user.Department,
                user.JobTitle,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} deactivated", userId);
            }
            
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ActivateUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} activated", userId);
            }
            
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _roleManager.RoleExistsAsync(roleName))
            {
                return false;
            }

            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                return true; // Already has role
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleName} assigned to user {UserId}", roleName, userId);
            }
            
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
            return false;
        }
    }

    public async Task<bool> RemoveRoleAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                return true; // User doesn't have role
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleName} removed from user {UserId}", roleName, userId);
            }
            
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
            return false;
        }
    }

    public async Task<string[]> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Array.Empty<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            return Array.Empty<string>();
        }
    }

    public async Task<string[]> GetAvailableRolesAsync()
    {
        try
        {
            var roles = await _roleManager.Roles.Select(r => r.Name!).ToArrayAsync();
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available roles");
            return Array.Empty<string>();
        }
    }
}