using Hangfire;
using BizCrm.Backend.Data;
using BizCrm.Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BizCrm.Backend.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ApplicationDbContext context,
        IHubContext<NotificationHub> hubContext,
        ILogger<BackgroundJobService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public string ScheduleEmailNotification(string recipientEmail, string subject, string body, DateTime? scheduleAt = null)
    {
        if (scheduleAt.HasValue)
        {
            return _backgroundJobClient.Schedule(() => SendEmailNotification(recipientEmail, subject, body), scheduleAt.Value);
        }
        else
        {
            return _backgroundJobClient.Enqueue(() => SendEmailNotification(recipientEmail, subject, body));
        }
    }

    public string ScheduleDataCleanup(DateTime olderThan)
    {
        return _backgroundJobClient.Enqueue(() => CleanupOldData(olderThan));
    }

    public string ScheduleReportGeneration(string reportType, string userId, Dictionary<string, object>? parameters = null)
    {
        return _backgroundJobClient.Enqueue(() => GenerateReport(reportType, userId, parameters ?? new Dictionary<string, object>()));
    }

    public string ScheduleActivityReminder(int activityId, DateTime reminderTime)
    {
        return _backgroundJobClient.Schedule(() => SendActivityReminder(activityId), reminderTime);
    }

    public string ScheduleRecurringDataBackup(string cronExpression)
    {
        _recurringJobManager.AddOrUpdate(
            "data-backup",
            () => PerformDataBackup(),
            cronExpression,
            TimeZoneInfo.Utc);
        
        return "data-backup";
    }

    public bool CancelJob(string jobId)
    {
        try
        {
            return _backgroundJobClient.Delete(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling job {JobId}", jobId);
            return false;
        }
    }

    public void TriggerImmediateBackup()
    {
        _backgroundJobClient.Enqueue(() => PerformDataBackup());
    }

    // Background job methods (these will be executed by Hangfire)
    
    [AutomaticRetry(Attempts = 3)]
    public async Task SendEmailNotification(string recipientEmail, string subject, string body)
    {
        try
        {
            // In production, integrate with your email service (SendGrid, SES, etc.)
            _logger.LogInformation("Sending email to {Email} with subject: {Subject}", recipientEmail, subject);
            
            // Simulate email sending
            await Task.Delay(1000);
            
            // Send real-time notification of successful email send
            await _hubContext.Clients.All.SendAsync("EmailSent", new 
            { 
                Recipient = recipientEmail, 
                Subject = subject, 
                SentAt = DateTime.UtcNow 
            });
            
            _logger.LogInformation("Email sent successfully to {Email}", recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", recipientEmail);
            throw; // This will trigger Hangfire retry mechanism
        }
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task CleanupOldData(DateTime olderThan)
    {
        try
        {
            _logger.LogInformation("Starting data cleanup for records older than {Date}", olderThan);
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            // Clean up old activities
            var oldActivities = await _context.Activities
                .Where(a => a.CreatedAt < olderThan && a.Status == Models.ActivityStatus.Completed)
                .ToListAsync();
                
            if (oldActivities.Any())
            {
                _context.Activities.RemoveRange(oldActivities);
                _logger.LogInformation("Removed {Count} old activities", oldActivities.Count);
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Notify users of completed cleanup
            await _hubContext.Clients.All.SendAsync("DataCleanupCompleted", new
            {
                ActivitiesRemoved = oldActivities.Count,
                CompletedAt = DateTime.UtcNow
            });
            
            _logger.LogInformation("Data cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data cleanup");
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task GenerateReport(string reportType, string userId, Dictionary<string, object> parameters)
    {
        try
        {
            _logger.LogInformation("Generating {ReportType} report for user {UserId}", reportType, userId);
            
            // Simulate report generation
            await Task.Delay(5000);
            
            var reportData = reportType.ToLower() switch
            {
                "contacts" => await GenerateContactsReport(parameters),
                "opportunities" => await GenerateOpportunitiesReport(parameters),
                "activities" => await GenerateActivitiesReport(parameters),
                _ => throw new ArgumentException($"Unknown report type: {reportType}")
            };
            
            // In production, save report to blob storage and send download link
            var reportUrl = $"/api/reports/{Guid.NewGuid()}.pdf";
            
            // Send real-time notification to the user
            await _hubContext.Clients.User(userId).SendAsync("ReportReady", new
            {
                ReportType = reportType,
                DownloadUrl = reportUrl,
                GeneratedAt = DateTime.UtcNow,
                RecordCount = reportData.Count
            });
            
            _logger.LogInformation("{ReportType} report generated successfully for user {UserId}", reportType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {ReportType} report for user {UserId}", reportType, userId);
            
            // Notify user of failure
            await _hubContext.Clients.User(userId).SendAsync("ReportFailed", new
            {
                ReportType = reportType,
                Error = "Report generation failed. Please try again.",
                FailedAt = DateTime.UtcNow
            });
            
            throw;
        }
    }

    private async Task<List<object>> GenerateContactsReport(Dictionary<string, object> parameters)
    {
        var contacts = await _context.Contacts
            .Include(c => c.Company)
            .Include(c => c.CreatedBy)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.Email,
                c.PhoneNumber,
                c.JobTitle,
                CompanyName = c.Company != null ? c.Company.Name : null,
                Status = c.Status.ToString(),
                c.CreatedAt
            })
            .ToListAsync();
            
        return contacts.Cast<object>().ToList();
    }

    private async Task<List<object>> GenerateOpportunitiesReport(Dictionary<string, object> parameters)
    {
        var opportunities = await _context.Opportunities
            .Include(o => o.Contact)
            .Include(o => o.Company)
            .Include(o => o.AssignedTo)
            .Select(o => new
            {
                o.Id,
                o.Title,
                o.EstimatedValue,
                o.ProbabilityPercentage,
                Stage = o.Stage.ToString(),
                Status = o.Status.ToString(),
                ContactName = o.Contact != null ? o.Contact.FullName : null,
                CompanyName = o.Company != null ? o.Company.Name : null,
                AssignedTo = o.AssignedTo.FullName,
                o.ExpectedCloseDate,
                o.CreatedAt
            })
            .ToListAsync();
            
        return opportunities.Cast<object>().ToList();
    }

    private async Task<List<object>> GenerateActivitiesReport(Dictionary<string, object> parameters)
    {
        var activities = await _context.Activities
            .Include(a => a.Contact)
            .Include(a => a.AssignedTo)
            .Select(a => new
            {
                a.Id,
                a.Subject,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                Priority = a.Priority.ToString(),
                a.ScheduledDateTime,
                a.CompletedDateTime,
                ContactName = a.Contact != null ? a.Contact.FullName : null,
                AssignedTo = a.AssignedTo.FullName,
                a.CreatedAt
            })
            .ToListAsync();
            
        return activities.Cast<object>().ToList();
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendActivityReminder(int activityId)
    {
        try
        {
            var activity = await _context.Activities
                .Include(a => a.AssignedTo)
                .Include(a => a.Contact)
                .FirstOrDefaultAsync(a => a.Id == activityId);
                
            if (activity == null)
            {
                _logger.LogWarning("Activity {ActivityId} not found for reminder", activityId);
                return;
            }
            
            if (activity.Status != Models.ActivityStatus.Scheduled)
            {
                _logger.LogInformation("Skipping reminder for activity {ActivityId} as it's no longer scheduled", activityId);
                return;
            }
            
            // Send real-time notification
            await _hubContext.Clients.User(activity.AssignedToId).SendAsync("ActivityReminder", new
            {
                ActivityId = activity.Id,
                Subject = activity.Subject,
                ScheduledDateTime = activity.ScheduledDateTime,
                ContactName = activity.Contact?.FullName,
                Type = activity.Type.ToString(),
                Priority = activity.Priority.ToString()
            });
            
            // Also schedule an email notification
            ScheduleEmailNotification(
                activity.AssignedTo.Email!,
                $"Activity Reminder: {activity.Subject}",
                $"You have an upcoming activity '{activity.Subject}' scheduled for {activity.ScheduledDateTime:yyyy-MM-dd HH:mm}."
            );
            
            _logger.LogInformation("Activity reminder sent for activity {ActivityId} to user {UserId}", activityId, activity.AssignedToId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending activity reminder for activity {ActivityId}", activityId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task PerformDataBackup()
    {
        try
        {
            _logger.LogInformation("Starting database backup");
            
            // In production, implement actual backup logic
            // This could involve:
            // - Creating SQL Server backup
            // - Exporting data to Azure Storage
            // - Creating point-in-time snapshots
            
            await Task.Delay(10000); // Simulate backup time
            
            // Notify administrators of successful backup
            await _hubContext.Clients.Group("Administrators").SendAsync("BackupCompleted", new
            {
                BackupType = "Scheduled",
                CompletedAt = DateTime.UtcNow,
                Success = true
            });
            
            _logger.LogInformation("Database backup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database backup");
            
            // Notify administrators of backup failure
            await _hubContext.Clients.Group("Administrators").SendAsync("BackupFailed", new
            {
                BackupType = "Scheduled",
                FailedAt = DateTime.UtcNow,
                Error = ex.Message
            });
            
            throw;
        }
    }
}