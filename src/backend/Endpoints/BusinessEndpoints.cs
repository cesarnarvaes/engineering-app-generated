using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BizCrm.Backend.Data;
using BizCrm.Backend.DTOs;
using BizCrm.Backend.Models;
using BizCrm.Backend.Services;
using BizCrm.Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BizCrm.Backend.Endpoints;

public static class BusinessEndpoints
{
    public static void MapBusinessEndpoints(this IEndpointRouteBuilder app)
    {
        MapContactEndpoints(app);
        MapCompanyEndpoints(app);
        MapOpportunityEndpoints(app);
        MapActivityEndpoints(app);
        MapReportEndpoints(app);
    }

    private static void MapContactEndpoints(IEndpointRouteBuilder app)
    {
        var contacts = app.MapGroup("/api/contacts")
            .RequireAuthorization("BusinessUser")
            .WithTags("Contacts")
            .WithOpenApi();

        // Get contacts with pagination
        contacts.MapGet("/", async (
            ApplicationDbContext context,
            int page = 1,
            int pageSize = 10,
            string? search = null,
            ContactStatus? status = null) =>
        {
            var query = context.Contacts
                .Include(c => c.Company)
                .Include(c => c.CreatedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(c => 
                    c.FirstName.ToLower().Contains(lowerSearch) ||
                    c.LastName.ToLower().Contains(lowerSearch) ||
                    (c.Email != null && c.Email.ToLower().Contains(lowerSearch)) ||
                    (c.Company != null && c.Company.Name.ToLower().Contains(lowerSearch)));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var contacts = await query
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContactDto(
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.FullName,
                    c.Email,
                    c.PhoneNumber,
                    c.JobTitle,
                    c.Department,
                    c.Notes,
                    c.Status,
                    c.CompanyId,
                    c.Company != null ? c.Company.Name : null,
                    c.CreatedAt,
                    c.CreatedBy.FullName
                ))
                .ToListAsync();

            return Results.Ok(new PagedResponse<ContactDto>(
                contacts,
                page,
                pageSize,
                totalCount,
                totalPages
            ));
        });

        // Get contact by ID
        contacts.MapGet("/{id:int}", async (int id, ApplicationDbContext context) =>
        {
            var contact = await context.Contacts
                .Include(c => c.Company)
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null)
            {
                return Results.NotFound(new ApiResponse<object?>(null, false, "Contact not found"));
            }

            var contactDto = new ContactDto(
                contact.Id,
                contact.FirstName,
                contact.LastName,
                contact.FullName,
                contact.Email,
                contact.PhoneNumber,
                contact.JobTitle,
                contact.Department,
                contact.Notes,
                contact.Status,
                contact.CompanyId,
                contact.Company?.Name,
                contact.CreatedAt,
                contact.CreatedBy.FullName
            );

            return Results.Ok(new ApiResponse<ContactDto>(contactDto, true));
        });

        // Create contact
        contacts.MapPost("/", async (
            CreateContactRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            IHubContext<NotificationHub> hubContext) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var contact = new Contact
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                JobTitle = request.JobTitle,
                Department = request.Department,
                Notes = request.Notes,
                Status = request.Status,
                CompanyId = request.CompanyId,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            // Load the full contact with related data
            var createdContact = await context.Contacts
                .Include(c => c.Company)
                .Include(c => c.CreatedBy)
                .FirstAsync(c => c.Id == contact.Id);

            var contactDto = new ContactDto(
                createdContact.Id,
                createdContact.FirstName,
                createdContact.LastName,
                createdContact.FullName,
                createdContact.Email,
                createdContact.PhoneNumber,
                createdContact.JobTitle,
                createdContact.Department,
                createdContact.Notes,
                createdContact.Status,
                createdContact.CompanyId,
                createdContact.Company?.Name,
                createdContact.CreatedAt,
                createdContact.CreatedBy.FullName
            );

            // Send real-time notification
            await hubContext.Clients.All.SendAsync(NotificationHubExtensions.ClientMethods.ContactCreated, contactDto);

            return Results.Created($"/api/contacts/{contact.Id}", new ApiResponse<ContactDto>(contactDto, true, "Contact created successfully"));
        });

        // Update contact
        contacts.MapPut("/{id:int}", async (
            int id,
            UpdateContactRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            IHubContext<NotificationHub> hubContext) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var contact = await context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return Results.NotFound(new ApiResponse<object?>(null, false, "Contact not found"));
            }

            contact.FirstName = request.FirstName;
            contact.LastName = request.LastName;
            contact.Email = request.Email;
            contact.PhoneNumber = request.PhoneNumber;
            contact.JobTitle = request.JobTitle;
            contact.Department = request.Department;
            contact.Notes = request.Notes;
            contact.Status = request.Status;
            contact.CompanyId = request.CompanyId;
            contact.LastModifiedById = userId;
            contact.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Load the updated contact with related data
            var updatedContact = await context.Contacts
                .Include(c => c.Company)
                .Include(c => c.CreatedBy)
                .FirstAsync(c => c.Id == contact.Id);

            var contactDto = new ContactDto(
                updatedContact.Id,
                updatedContact.FirstName,
                updatedContact.LastName,
                updatedContact.FullName,
                updatedContact.Email,
                updatedContact.PhoneNumber,
                updatedContact.JobTitle,
                updatedContact.Department,
                updatedContact.Notes,
                updatedContact.Status,
                updatedContact.CompanyId,
                updatedContact.Company?.Name,
                updatedContact.CreatedAt,
                updatedContact.CreatedBy.FullName
            );

            // Send real-time notification
            await hubContext.Clients.All.SendAsync(NotificationHubExtensions.ClientMethods.ContactUpdated, contactDto);

            return Results.Ok(new ApiResponse<ContactDto>(contactDto, true, "Contact updated successfully"));
        });

        // Delete contact
        contacts.MapDelete("/{id:int}", [Authorize(Roles = "manager,systemadmin")] async (
            int id,
            ApplicationDbContext context) =>
        {
            var contact = await context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return Results.NotFound(new ApiResponse<object?>(null, false, "Contact not found"));
            }

            context.Contacts.Remove(contact);
            await context.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object?>(null, true, "Contact deleted successfully"));
        });
    }

    private static void MapCompanyEndpoints(IEndpointRouteBuilder app)
    {
        var companies = app.MapGroup("/api/companies")
            .RequireAuthorization("BusinessUser")
            .WithTags("Companies")
            .WithOpenApi();

        // Get companies with pagination
        companies.MapGet("/", async (
            ApplicationDbContext context,
            int page = 1,
            int pageSize = 10,
            string? search = null,
            CompanyStatus? status = null) =>
        {
            var query = context.Companies
                .Include(c => c.CreatedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(lowerSearch) ||
                    (c.Industry != null && c.Industry.ToLower().Contains(lowerSearch)) ||
                    (c.Website != null && c.Website.ToLower().Contains(lowerSearch)));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var companies = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CompanyDto(
                    c.Id,
                    c.Name,
                    c.Industry,
                    c.CompanySize,
                    c.Website,
                    c.PhoneNumber,
                    c.Email,
                    c.AddressLine1,
                    c.AddressLine2,
                    c.City,
                    c.StateProvince,
                    c.PostalCode,
                    c.Country,
                    c.AnnualRevenue,
                    c.Notes,
                    c.Status,
                    c.CreatedAt,
                    c.CreatedBy.FullName,
                    c.Contacts.Count,
                    c.Opportunities.Count
                ))
                .ToListAsync();

            return Results.Ok(new PagedResponse<CompanyDto>(
                companies,
                page,
                pageSize,
                totalCount,
                totalPages
            ));
        });

        // Additional CRUD operations for companies would follow similar pattern...
        // I'll include create as an example:

        companies.MapPost("/", async (
            CreateCompanyRequest request,
            ApplicationDbContext context,
            ClaimsPrincipal user,
            IHubContext<NotificationHub> hubContext) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var company = new Company
            {
                Name = request.Name,
                Industry = request.Industry,
                CompanySize = request.CompanySize,
                Website = request.Website,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                StateProvince = request.StateProvince,
                PostalCode = request.PostalCode,
                Country = request.Country,
                AnnualRevenue = request.AnnualRevenue,
                Notes = request.Notes,
                Status = request.Status,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.Companies.Add(company);
            await context.SaveChangesAsync();

            // Load the created company with related data
            var createdCompany = await context.Companies
                .Include(c => c.CreatedBy)
                .FirstAsync(c => c.Id == company.Id);

            var companyDto = new CompanyDto(
                createdCompany.Id,
                createdCompany.Name,
                createdCompany.Industry,
                createdCompany.CompanySize,
                createdCompany.Website,
                createdCompany.PhoneNumber,
                createdCompany.Email,
                createdCompany.AddressLine1,
                createdCompany.AddressLine2,
                createdCompany.City,
                createdCompany.StateProvince,
                createdCompany.PostalCode,
                createdCompany.Country,
                createdCompany.AnnualRevenue,
                createdCompany.Notes,
                createdCompany.Status,
                createdCompany.CreatedAt,
                createdCompany.CreatedBy.FullName,
                0, // No contacts yet
                0  // No opportunities yet
            );

            // Send real-time notification
            await hubContext.Clients.All.SendAsync(NotificationHubExtensions.ClientMethods.CompanyCreated, companyDto);

            return Results.Created($"/api/companies/{company.Id}", new ApiResponse<CompanyDto>(companyDto, true, "Company created successfully"));
        });
    }

    private static void MapOpportunityEndpoints(IEndpointRouteBuilder app)
    {
        var opportunities = app.MapGroup("/api/opportunities")
            .RequireAuthorization("BusinessUser")
            .WithTags("Opportunities")
            .WithOpenApi();

        // Get opportunities with pagination
        opportunities.MapGet("/", async (
            ApplicationDbContext context,
            ClaimsPrincipal user,
            int page = 1,
            int pageSize = 10,
            string? search = null,
            OpportunityStage? stage = null,
            OpportunityStatus? status = null) =>
        {
            var query = context.Opportunities
                .Include(o => o.Contact)
                .Include(o => o.Company)
                .Include(o => o.AssignedTo)
                .Include(o => o.CreatedBy)
                .AsQueryable();

            // Filter based on user role
            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!userRoles.Intersect(new[] { "manager", "systemadmin" }).Any())
            {
                // Non-managers can only see their own opportunities
                query = query.Where(o => o.AssignedToId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(o =>
                    o.Title.ToLower().Contains(lowerSearch) ||
                    (o.Contact != null && (o.Contact.FirstName + " " + o.Contact.LastName).ToLower().Contains(lowerSearch)) ||
                    (o.Company != null && o.Company.Name.ToLower().Contains(lowerSearch)));
            }

            if (stage.HasValue)
            {
                query = query.Where(o => o.Stage == stage.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var opportunities = await query
                .OrderByDescending(o => o.EstimatedValue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize) 
                .Select(o => new OpportunityDto(
                    o.Id,
                    o.Title,
                    o.EstimatedValue,
                    o.ProbabilityPercentage,
                    o.Stage,
                    o.Status,
                    o.ExpectedCloseDate,
                    o.ActualCloseDate,
                    o.Description,
                    o.Notes,
                    o.ContactId,
                    o.Contact != null ? o.Contact.FullName : null,
                    o.CompanyId,
                    o.Company != null ? o.Company.Name : null,
                    o.AssignedToId,
                    o.AssignedTo.FullName,
                    o.CreatedAt,
                    o.CreatedBy.FullName
                ))
                .ToListAsync();

            return Results.Ok(new PagedResponse<OpportunityDto>(
                opportunities,
                page,
                pageSize,
                totalCount,
                totalPages
            ));
        });
    }

    private static void MapActivityEndpoints(IEndpointRouteBuilder app)
    {
        var activities = app.MapGroup("/api/activities")
            .RequireAuthorization("BusinessUser")
            .WithTags("Activities")
            .WithOpenApi();

        // Get activities with pagination
        activities.MapGet("/", async (
            ApplicationDbContext context,
            ClaimsPrincipal user,
            int page = 1,
            int pageSize = 10,
            ActivityStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null) =>
        {
            var query = context.Activities
                .Include(a => a.Contact)
                .Include(a => a.Company)
                .Include(a => a.Opportunity)
                .Include(a => a.AssignedTo)
                .Include(a => a.CreatedBy)
                .AsQueryable();

            // Filter based on user role
            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!userRoles.Intersect(new[] { "manager", "systemadmin" }).Any())
            {
                // Non-managers can only see their own activities
                query = query.Where(a => a.AssignedToId == currentUserId);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.ScheduledDateTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.ScheduledDateTime <= toDate.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var activities = await query
                .OrderBy(a => a.ScheduledDateTime ?? DateTime.MaxValue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ActivityDto(
                    a.Id,
                    a.Subject,
                    a.Description,
                    a.Type,
                    a.Status,
                    a.Priority,
                    a.ScheduledDateTime,
                    a.CompletedDateTime,
                    a.DurationMinutes,
                    a.Location,
                    a.Notes,
                    a.ContactId,
                    a.Contact != null ? a.Contact.FullName : null,
                    a.CompanyId,
                    a.Company != null ? a.Company.Name : null,
                    a.OpportunityId,
                    a.Opportunity != null ? a.Opportunity.Title : null,
                    a.AssignedToId,
                    a.AssignedTo.FullName,
                    a.CreatedAt,
                    a.CreatedBy.FullName
                ))
                .ToListAsync();

            return Results.Ok(new PagedResponse<ActivityDto>(
                activities,
                page,
                pageSize,
                totalCount,
                totalPages
            ));
        });
    }

    private static void MapReportEndpoints(IEndpointRouteBuilder app)
    {
        var reports = app.MapGroup("/api/reports")
            .RequireAuthorization("Staff")
            .WithTags("Reports")
            .WithOpenApi();

        // Generate report (background job)
        reports.MapPost("/generate", [Authorize(Roles = "manager,systemadmin")] async (
            GenerateReportRequest request,
            IBackgroundJobService backgroundJobService,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var jobId = backgroundJobService.ScheduleReportGeneration(
                request.ReportType,
                userId,
                request.Parameters
            );

            return Results.Accepted("", new ApiResponse<string>(jobId, true, "Report generation started. You will be notified when complete."));
        });
    }
}

public record GenerateReportRequest(
    string ReportType,
    Dictionary<string, object>? Parameters = null
);