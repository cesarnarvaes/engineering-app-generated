using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BizCrm.Backend.Models;

public class Opportunity
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedValue { get; set; }
    
    [Range(0, 100)]
    public int ProbabilityPercentage { get; set; }
    
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign Keys
    public int? ContactId { get; set; }
    public int? CompanyId { get; set; }
    public string AssignedToId { get; set; } = string.Empty;
    public string CreatedById { get; set; } = string.Empty;
    public string? LastModifiedById { get; set; }
    
    // Navigation Properties
    [ForeignKey("ContactId")]
    public Contact? Contact { get; set; }
    
    [ForeignKey("CompanyId")]
    public Company? Company { get; set; }
    
    [ForeignKey("AssignedToId")]
    public ApplicationUser AssignedTo { get; set; } = null!;
    
    [ForeignKey("CreatedById")]
    public ApplicationUser CreatedBy { get; set; } = null!;
    
    [ForeignKey("LastModifiedById")]
    public ApplicationUser? LastModifiedBy { get; set; }
    
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public enum OpportunityStage
{
    Prospecting = 1,
    Qualification = 2,
    NeedsAnalysis = 3,
    ValueProposition = 4,
    ProposalPriceQuote = 5,
    Negotiation = 6,
    ClosedWon = 7,
    ClosedLost = 8
}

public enum OpportunityStatus
{
    Open = 1,
    Won = 2,
    Lost = 3,
    Canceled = 4
}