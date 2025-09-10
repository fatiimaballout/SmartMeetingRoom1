using System.ComponentModel.DataAnnotations;
using SmartMeetingRoom1.Models;
namespace SmartMeetingRoom1.Dtos
{
    public class RegisterDto
    {
        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required, MinLength(6)] public string Password { get; set; } = default!;
        [Required] public string UserName { get; set; } = default!;
        public string Role { get; set; } = "User"; 
    }

    public class LoginDto
    {
        [Required] public string UserNameOrEmail { get; set; } = default!;
        [Required] public string Password { get; set; } = default!;
    }

    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresInSeconds { get; set; }
    }

    public class RefreshRequestDto
    {
        [Required] public string AccessToken { get; set; } = default!;
        [Required] public string RefreshToken { get; set; } = default!;
    }
}
