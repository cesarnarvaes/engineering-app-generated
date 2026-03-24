using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BizCrm.Backend.Models;

public class Company
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Industry { get; set; }
    
    [StringLength(50)]
    public string? CompanySize { get; set; }
    
    [StringLength(255)]
    public string? Website { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }
    
    // Address Information
    [StringLength(200)]
    public string? AddressLine1 { get; set; }
    
    [StringLength(200)]
    public string? AddressLine2 { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(50)]
    public string? StateProvince { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AnnualRevenue { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public CompanyStatus Status { get; set; } = CompanyStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign Keys
    public string CreatedById { get; set; } = string.Empty;
    public string? LastModifiedById { get; set; }
    
    // Navigation Properties
    [ForeignKey("CreatedById")]
    public ApplicationUser CreatedBy { get; set; } = null!;
    
    [ForeignKey("LastModifiedById")]
    public ApplicationUser? LastModifiedBy { get; set; }
    
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public enum CompanyStatus
{
    Active = 1,
    Inactive = 2,
    Prospect = 3,
    Customer = 4,
    Partner = 5
}