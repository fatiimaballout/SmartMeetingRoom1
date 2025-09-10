using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUser _service;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UsersController(
        IUser service,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _service = service;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ---------------- Existing (kept) ----------------

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> Get(int id)
    {
        var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && myId != id.ToString()) return Forbid();

        var user = await _service.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    // ---------------- "me" helpers ----------------

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(myId)) return Unauthorized();

        var me = await _service.GetByIdAsync(int.Parse(myId));
        if (me == null) return NotFound();
        return Ok(me);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
    {
        var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(myId)) return Unauthorized();

        var ok = await _service.UpdateAsync(int.Parse(myId), dto);
        if (!ok) return NotFound();
        return NoContent();
    }

    // ---------------- Admin: users & employees ----------------

    // DTOs
    public record UserListItemDto(int Id, string FullName, string Email, IEnumerable<string> Roles);
    public record CreateEmployeeDto(string FullName, string Email, string Password);
    public record UpdateEmployeeDto(string? FullName, string? Email, string? NewPassword);
    public record ResetPasswordDto(string NewPassword);

    // NAME: central mapping for display-name so we don't require a FullName property.
    private static string MapName(ApplicationUser u) =>
        // ⚠️ If you later add u.FullName, change to: u.FullName ?? u.UserName ?? ""
        u.UserName ?? "";

    // GET /api/users  — list all users (admin)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> ListAllUsers()
    {
        var users = _userManager.Users.ToList(); // or ToListAsync if using EF async
        var list = new List<UserListItemDto>(users.Count);
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(new UserListItemDto(u.Id, MapName(u), u.Email ?? "", roles));
        }
        return Ok(list);
    }

    // GET /api/users/employees  — only Employee role
    [HttpGet("employees")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> ListEmployees()
    {
        if (!await _roleManager.RoleExistsAsync("Employee"))
            return BadRequest("Employee role does not exist.");

        var employees = await _userManager.GetUsersInRoleAsync("Employee");
        var result = new List<UserListItemDto>(employees.Count);
        foreach (var u in employees)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserListItemDto(u.Id, MapName(u), u.Email ?? "", roles));
        }
        return Ok(result);
    }

    // POST /api/users/employees
    [HttpPost("employees")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserListItemDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        if (!await _roleManager.RoleExistsAsync("Employee"))
            return BadRequest("Employee role does not exist.");

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null) return BadRequest("A user with that email already exists.");

        var user = new ApplicationUser
        {
            // No FullName property — store login identity as email
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var create = await _userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded) return BadRequest(create.Errors);

        await _userManager.AddToRoleAsync(user, "Employee");

        var roles = await _userManager.GetRolesAsync(user);
        var result = new UserListItemDto(user.Id, MapName(user), user.Email ?? "", roles);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, result);
    }

    // GET /api/users/employees/5
    [HttpGet("employees/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserListItemDto>> GetEmployee(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Employee")) return Forbid();

        return Ok(new UserListItemDto(user.Id, MapName(user), user.Email ?? "", roles));
    }

    // PUT /api/users/employees/5
    [HttpPut("employees/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        // NAME: since there's no FullName column, let "FullName" update UserName (display name == login name)
        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.UserName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !dto.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists != null && exists.Id != user.Id)
                return BadRequest("Email is already in use.");
            user.Email = dto.Email;
        }

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded) return BadRequest(update.Errors);

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!reset.Succeeded) return BadRequest(reset.Errors);
        }

        return NoContent();
    }

    // DELETE /api/users/employees/5
    [HttpDelete("employees/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentId == user.Id.ToString())
            return BadRequest("You cannot delete your own account.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Employee")) return Forbid();

        var del = await _userManager.DeleteAsync(user);
        if (!del.Succeeded) return BadRequest(del.Errors);

        return NoContent();
    }

    // POST /api/users/{id}/password  (admin reset any user)
    [HttpPost("{id:int}/password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return NoContent();
    }

    // POST /api/users/{id}/roles/{role}
    [HttpPost("{id:int}/roles/{role}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRole(int id, string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
            return BadRequest($"Role '{role}' does not exist.");

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var res = await _userManager.AddToRoleAsync(user, role);
        if (!res.Succeeded) return BadRequest(res.Errors);

        return NoContent();
    }

    // DELETE /api/users/{id}/roles/{role}
    [HttpDelete("{id:int}/roles/{role}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole(int id, string role)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var res = await _userManager.RemoveFromRoleAsync(user, role);
        if (!res.Succeeded) return BadRequest(res.Errors);

        return NoContent();
    }
}
