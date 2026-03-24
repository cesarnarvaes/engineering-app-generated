using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using BizCrm.Backend.Models;
using System.Security.Claims;

namespace BizCrm.Backend.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(UserManager<ApplicationUser> userManager, ILogger<NotificationHub> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var user = await _userManager.FindByIdAsync(userId!);
        
        if (user != null)
        {
            // Add user to role-based groups for targeted notifications
            var roles = await _userManager.GetRolesAsync(user);
            
            foreach (var role in roles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, role);
                _logger.LogDebug("User {UserId} added to role group {Role}", userId, role);
            }

            // Add to administrators group if user is system admin
            if (roles.Contains("systemadmin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Administrators");
            }

            // Add to managers group if user is manager or above
            if (roles.Contains("manager") || roles.Contains("systemadmin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");
            }

            _logger.LogInformation("User {UserId} ({FullName}) connected to NotificationHub", userId, user.FullName);
            
            // Notify other users a user is online (for presence indicators)
            await Clients.Others.SendAsync("UserOnline", new { userId, fullName = user.FullName });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var user = await _userManager.FindByIdAsync(userId!);
        
        if (user != null)
        {
            _logger.LogInformation("User {UserId} ({FullName}) disconnected from NotificationHub", userId, user.FullName);
            
            // Notify other users a user is offline
            await Clients.Others.SendAsync("UserOffline", new { userId, fullName = user.FullName });
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Client-callable methods

    /// <summary>
    /// Join a project or team-specific group
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var userId = Context.UserIdentifier;
        _logger.LogDebug("User {UserId} joined group {GroupName}", userId, groupName);
    }

    /// <summary>
    /// Leave a project or team-specific group
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        var userId = Context.UserIdentifier;
        _logger.LogDebug("User {UserId} left group {GroupName}", userId, groupName);
    }

    /// <summary>
    /// Send a direct message to another user
    /// </summary>
    public async Task SendDirectMessage(string targetUserId, string message)
    {
        var senderUserId = Context.UserIdentifier;
        var sender = await _userManager.FindByIdAsync(senderUserId!);
        
        if (sender == null)
            return;

        await Clients.User(targetUserId).SendAsync("DirectMessage", new 
        {
            senderId = senderUserId,
            senderName = sender.FullName,
            message,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("Direct message sent from {SenderId} to {TargetUserId}", senderUserId, targetUserId);
    }

    /// <summary>
    /// Broadcast a message to a specific group
    /// </summary>
    [Authorize(Roles = "manager,systemadmin")]
    public async Task SendGroupMessage(string groupName, string message)
    {
        var senderUserId = Context.UserIdentifier;
        var sender = await _userManager.FindByIdAsync(senderUserId!);
        
        if (sender == null)
            return;

        await Clients.Group(groupName).SendAsync("GroupMessage", new 
        {
            senderId = senderUserId,
            senderName = sender.FullName,
            groupName,
            message,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Group message sent to {GroupName} by {SenderId}", groupName, senderUserId);
    }

    /// <summary>
    /// Update user's activity status (typing, away, etc.)
    /// </summary>
    public async Task UpdateStatus(string status, string? context = null)
    {
        var userId = Context.UserIdentifier;
        var user = await _userManager.FindByIdAsync(userId!);
        
        if (user == null)
            return;

        await Clients.Others.SendAsync("UserStatusChanged", new 
        {
            userId,
            fullName = user.FullName,
            status,
            context,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("User {UserId} status updated to {Status}", userId, status);
    }

    /// <summary>
    /// Notify about real-time data changes (for live updates)
    /// </summary>
    public async Task NotifyDataChange(string entityType, string action, object data)
    {
        var userId = Context.UserIdentifier;
        var user = await _userManager.FindByIdAsync(userId!);
        
        if (user == null)
            return;

        // Broadcast to all connected users except the sender
        await Clients.Others.SendAsync("DataChanged", new 
        {
            entityType,
            action,
            data,
            changedBy = user.FullName,
            timestamp = DateTime.UtcNow
        });

        _logger.LogDebug("Data change notification sent: {EntityType} {Action} by {UserId}", entityType, action, userId);
    }

    /// <summary>
    /// Request current online users list
    /// </summary>
    public async Task RequestOnlineUsers()
    {
        // In a production app, you'd maintain a list of online users
        // For demo purposes, we'll just acknowledge the request
        await Clients.Caller.SendAsync("OnlineUsersUpdate", new 
        {
            requestedAt = DateTime.UtcNow,
            message = "Online users list would be here"
        });
    }
}

/// <summary>
/// Extension to help with client-side typing
/// </summary>
public static class NotificationHubExtensions
{
    public static class ClientMethods
    {
        public const string UserOnline = "UserOnline";
        public const string UserOffline = "UserOffline";
        public const string DirectMessage = "DirectMessage";
        public const string GroupMessage = "GroupMessage";
        public const string UserStatusChanged = "UserStatusChanged";
        public const string DataChanged = "DataChanged";
        public const string OnlineUsersUpdate = "OnlineUsersUpdate";
        
        // Background job notifications
        public const string EmailSent = "EmailSent";
        public const string DataCleanupCompleted = "DataCleanupCompleted";
        public const string ReportReady = "ReportReady";
        public const string ReportFailed = "ReportFailed";
        public const string ActivityReminder = "ActivityReminder";
        public const string BackupCompleted = "BackupCompleted";
        public const string BackupFailed = "BackupFailed";
        
        // Business entity notifications
        public const string ContactCreated = "ContactCreated";
        public const string ContactUpdated = "ContactUpdated";
        public const string CompanyCreated = "CompanyCreated";
        public const string CompanyUpdated = "CompanyUpdated";
        public const string OpportunityCreated = "OpportunityCreated";
        public const string OpportunityUpdated = "OpportunityUpdated";
        public const string ActivityCreated = "ActivityCreated";
        public const string ActivityUpdated = "ActivityUpdated";
    }
}