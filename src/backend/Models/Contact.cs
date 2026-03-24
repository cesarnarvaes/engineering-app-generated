using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BizCrm.Backend.Models;

public class Contact
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }
    
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(200)]
    public string? JobTitle { get; set; }
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public ContactStatus Status { get; set; } = ContactStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign Keys
    public int? CompanyId { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string? LastModifiedById { get; set; }
    
    // Navigation Properties
    [ForeignKey("CompanyId")]
    public Company? Company { get; set; }
    
    [ForeignKey("CreatedById")]
    public ApplicationUser CreatedBy { get; set; } = null!;
    
    [ForeignKey("LastModifiedById")]
    public ApplicationUser? LastModifiedBy { get; set; }
    
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public enum ContactStatus
{
    Active = 1,
    Inactive = 2,
    Lead = 3,
    Customer = 4,
    Prospect = 5
}