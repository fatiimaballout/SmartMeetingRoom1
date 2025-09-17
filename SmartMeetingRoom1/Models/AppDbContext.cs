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
        modelBuilder.Entity<ActionItem>(e =>
        {
            e.ToTable("ActionItems");   // existing table
            e.HasKey(x => x.Id);

            // keep simple so EF doesn't try to change the DB
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.Status).IsRequired();

            e.HasOne(x => x.Minute)
             .WithMany(m => m.ActionItems)
             .HasForeignKey(x => x.MinutesId)   // NOTE: MinutesId (plural) in DB
             .OnDelete(DeleteBehavior.Cascade);
        });

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

        // Attendee -> Identity User (ApplicationUser)
        modelBuilder.Entity<MeetingAttendee>()
            .HasOne(ma => ma.User)
            .WithMany()
            .HasForeignKey(ma => ma.UserId)
            .OnDelete(DeleteBehavior.Restrict);



        // Attachment -> Meeting
        // Attachments ↔ Meeting  (optional FK; null out on meeting delete)
        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Meeting)
            .WithMany(m => m.Attachments)
            .HasForeignKey(a => a.MeetingId)
            .IsRequired(false)                     // <- make FK optional
            .OnDelete(DeleteBehavior.SetNull);     // <- if Meeting is deleted, set MeetingId = NULL


        // ActionItem -> Minute
        modelBuilder.Entity<ActionItem>()
            .HasOne(ai => ai.Minute)
            .WithMany(m => m.ActionItems)
            .HasForeignKey(ai => ai.MinutesId)
            .OnDelete(DeleteBehavior.Cascade);

        // Models/AppDbContext.cs (inside OnModelCreating or fluent config)
        modelBuilder.Entity<Minute>(e =>
        {
            e.ToTable("Minutes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Notes).HasColumnName("Notes");
            e.Property(x => x.Discussion).HasColumnName("Discussion");
            e.Property(x => x.Decisions).HasColumnName("Decisions");
            e.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        });


        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => x.Token)
            .IsUnique();
    }
}
