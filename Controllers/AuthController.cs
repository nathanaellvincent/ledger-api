using LedgerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LedgerApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    public record AuthRequest(string Username, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest req)
    {
        var token = await authService.RegisterAsync(req.Username, req.Password);
        if (token is null)
            return Conflict(new { error = "username already taken" });
        return Ok(new { token, req.Username });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest req)
    {
        var token = await authService.LoginAsync(req.Username, req.Password);
        if (token is null)
            return Unauthorized(new { error = "invalid credentials" });
        return Ok(new { token, req.Username });
    }
}
