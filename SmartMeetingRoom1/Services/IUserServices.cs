using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IUserServices : IUser
    {
        private readonly AppDbContext _db;
        public IUserServices(AppDbContext db) => _db = db;

        private static UserDto MapToDto(User u) => new()
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Phone = u.Phone,
            Role = u.Role,
            CreatedAt = u.CreatedUtc
        };

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _db.DomainUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            if (await _db.DomainUsers.AnyAsync(u => u.Email == dto.Email))
                throw new ArgumentException($"Email '{dto.Email}' is already in use.");

            var user = new User
            {
                Name = dto.Name ?? string.Empty,
                Email = dto.Email ?? string.Empty,
                // ⚠ Prefer to remove Password from this entity; Identity should own passwords.
                Password = dto.Password,
                Phone = dto.Phone,
                Role = dto.Role ?? "Employee",
                CreatedUtc = DateTime.UtcNow
            };

            _db.DomainUsers.Add(user);
            await _db.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<bool> UpdateAsync(int id, UpdateUserDto dto)
        {
            var user = await _db.DomainUsers.FindAsync(id);
            if (user == null) return false;

            var emailExists = await _db.DomainUsers
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailExists)
                throw new ArgumentException($"Email '{dto.Email}' is already in use by another user.");

            user.Name = dto.Name ?? user.Name;
            user.Email = dto.Email ?? user.Email;
            user.Phone = dto.Phone ?? user.Phone;
            user.Role = dto.Role ?? user.Role;

            // only overwrite if provided
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = dto.Password;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _db.DomainUsers.FindAsync(id);
            if (user == null) return false;

            _db.DomainUsers.Remove(user);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
