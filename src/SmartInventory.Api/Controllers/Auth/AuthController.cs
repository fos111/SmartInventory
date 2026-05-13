using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.Auth.DTOs.Requests;
using SmartInventory.Application.Auth.DTOs.Responses;
using SmartInventory.Application.Auth.Interfaces;

namespace SmartInventory.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        if (result == null)
            return Conflict(new { message = "Username or email already exists" });

        return Accepted(new { message = result.Message, userId = result.UserId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _authService.LoginAsync(request, ct);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        if (!user.IsActive)
        {
            var limitedUser = await _authService.GetLimitedLoginAsync(request, ct);
            if (limitedUser != null)
                return StatusCode(403, limitedUser);
        }

        return Ok(user);
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
    {
        var success = await _authService.VerifyEmailAsync(token, ct);
        if (!success)
            return BadRequest(new { message = "Invalid or expired token" });

        return Redirect("http://localhost:3000/email-verified");
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken ct)
    {
        var success = await _authService.ResendVerificationEmailAsync(request.Email, ct);
        if (!success)
            return BadRequest(new { message = "Email not found or already verified" });

        return Ok(new { message = "Verification email sent" });
    }

    [HttpPost("request-re-evaluation")]
    public async Task<IActionResult> RequestReEvaluation([FromBody] RequestReEvaluationRequest request, CancellationToken ct)
    {
        var result = await _authService.RequestReEvaluationAsync(request.UserId, ct);
        if (result == null)
            return BadRequest(new { message = "User not found or not eligible for re-evaluation" });

        return Ok(result);
    }
}