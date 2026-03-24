using System.ComponentModel.DataAnnotations;

namespace BizCrm.Backend.DTOs;

public record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);

public record RegisterRequest(
    [Required] [EmailAddress] string Email,
    [Required] [MinLength(8)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    string? Department,
    string? JobTitle,
    [Required] string Role
);

public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required] [MinLength(8)] string NewPassword
);

public record ResetPasswordRequest(
    [Required] [EmailAddress] string Email
);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string[] Roles,
    string? Department,
    string? JobTitle,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);