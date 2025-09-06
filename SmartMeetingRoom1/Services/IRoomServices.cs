using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;


namespace SmartMeetingRoom1.Services
{
    public class IRoomServices : IRoom
    {
        private readonly AppDbContext _db;

        public IRoomServices(AppDbContext db)
        {
            _db = db;
        }
        private RoomDto MapToDto(Room room) => new()
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            Location = room.Location,
            Features = room.Features,
            CreatedAt = room.CreatedAt
        };

        public async Task<RoomDto?> GetByIdAsync (int id)
        {
            var room = await _db.Rooms
               .AsNoTracking()
               .FirstOrDefaultAsync(r => r.Id == id);

            return room == null ? null : MapToDto(room);
        }
        public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
        {
            var room = new Room
            {
                Name = dto.Name,
                Capacity = dto.Capacity,
                Location = dto.Location,
                Features = dto.Features,
                CreatedAt = DateTime.UtcNow
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            return MapToDto(room);
        }

        public async Task<bool> UpdateAsync(int id, UpdateRoomDto dto)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return false;

            room.Name = dto.Name;
            room.Capacity = dto.Capacity;
            room.Location = dto.Location;
            room.Features = dto.Features;

            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return false;

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoomDto>> GetAllAsync()
        {
            return await _db.Rooms
                .AsNoTracking()
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Capacity = r.Capacity,
                    // you removed Status in the UI, so don’t send it
                    Features = r.Features,   // comma-separated or array, just match your DTO
                    Location = r.Location,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }


    }
}