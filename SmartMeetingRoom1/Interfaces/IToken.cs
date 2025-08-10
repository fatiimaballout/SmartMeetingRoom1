using SmartMeetingRoom1.Models;
using System.Security.Claims;
using SmartMeetingRoom1.Dtos;
namespace SmartMeetingRoom1.Interfaces
{
    public interface IToken
    {
        Task<(string accessToken, RefreshToken refreshToken)> CreateTokensAsync(ApplicationUser user, string ipAddress);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
