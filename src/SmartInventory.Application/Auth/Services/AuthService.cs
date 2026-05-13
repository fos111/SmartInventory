using SmartInventory.Application.Auth.DTOs.Requests;
using SmartInventory.Application.Auth.DTOs.Responses;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailVerificationService emailVerificationService,
        IEmailSender emailSender)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailVerificationService = emailVerificationService;
        _emailSender = emailSender;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByEmailAsync(request.Email, ct);
        if (user == null)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        if (!user.IsEmailVerified)
            return new AuthResponse
            {
                Message = "Email not verified",
                Status = user.Status,
                Username = user.Username
            };

        if (user.Status != AccountStatus.Active)
        {
            return new AuthResponse
            {
                Message = "Account not active",
                Status = user.Status,
                Username = user.Username,
                UserId = user.Id,
                IsActive = false
            };
        }

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            Status = user.Status,
            IsActive = true,
            Message = "Login successful"
        };
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _authRepository.ExistsAsync(request.Username, ct))
            return null;

        if (await _authRepository.ExistsByEmailAsync(request.Email, ct))
            return null;

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            // Role is null until assigned by supervisor during approval
            Status = AccountStatus.Pending,
            IsEmailVerified = false
        };

        await _authRepository.AddAsync(user, ct);
        await _emailVerificationService.SendVerificationEmailAsync(user, ct);

        await NotifySupervisorsOfNewUser(user, ct);

        return new AuthResponse
        {
            Message = "Registration successful. Please verify your email.",
            Username = user.Username,
            UserId = user.Id,
            Status = user.Status
        };
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        var (success, error) = await _emailVerificationService.ValidateTokenAsync(token, ct);
        if (!success)
            return false;

        var user = await _authRepository.GetUserByVerificationTokenAsync(token, ct);
        if (user == null)
            return false;

        user.IsEmailVerified = true;
        await _authRepository.UpdateAsync(user, ct);
        await _emailVerificationService.MarkTokenAsUsedAsync(token, ct);

        await NotifySupervisorsOfVerifiedUser(user, ct);

        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByEmailAsync(email, ct);
        if (user == null || user.IsEmailVerified)
            return false;

        await _emailVerificationService.SendVerificationEmailAsync(user, ct);
        return true;
    }

    public async Task<AuthResponse?> GetLimitedLoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByEmailAsync(request.Email, ct);
        if (user == null)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var token = _tokenService.GenerateLimitedToken(user);
        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            Status = user.Status,
            IsActive = user.Status == AccountStatus.Active,
            Message = user.Status == AccountStatus.Rejected
                ? "Account rejected. Request re-evaluation to apply again."
                : "Limited access - pending approval"
        };
    }

    public async Task<AuthResponse?> RequestReEvaluationAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user == null || user.Status != AccountStatus.Rejected)
            return null;

        user.Status = AccountStatus.Pending;
        user.RejectionReason = null;
        await _authRepository.UpdateAsync(user, ct);

        await NotifySupervisorsOfReEvaluation(user, ct);

        return new AuthResponse
        {
            Message = "Re-evaluation requested. You will be notified once reviewed.",
            UserId = user.Id,
            Username = user.Username,
            Status = user.Status
        };
    }

    private async Task NotifySupervisorsOfNewUser(User user, CancellationToken ct = default)
    {
        var htmlBody = $@"
            <h2>New User Registration</h2>
            <p>A new user has registered and is awaiting approval:</p>
            <ul>
                <li>Username: {user.Username}</li>
                <li>Email: {user.Email}</li>
                <li>Registered: {user.CreatedAt:u}</li>
            </ul>
        ";
        await _emailSender.SendToRoleAsync(UserRole.Supervisor, "New User Registration", htmlBody, ct);
    }

    private async Task NotifySupervisorsOfVerifiedUser(User user, CancellationToken ct = default)
    {
        var htmlBody = $@"
            <h2>User Email Verified</h2>
            <p>A user has verified their email and is awaiting approval:</p>
            <ul>
                <li>Username: {user.Username}</li>
                <li>Email: {user.Email}</li>
            </ul>
        ";
        await _emailSender.SendToRoleAsync(UserRole.Supervisor, "User Email Verified", htmlBody, ct);
    }

    private async Task NotifySupervisorsOfReEvaluation(User user, CancellationToken ct = default)
    {
        var htmlBody = $@"
            <h2>Re-Evaluation Request</h2>
            <p>A previously rejected user has requested re-evaluation:</p>
            <ul>
                <li>Username: {user.Username}</li>
                <li>Email: {user.Email}</li>
            </ul>
        ";
        await _emailSender.SendToRoleAsync(UserRole.Supervisor, "Re-Evaluation Request", htmlBody, ct);
    }
}