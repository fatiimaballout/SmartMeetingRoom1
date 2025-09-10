using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartMeetingRoom1.Models;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Domain tables – rename the domain User set:
    public DbSet<User> DomainUsers { get; set; }
    public DbSet<Meeting> Meetings { get; set; } = default!;
    public DbSet<Minute> Minutes { get; set; } = default!;
    public DbSet<MeetingAttendee> MeetingAttendees { get; set; } = default!;
    public DbSet<Attachment> Attachments { get; set; } = default!;
    public DbSet<Room> Rooms { get; set; } = default!;
    public DbSet<Notification> Notifications { get; set; } = default!;
    public DbSet<ActionItem> ActionItems { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Minute -> Creator (Identity user), no back-collection
        modelBuilder.Entity<Minute>()
            .HasOne(m => m.Creator)
            .WithMany()
            .HasForeignKey(m => m.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Meeting -> Organizer (Identity user), no back-collection
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.Organizer)
            .WithMany()
            .HasForeignKey(m => m.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Meeting -> Room
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.Room)
            .WithMany(r => r.Meetings)
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // MeetingAttendee composite key
        modelBuilder.Entity<MeetingAttendee>()
            .HasKey(ma => new { ma.MeetingId, ma.UserId });

        modelBuilder.Entity<MeetingAttendee>()
            .HasOne(ma => ma.Meeting)
            .WithMany(m => m.Attendees)
            .HasForeignKey(ma => ma.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Attendee -> domain User with back-collection
        modelBuilder.Entity<MeetingAttendee>()
            .HasOne(ma => ma.User)
            .WithMany(u => u.MeetingAttendees)
            .HasForeignKey(ma => ma.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attachment -> Meeting
        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Meeting)
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        // ActionItem -> Minute
        modelBuilder.Entity<ActionItem>()
            .HasOne(ai => ai.Minute)
            .WithMany(m => m.ActionItems)
            .HasForeignKey(ai => ai.MinutesId)
            .OnDelete(DeleteBehavior.Cascade);

        // ActionItem -> Assignee (domain User)
        modelBuilder.Entity<ActionItem>()
            .HasOne(ai => ai.Assignee)
            .WithMany(u => u.AssignedActionItems)
            .HasForeignKey(ai => ai.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => x.Token)
            .IsUnique();
    }
}
