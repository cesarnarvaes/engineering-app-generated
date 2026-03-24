using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BizCrm.Backend.Data;
using BizCrm.Backend.Models;

namespace BizCrm.Backend.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Migrate database if needed
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed");
            }
            
            // Seed roles
            await SeedRolesAsync(roleManager, logger);
            
            // Seed default administrator
            await SeedDefaultAdminAsync(userManager, logger);
            
            // Seed sample data in development
            if (app.Environment.IsDevelopment())
            {
                await SeedSampleDataAsync(context, userManager, logger);
            }
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
    
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = { "systemadmin", "manager", "userstaff", "businessuser" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role '{Role}' created", role);
            }
        }
    }
    
    private static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@bizcrm.com";
        const string adminPassword = "Admin123!";
        
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                Department = "IT",
                JobTitle = "System Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "systemadmin");
                logger.LogInformation("Default administrator created with email: {Email}", adminEmail);
                logger.LogInformation("Default password: {Password}", adminPassword);
            }
            else
            {
                logger.LogError("Failed to create default administrator: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
    
    private static async Task SeedSampleDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
    {
        // Check if sample data already exists
        if (context.Companies.Any())
        {
            return;
        }
        
        try
        {
            // Create sample users
            var manager = await CreateSampleUserAsync(userManager, "manager@bizcrm.com", "Manager123!", 
                "John", "Smith", "Sales", "Sales Manager", "manager");
                
            var staff = await CreateSampleUserAsync(userManager, "staff@bizcrm.com", "Staff123!", 
                "Jane", "Johnson", "Sales", "Sales Representative", "userstaff");
                
            var businessUser = await CreateSampleUserAsync(userManager, "business@bizcrm.com", "Business123!", 
                "Mike", "Brown", "Operations", "Business Analyst", "businessuser");
            
            // Create sample companies
            var companies = new List<Company>
            {
                new()
                {
                    Name = "Acme Corporation",
                    Industry = "Technology",
                    CompanySize = "500-1000",
                    Website = "https://acme.com",
                    Email = "info@acme.com",
                    PhoneNumber = "+1-555-0100",
                    AddressLine1 = "123 Business Ave",
                    City = "New York",
                    StateProvince = "NY",
                    PostalCode = "10001",
                    Country = "United States",
                    AnnualRevenue = 50000000,
                    Status = CompanyStatus.Customer,
                    CreatedById = manager.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Name = "Global Solutions Inc",
                    Industry = "Consulting",
                    CompanySize = "100-500",
                    Website = "https://globalsolutions.com",
                    Email = "contact@globalsolutions.com",
                    PhoneNumber = "+1-555-0200",
                    AddressLine1 = "456 Enterprise Blvd",
                    City = "San Francisco",
                    StateProvince = "CA",
                    PostalCode = "94105",
                    Country = "United States",
                    AnnualRevenue = 25000000,
                    Status = CompanyStatus.Prospect,
                    CreatedById = staff.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };
            
            context.Companies.AddRange(companies);
            await context.SaveChangesAsync();
            
            // Create sample contacts
            var contacts = new List<Contact>
            {
                new()
                {
                    FirstName = "Alice",
                    LastName = "Cooper",
                    Email = "alice.cooper@acme.com",
                    PhoneNumber = "+1-555-0101",
                    JobTitle = "IT Director",
                    Department = "Information Technology",
                    Status = ContactStatus.Customer,
                    CompanyId = companies[0].Id,
                    CreatedById = manager.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new()
                {
                    FirstName = "Bob",
                    LastName = "Wilson",
                    Email = "bob.wilson@globalsolutions.com",
                    PhoneNumber = "+1-555-0201",
                    JobTitle = "CEO",
                    Department = "Executive",
                    Status = ContactStatus.Lead,
                    CompanyId = companies[1].Id,
                    CreatedById = staff.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };
            
            context.Contacts.AddRange(contacts);
            await context.SaveChangesAsync();
            
            // Create sample opportunities
            var opportunities = new List<Opportunity>
            {
                new()
                {
                    Title = "Enterprise Software License",
                    EstimatedValue = 250000,
                    ProbabilityPercentage = 75,
                    Stage = OpportunityStage.Negotiation,
                    Status = OpportunityStatus.Open,
                    ExpectedCloseDate = DateTime.UtcNow.AddDays(30),
                    Description = "Enterprise software licensing deal for Acme Corporation",
                    ContactId = contacts[0].Id,
                    CompanyId = companies[0].Id,
                    AssignedToId = manager.Id,
                    CreatedById = manager.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new()
                {
                    Title = "Consulting Services Agreement",
                    EstimatedValue = 150000,
                    ProbabilityPercentage = 50,
                    Stage = OpportunityStage.ValueProposition,
                    Status = OpportunityStatus.Open,
                    ExpectedCloseDate = DateTime.UtcNow.AddDays(45),
                    Description = "6-month consulting engagement for digital transformation",
                    ContactId = contacts[1].Id,
                    CompanyId = companies[1].Id,
                    AssignedToId = staff.Id,
                    CreatedById = staff.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };
            
            context.Opportunities.AddRange(opportunities);
            await context.SaveChangesAsync();
            
            // Create sample activities
            var activities = new List<Activity>
            {
                new()
                {
                    Subject = "Initial Discovery Call",
                    Description = "Discuss requirements and timeline with Alice",
                    Type = ActivityType.Call,
                    Status = ActivityStatus.Completed,
                    Priority = ActivityPriority.High,
                    ScheduledDateTime = DateTime.UtcNow.AddDays(-18),
                    CompletedDateTime = DateTime.UtcNow.AddDays(-18),
                    DurationMinutes = 60,
                    ContactId = contacts[0].Id,
                    OpportunityId = opportunities[0].Id,
                    AssignedToId = manager.Id,
                    CreatedById = manager.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-19)
                },
                new()
                {
                    Subject = "Follow-up Email",
                    Description = "Send proposal and pricing information",
                    Type = ActivityType.Email,
                    Status = ActivityStatus.Scheduled,
                    Priority = ActivityPriority.Normal,
                    ScheduledDateTime = DateTime.UtcNow.AddDays(1),
                    ContactId = contacts[1].Id,
                    OpportunityId = opportunities[1].Id,
                    AssignedToId = staff.Id,
                    CreatedById = staff.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };
            
            context.Activities.AddRange(activities);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Sample data created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sample data");
        }
    }
    
    private static async Task<ApplicationUser> CreateSampleUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string department,
        string jobTitle,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                Department = department,
                JobTitle = jobTitle,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
        
        return user;
    }
}