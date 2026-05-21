using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Models;
using SmartInventory.Application.Mobile.Auth.DTOs;
using SmartInventory.Application.Mobile.Auth.Interfaces;
using SmartInventory.Application.PasswordReset.DTOs;
using SmartInventory.Application.PasswordReset.Interfaces;

namespace SmartInventory.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile/auth")]
public class MobileAuthController : ControllerBase
{
    private readonly IMobileAuthService _mobileAuthService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPasswordResetService _passwordResetService;

    public MobileAuthController(
        IMobileAuthService mobileAuthService,
        IRefreshTokenService refreshTokenService,
        IPasswordResetService passwordResetService)
    {
        _mobileAuthService = mobileAuthService;
        _refreshTokenService = refreshTokenService;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileEnvelope<RegisterResultDto>>> Register(
        [FromBody] MobileRegisterRequest request,
        CancellationToken ct)
    {
        var result = await _mobileAuthService.RegisterAsync(request, ct);
        if (result == null)
            return Conflict(MobileEnvelope<RegisterResultDto>.FailureResult("Username or email already exists"));

        return Ok(MobileEnvelope<RegisterResultDto>.SuccessResult(
            result, "Registration successful, please verify your email"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileEnvelope<TokenPairDto>>> Login(
        [FromBody] MobileLoginRequest request,
        CancellationToken ct)
    {
        var (user, needsVerification) = await _mobileAuthService.LoginAsync(request, ct);
        if (user == null)
            return Ok(MobileEnvelope<TokenPairDto>.FailureResult("Invalid credentials"));

        if (needsVerification)
            return Ok(new MobileEnvelope<TokenPairDto>
            {
                Success = false,
                Message = "Email not verified",
                NeedsVerification = true,
                Email = request.Email
            });

        var tokenPair = await _refreshTokenService.CreateTokenPairAsync(user, ct);
        return Ok(MobileEnvelope<TokenPairDto>.SuccessResult(tokenPair));
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileEnvelope<TokenPairDto>>> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken ct)
    {
        var result = await _mobileAuthService.VerifyEmailAsync(request, ct);
        if (result == null)
            return BadRequest(MobileEnvelope<TokenPairDto>.FailureResult("Invalid or expired code"));

        return Ok(MobileEnvelope<TokenPairDto>.SuccessResult(result));
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileEnvelope>> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken ct)
    {
        var success = await _mobileAuthService.ResendVerificationAsync(request, ct);
        if (!success)
            return BadRequest(MobileEnvelope.FailureResult("Email not found or already verified"));

        return Ok(MobileEnvelope.SuccessResult("Verification code sent"));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileEnvelope<TokenPairDto>>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await _refreshTokenService.RefreshAsync(request.RefreshToken, ct);
        if (result == null)
            return Unauthorized(MobileEnvelope<TokenPairDto>.FailureResult("Invalid or expired refresh token"));

        return Ok(MobileEnvelope<TokenPairDto>.SuccessResult(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<MobileEnvelope>> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken ct)
    {
        await _refreshTokenService.RevokeAsync(request.RefreshToken, ct);
        return Ok(MobileEnvelope.SuccessResult("Logged out"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MobileEnvelope<MobileUserDto>>> GetProfile(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(MobileEnvelope<MobileUserDto>.FailureResult("Invalid token"));

        var profile = await _mobileAuthService.GetProfileAsync(userId.Value, ct);
        if (profile == null)
            return NotFound(MobileEnvelope<MobileUserDto>.FailureResult("User not found"));

        return Ok(MobileEnvelope<MobileUserDto>.SuccessResult(profile));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<MobileEnvelope>> UpdateProfile(
        IFormFile? avatar,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(MobileEnvelope.FailureResult("Invalid token"));

        if (avatar == null || avatar.Length == 0)
            return BadRequest(MobileEnvelope.FailureResult("No file provided"));

        if (!IsValidImage(avatar))
            return BadRequest(MobileEnvelope.FailureResult("Only JPEG and PNG files are allowed (max 5MB)"));

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
        var fileName = $"{userId:N}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await avatar.CopyToAsync(stream, ct);

        var avatarUrl = $"/uploads/avatars/{fileName}";
        await _mobileAuthService.UpdateAvatarAsync(userId.Value, avatarUrl, ct);

        return Ok(MobileEnvelope.SuccessResult("Profile updated"));
    }

    // Password reset endpoints
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileEnvelope>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        var success = await _passwordResetService.RequestResetAsync(request.Email, ct);
        if (!success)
            return BadRequest(MobileEnvelope.FailureResult("Email not found"));

        return Ok(MobileEnvelope.SuccessResult("Reset code sent to your email"));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileEnvelope>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var success = await _passwordResetService.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword, ct);
        if (!success)
            return BadRequest(MobileEnvelope.FailureResult("Invalid or expired code"));

        return Ok(MobileEnvelope.SuccessResult("Password updated"));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            return null;
        return userId;
    }

    private static bool IsValidImage(IFormFile file)
    {
        if (file.Length > 5 * 1024 * 1024)
            return false;

        var contentType = file.ContentType.ToLowerInvariant();
        return contentType is "image/jpeg" or "image/png";
    }
}
