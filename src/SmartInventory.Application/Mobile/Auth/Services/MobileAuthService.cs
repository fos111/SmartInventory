using AutoMapper;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Helpers;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Domain.Auth.Entities;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Application.Mobile.Auth.Services;

public class MobileAuthService : IMobileAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IMapper _mapper;

    public MobileAuthService(
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        IEmailVerificationService emailVerificationService,
        IRefreshTokenService refreshTokenService,
        IMapper mapper)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _emailVerificationService = emailVerificationService;
        _refreshTokenService = refreshTokenService;
        _mapper = mapper;
    }

    public async Task<RegisterResultDto?> RegisterAsync(MobileRegisterRequest request, CancellationToken ct = default)
    {
        var emailExists = await _authRepository.ExistsByEmailAsync(request.Email, ct);
        if (emailExists)
            return null;

        var usernameExists = await _authRepository.ExistsAsync(request.Name, ct);
        if (usernameExists)
            return null;

        var role = request.Role is not null
            ? MobileRoleMapper.MapToDotNet(request.Role)
            : UserRole.Technicien;

        var user = new User
        {
            Username = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            Status = AccountStatus.Active,
            IsEmailVerified = false
        };

        await _authRepository.AddAsync(user, ct);

        await _emailVerificationService.SendVerificationEmailAsync(user, ct);

        return new RegisterResultDto(user.Id, true, user.Email);
    }

    public async Task<(User? User, bool NeedsVerification)> LoginAsync(MobileLoginRequest request, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByEmailAsync(request.Email, ct);
        if (user == null)
            return (null, false);

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return (null, false);

        if (!user.IsEmailVerified)
            return (user, true);

        return (user, false);
    }

    public async Task<TokenPairDto?> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var (success, _) = await _emailVerificationService.ValidateTokenAsync(request.Otp, ct);
        if (!success)
            return null;

        var user = await _authRepository.GetByEmailAsync(request.Email, ct);
        if (user == null)
            return null;

        user.IsEmailVerified = true;
        await _authRepository.UpdateAsync(user, ct);
        await _emailVerificationService.MarkTokenAsUsedAsync(request.Otp, ct);

        return await _refreshTokenService.CreateTokenPairAsync(user, ct);
    }

    public async Task<bool> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByEmailAsync(request.Email, ct);
        if (user == null || user.IsEmailVerified)
            return false;

        await _emailVerificationService.SendVerificationEmailAsync(user, ct);
        return true;
    }

    public async Task<MobileUserDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByIdAsync(userId, ct);
        return user is null ? null : _mapper.Map<MobileUserDto>(user);
    }

    public async Task<string?> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct = default)
    {
        var user = await _authRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return null;

        user.AvatarUrl = avatarUrl;
        await _authRepository.UpdateAsync(user, ct);
        return avatarUrl;
    }
}
