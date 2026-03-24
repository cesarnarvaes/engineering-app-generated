using System.ComponentModel.DataAnnotations;
using BizCrm.Backend.Models;

namespace BizCrm.Backend.DTOs;

// Contact DTOs
public record ContactDto(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? JobTitle,
    string? Department,
    string? Notes,
    ContactStatus Status,
    int? CompanyId,
    string? CompanyName,
    DateTime CreatedAt,
    string CreatedBy
);

public record CreateContactRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [EmailAddress] string? Email,
    string? PhoneNumber,
    string? JobTitle,
    string? Department,
    string? Notes,
    ContactStatus Status,
    int? CompanyId
);

public record UpdateContactRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [EmailAddress] string? Email,
    string? PhoneNumber,
    string? JobTitle,
    string? Department,
    string? Notes,
    ContactStatus Status,
    int? CompanyId
);

// Company DTOs
public record CompanyDto(
    int Id,
    string Name,
    string? Industry,
    string? CompanySize,
    string? Website,
    string? PhoneNumber,
    string? Email,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateProvince,
    string? PostalCode,
    string? Country,
    decimal? AnnualRevenue,
    string? Notes,
    CompanyStatus Status,
    DateTime CreatedAt,
    string CreatedBy,
    int ContactCount,
    int OpportunityCount
);

public record CreateCompanyRequest(
    [Required] string Name,
    string? Industry,
    string? CompanySize,
    string? Website,
    string? PhoneNumber,
    [EmailAddress] string? Email,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateProvince,
    string? PostalCode,
    string? Country,
    decimal? AnnualRevenue,
    string? Notes,
    CompanyStatus Status
);

public record UpdateCompanyRequest(
    [Required] string Name,
    string? Industry,
    string? CompanySize,
    string? Website,
    string? PhoneNumber,
    [EmailAddress] string? Email,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateProvince,
    string? PostalCode,
    string? Country,
    decimal? AnnualRevenue,
    string? Notes,
    CompanyStatus Status
);

// Opportunity DTOs
public record OpportunityDto(
    int Id,
    string Title,
    decimal EstimatedValue,
    int ProbabilityPercentage,
    OpportunityStage Stage,
    OpportunityStatus Status,
    DateTime? ExpectedCloseDate,
    DateTime? ActualCloseDate,
    string? Description,
    string? Notes,
    int? ContactId,
    string? ContactName,
    int? CompanyId,
    string? CompanyName,
    string AssignedToId,
    string AssignedToName,
    DateTime CreatedAt,
    string CreatedBy
);

public record CreateOpportunityRequest(
    [Required] string Title,
    [Range(0, double.MaxValue)] decimal EstimatedValue,
    [Range(0, 100)] int ProbabilityPercentage,
    OpportunityStage Stage,
    DateTime? ExpectedCloseDate,
    string? Description,
    string? Notes,
    int? ContactId,
    int? CompanyId,
    [Required] string AssignedToId
);

public record UpdateOpportunityRequest(
    [Required] string Title,
    [Range(0, double.MaxValue)] decimal EstimatedValue,
    [Range(0, 100)] int ProbabilityPercentage,
    OpportunityStage Stage,
    OpportunityStatus Status,
    DateTime? ExpectedCloseDate,
    DateTime? ActualCloseDate,
    string? Description,
    string? Notes,
    int? ContactId,
    int? CompanyId,
    [Required] string AssignedToId
);

// Activity DTOs
public record ActivityDto(
    int Id,
    string Subject,
    string? Description,
    ActivityType Type,
    ActivityStatus Status,
    ActivityPriority Priority,
    DateTime? ScheduledDateTime,
    DateTime? CompletedDateTime,
    int? DurationMinutes,
    string? Location,
    string? Notes,
    int? ContactId,
    string? ContactName,
    int? CompanyId,
    string? CompanyName,
    int? OpportunityId,
    string? OpportunityTitle,
    string AssignedToId,
    string AssignedToName,
    DateTime CreatedAt,
    string CreatedBy
);

public record CreateActivityRequest(
    [Required] string Subject,
    string? Description,
    ActivityType Type,
    ActivityPriority Priority,
    DateTime? ScheduledDateTime,
    int? DurationMinutes,
    string? Location,
    string? Notes,
    int? ContactId,
    int? CompanyId,
    int? OpportunityId,
    [Required] string AssignedToId
);

public record UpdateActivityRequest(
    [Required] string Subject,
    string? Description,
    ActivityType Type,
    ActivityStatus Status,
    ActivityPriority Priority,
    DateTime? ScheduledDateTime,
    DateTime? CompletedDateTime,
    int? DurationMinutes,
    string? Location,
    string? Notes,
    int? ContactId,
    int? CompanyId,
    int? OpportunityId,
    [Required] string AssignedToId
);

// Standard response wrapper
public record ApiResponse<T>(
    T Data,
    bool Success = true,
    string? Message = null,
    string[]? Errors = null
);

public record PagedResponse<T>(
    IEnumerable<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool Success = true,
    string? Message = null
);