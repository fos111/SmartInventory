using SmartInventory.Application.Auth.DTOs.Responses;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Notification.DTOs;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;
using SmartInventory.Domain.Notification.Enums;

namespace SmartInventory.Application.Auth.Services;

public class SupervisorService : ISupervisorService
{
    private readonly IAuthRepository _authRepository;
    private readonly IEmailSender _emailSender;
    private readonly INotificationService _notificationService;

    public SupervisorService(
        IAuthRepository authRepository,
        IEmailSender emailSender,
        INotificationService notificationService)
    {
        _authRepository = authRepository;
        _emailSender = emailSender;
        _notificationService = notificationService;
    }

    public async Task<List<UserListResponse>> GetPendingUsersAsync(CancellationToken ct = default)
    {
        var users = await _authRepository.GetUsersByStatusAsync(AccountStatus.Pending, ct);
        return users
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserListResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                IsEmailVerified = u.IsEmailVerified
            })
            .ToList();
    }

    public async Task<bool> ApproveUserAsync(Guid userId, UserRole role, Guid supervisorId, CancellationToken ct = default)
    {
        if (role != UserRole.Technicien && role != UserRole.Gestionnaire && role != UserRole.Supervisor)
            return false;

        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null || user.Status != AccountStatus.Pending)
            return false;

        user.Role = role;
        user.Status = AccountStatus.Active;
        user.ApprovedByUserId = supervisorId;
        user.ApprovedAt = DateTime.UtcNow;
        await _authRepository.UpdateAsync(user, ct);

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            EventType = NotificationEventType.AuthUserApproved,
            Type = NotificationType.Info,
            Title = "User Approved",
            Message = $"{user.Username} ({user.Email}) was approved as {role}",
            TargetRole = UserRole.Supervisor
        }, ct);

        var roleLabel = role == UserRole.Gestionnaire ? "Gestionnaire" : role == UserRole.Supervisor ? "Supervisor" : "Technician";
        var htmlBody = $@"
            <h2>Account Approved</h2>
            <p>Congratulations! Your {roleLabel} account has been approved. You can now log in with full access.</p>
        ";
        await _emailSender.SendEmailAsync(user.Email, $"{roleLabel} Account Approved", htmlBody, ct);

        return true;
    }

    public async Task<bool> RejectUserAsync(Guid userId, Guid supervisorId, string? reason, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null || user.Status != AccountStatus.Pending)
            return false;

        user.Status = AccountStatus.Rejected;
        user.RejectionReason = reason;
        await _authRepository.UpdateAsync(user, ct);

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            EventType = NotificationEventType.AuthUserRejected,
            Type = NotificationType.Warning,
            Title = "User Rejected",
            Message = $"{user.Username} ({user.Email}) was rejected{(string.IsNullOrEmpty(reason) ? "" : $": {reason}")}",
            TargetRole = UserRole.Supervisor
        }, ct);

        var htmlBody = $@"
            <h2>Account Rejected</h2>
            <p>Your account application has been rejected.</p>
            {(string.IsNullOrEmpty(reason) ? "" : $"<p>Reason: {reason}</p>")}
            <p>You may request re-evaluation after 30 days.</p>
        ";
        await _emailSender.SendEmailAsync(user.Email, "Account Rejected", htmlBody, ct);

        return true;
    }
}
