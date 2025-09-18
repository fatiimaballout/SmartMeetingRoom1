// Controllers/ProfileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Models; // ApplicationUser
using System.Security.Claims;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public ProfileController(UserManager<ApplicationUser> um, IWebHostEnvironment env)
    {
        _userManager = um;
        _env = env;
    }

    private async Task<ApplicationUser?> GetMeAsync()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return (int.TryParse(idStr, out var id))
            ? await _userManager.FindByIdAsync(id.ToString())
            : null;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> Get()
    {
        var u = await GetMeAsync();
        if (u is null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(u);
        return Ok(new ProfileDto
        {
            Id = u.Id,
            FullName = u.FullName,
            UserName = u.UserName,
            Email = u.Email,
            Roles = roles.ToArray(),
            AvatarUrl = u.AvatarUrl,
            CreatedUtc = u is { } ? u.CreatedUtc : null,
            LastLoginUtc = u.LastLoginUtc
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        if (dto is null) return BadRequest("Body required.");

        var u = await GetMeAsync();
        if (u is null) return Unauthorized();

        // Update full name
        if (!string.IsNullOrWhiteSpace(dto.FullName))
            u.FullName = dto.FullName.Trim();

        // Update email (and normalized/username if you use email as username)
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Equals(u.Email, StringComparison.OrdinalIgnoreCase))
        {
            // ensure email isn't taken
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null && existing.Id != u.Id)
                return BadRequest("Email is already used by another account.");

            u.Email = dto.Email.Trim();
            u.UserName ??= dto.Email.Trim(); // keep your username read-only on UI
            u.NormalizedEmail = _userManager.NormalizeEmail(u.Email);
            u.NormalizedUserName = _userManager.NormalizeName(u.UserName);
        }

        var result = await _userManager.UpdateAsync(u);
        return result.Succeeded ? NoContent() : BadRequest(string.Join(" | ", result.Errors.Select(e => e.Description)));
    }

    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest("Both currentPassword and newPassword are required.");

        var u = await GetMeAsync();
        if (u is null) return Unauthorized();

        var res = await _userManager.ChangePasswordAsync(u, dto.CurrentPassword, dto.NewPassword);
        return res.Succeeded ? NoContent() : BadRequest(string.Join(" | ", res.Errors.Select(e => e.Description)));
    }

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<object>> UploadAvatar([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0) return BadRequest("No file.");

        var u = await GetMeAsync();
        if (u is null) return Unauthorized();

        // Save to /wwwroot/uploads/avatars/
        var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "avatars");
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(file.FileName);
        var name = $"u-{u.Id}-{DateTime.UtcNow.Ticks}{ext}";
        var path = Path.Combine(folder, name);
        using (var fs = System.IO.File.Create(path)) { await file.CopyToAsync(fs); }

        // store relative URL for <img src>
        u.AvatarUrl = $"/uploads/avatars/{name}";
        await _userManager.UpdateAsync(u);

        return Ok(new { url = u.AvatarUrl });
    }
}
