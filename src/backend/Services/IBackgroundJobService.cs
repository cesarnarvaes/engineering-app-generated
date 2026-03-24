namespace BizCrm.Backend.Services;

public interface IBackgroundJobService
{
    string ScheduleEmailNotification(string recipientEmail, string subject, string body, DateTime? scheduleAt = null);
    string ScheduleDataCleanup(DateTime olderThan);
    string ScheduleReportGeneration(string reportType, string userId, Dictionary<string, object>? parameters = null);
    string ScheduleActivityReminder(int activityId, DateTime reminderTime);
    string ScheduleRecurringDataBackup(string cronExpression);
    bool CancelJob(string jobId);
    void TriggerImmediateBackup();
}