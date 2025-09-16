using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Dtos;
using SmartMeetingRoom1.Interfaces;
using SmartMeetingRoom1.Models;

namespace SmartMeetingRoom1.Services
{
    public class IActionItemServices : IActionItem
    {
        private readonly AppDbContext _db;

        public IActionItemServices(AppDbContext db)
        {
            _db = db;
        }

        private ActionItemDto MapToDto(ActionItem a) => new()
        {
            Id = a.Id,
            MinutesId = a.MinutesId,
            AssignedTo = a.AssignedTo,
            Description = a.Description,
            Status = a.Status,
            DueDate = a.DueDate
        };

        public async Task<ActionItemDto?> GetByIdAsync(int id)
        {
            var action = await _db.ActionItems
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return action == null ? null : MapToDto(action);
        }

        public async Task<ActionItemDto> CreateAsync(CreateActionItemDto dto)
        {
            if (!await _db.Minutes.AnyAsync(m => m.Id == dto.MinutesId))
                throw new ArgumentException($"Minute ID {dto.MinutesId} does not exist.");

            // If you want to *optionally* validate when a numeric ID is supplied:
            // if (int.TryParse(dto.AssignedTo, out var uid) &&
            //     !await _db.Users.AnyAsync(u => u.Id == uid))
            //     throw new ArgumentException($"User ID {uid} does not exist.");

            var action = new ActionItem
            {
                MinutesId = dto.MinutesId,
                Description = dto.Description.Trim(),
                AssignedTo = string.IsNullOrWhiteSpace(dto.AssignedTo) ? null : dto.AssignedTo.Trim(),
                DueDate = dto.DueDate,
                Status = dto.Status ?? "Pending"
            };

            _db.ActionItems.Add(action);
            await _db.SaveChangesAsync();

            return MapToDto(action);
        }


        public async Task<bool> UpdateAsync(int id, UpdateActionItemDto dto)
        {
            var action = await _db.ActionItems.FindAsync(id);
            if (action == null) return false;

            action.Description = dto.Description;
            action.Status = dto.Status;
            action.DueDate = dto.DueDate;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var action = await _db.ActionItems.FindAsync(id);
            if (action == null) return false;

            _db.ActionItems.Remove(action);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
