using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartMeetingRoom1.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SmartMeetingRoom1.Interfaces;
namespace SmartMeetingRoom1.Services
{
    public class ITokenService : IToken
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public ITokenService(IConfiguration config, UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _config = config;
            _userManager = userManager;
            _db = db;
        }

        public async Task<(string accessToken, RefreshToken refreshToken)> CreateTokensAsync(ApplicationUser user, string ipAddress)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"] ?? "15")),
                signingCredentials: creds
            );

            var access = new JwtSecurityTokenHandler().WriteToken(token);

            var refresh = new RefreshToken
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(jwt["RefreshTokenDays"] ?? "7")),
                CreatedByIp = ipAddress
            };
            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return (access, refresh);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwt = _config.GetSection("Jwt");
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
            };

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, parameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch { return null; }
        }
    }
}
