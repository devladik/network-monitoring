using System.Security.Claims;
using a_webapi.DTOs.Auth;
using a_webapi.Identity;
using a_webapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace a_webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthService _authService;

    public AuthController(UserManager<ApplicationUser> userManager, AuthService authService)
    {
        _userManager = userManager;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest("User already exists");

        var user = new ApplicationUser()
        {
            UserName = dto.Email,
            Email = dto.Email,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("User created successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Invalid email or password");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Unauthorized("Invalid email or password");

        var tokens = await _authService.CreateTokenPairAsync(user);

        return Ok(tokens);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
    {
        var tokens = await _authService.RefreshAsync(dto.RefreshToken);
        if (tokens == null)
            return Unauthorized("Invalid or expired refresh token");
        
        return Ok(tokens);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();
        
        await _authService.RevokeAsync(dto.RefreshToken,  userId);
        return NoContent();
    }
}