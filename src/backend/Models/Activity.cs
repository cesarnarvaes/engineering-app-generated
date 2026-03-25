using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BizCrm.Backend.Models;

public class Activity
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public ActivityType Type { get; set; }
    
    public ActivityStatus Status { get; set; } = ActivityStatus.Scheduled;
    
    public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;
    
    public DateTime? ScheduledDateTime { get; set; }
    public DateTime? CompletedDateTime { get; set; }
    
    [Range(0, 1440)] // Max 24 hours in minutes
    public int? DurationMinutes { get; set; }
    
    [StringLength(500)]
    public string? Location { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign Keys
    public int? ContactId { get; set; }
    public int? CompanyId { get; set; }
    public int? OpportunityId { get; set; }
    public string AssignedToId { get; set; } = string.Empty;
    public string CreatedById { get; set; } = string.Empty;
    public string? LastModifiedById { get; set; }
    
    // Navigation Properties
    [ForeignKey("ContactId")]
    public Contact? Contact { get; set; }
    
    [ForeignKey("CompanyId")]
    public Company? Company { get; set; }
    
    [ForeignKey("OpportunityId")]
    public Opportunity? Opportunity { get; set; }
    
    [ForeignKey("AssignedToId")]
    public ApplicationUser AssignedTo { get; set; } = null!;
    
    [ForeignKey("CreatedById")]
    public ApplicationUser CreatedBy { get; set; } = null!;
    
    [ForeignKey("LastModifiedById")]
    public ApplicationUser? LastModifiedBy { get; set; }
}

public enum ActivityType
{
    Call = 1,
    Email = 2,
    Meeting = 3,
    Task = 4,
    Appointment = 5,
    Note = 6,
    FollowUp = 7
}

public enum ActivityStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Canceled = 4,
    Overdue = 5
}

public enum ActivityPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}