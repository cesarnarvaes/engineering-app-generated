using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BizCrm.Backend.Models;

namespace BizCrm.Backend.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure entity relationships and constraints
        
        // Contact relationships
        builder.Entity<Contact>()
            .HasOne(c => c.Company)
            .WithMany(comp => comp.Contacts)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Contact>()
            .HasOne(c => c.CreatedBy)
            .WithMany(u => u.CreatedContacts)
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Company relationships
        builder.Entity<Company>()
            .HasOne(c => c.CreatedBy)
            .WithMany(u => u.CreatedCompanies)
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Opportunity relationships
        builder.Entity<Opportunity>()
            .HasOne(o => o.Contact)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(o => o.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Opportunity>()
            .HasOne(o => o.Company)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Opportunity>()
            .HasOne(o => o.AssignedTo)
            .WithMany(u => u.AssignedOpportunities)
            .HasForeignKey(o => o.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(o => o.CreatedBy)
            .WithMany()
            .HasForeignKey(o => o.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(o => o.LastModifiedBy)
            .WithMany()
            .HasForeignKey(o => o.LastModifiedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Activity relationships
        builder.Entity<Activity>()
            .HasOne(a => a.Contact)
            .WithMany(c => c.Activities)
            .HasForeignKey(a => a.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(a => a.Company)
            .WithMany(c => c.Activities)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(a => a.Opportunity)
            .WithMany(o => o.Activities)
            .HasForeignKey(a => a.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(a => a.AssignedTo)
            .WithMany()
            .HasForeignKey(a => a.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(a => a.LastModifiedBy)
            .WithMany()
            .HasForeignKey(a => a.LastModifiedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.Entity<Contact>()
            .HasIndex(c => new { c.Email })
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.Entity<Contact>()
            .HasIndex(c => new { c.FirstName, c.LastName });

        builder.Entity<Company>()
            .HasIndex(c => c.Name);

        builder.Entity<Opportunity>()
            .HasIndex(o => new { o.Stage, o.Status });

        builder.Entity<Activity>()
            .HasIndex(a => new { a.ScheduledDateTime, a.AssignedToId });

        // Configure decimal precision
        builder.Entity<Company>()
            .Property(c => c.AnnualRevenue)
            .HasPrecision(18, 2);

        builder.Entity<Opportunity>()
            .Property(o => o.EstimatedValue)
            .HasPrecision(18, 2);

        // Configure enum storage
        builder.Entity<Contact>()
            .Property(c => c.Status)
            .HasConversion<int>();

        builder.Entity<Company>()
            .Property(c => c.Status)
            .HasConversion<int>();

        builder.Entity<Opportunity>()
            .Property(o => o.Stage)
            .HasConversion<int>();

        builder.Entity<Opportunity>()
            .Property(o => o.Status)
            .HasConversion<int>();

        builder.Entity<Activity>()
            .Property(a => a.Type)
            .HasConversion<int>();

        builder.Entity<Activity>()
            .Property(a => a.Status)
            .HasConversion<int>();

        builder.Entity<Activity>()
            .Property(a => a.Priority)
            .HasConversion<int>();
    }
}