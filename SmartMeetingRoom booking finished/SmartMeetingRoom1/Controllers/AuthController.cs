using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Models;
using SmartMeetingRoom1.Services;
using System.Security.Claims;
using SmartMeetingRoom1.Interfaces;

namespace SmartMeetingRoom1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IToken _tokenService;
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              IToken tokenService,
                              AppDbContext db,
                              IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _db = db;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Conflict("Email already in use.");

            var userName = string.IsNullOrWhiteSpace(dto.UserName) ? dto.Email : dto.UserName;

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = dto.Email,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            var role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;
            if (!await _db.Roles.AnyAsync(r => r.Name == role)) role = "User";
            await _userManager.AddToRoleAsync(user, role);

            return StatusCode(201);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(dto.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(dto.UserNameOrEmail);
            if (user == null) return Unauthorized("Invalid credentials.");

            var signIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            // 🔎 detailed failure reasons (added)
            if (!signIn.Succeeded)
            {
                if (signIn.IsLockedOut) return Unauthorized("Locked out.");
                if (signIn.IsNotAllowed) return Unauthorized("Not allowed (confirm email/phone?).");
                if (signIn.RequiresTwoFactor) return Unauthorized("Two-factor required.");
                return Unauthorized("Invalid credentials.");
            }

            var (access, refresh) = await _tokenService.CreateTokensAsync(
                user, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            var expiresIn = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "15") * 60;

            return Ok(new TokenResponseDto
            {
                AccessToken = access,
                RefreshToken = refresh.Token,
                ExpiresInSeconds = expiresIn
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshRequestDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null) return Unauthorized();

            var idStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(idStr);
            if (user == null) return Unauthorized();

            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Token == dto.RefreshToken);
            if (token == null || !token.IsActive) return Unauthorized("Invalid refresh token.");

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var (access, newRefresh) = await _tokenService.CreateTokensAsync(user, token.RevokedByIp ?? "unknown");
            token.ReplacedByToken = newRefresh.Token;
            await _db.SaveChangesAsync();

            var expiresIn = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "15") * 60;

            return Ok(new TokenResponseDto
            {
                AccessToken = access,
                RefreshToken = newRefresh.Token,
                ExpiresInSeconds = expiresIn
            });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId == userId);
            if (token == null || !token.IsActive) return NotFound();

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var u = await _userManager.FindByIdAsync(id.ToString());
            if (u == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(u);
            return Ok(new { u.Id, u.UserName, u.Email, roles });
        }
    }
}
