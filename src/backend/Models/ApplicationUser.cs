using Microsoft.AspNetCore.Identity;

namespace BizCrm.Backend.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Navigation properties
    public ICollection<Contact> CreatedContacts { get; set; } = new List<Contact>();
    public ICollection<Company> CreatedCompanies { get; set; } = new List<Company>();
    public ICollection<Opportunity> AssignedOpportunities { get; set; } = new List<Opportunity>();
}