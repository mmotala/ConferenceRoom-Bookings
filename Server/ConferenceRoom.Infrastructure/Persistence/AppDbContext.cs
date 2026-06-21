using ConferenceRoom.Domain.Entities;
using ConferenceRoom.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(user => user.Id);

            builder.Property(user => user.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(user => user.Email)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(user => user.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasQueryFilter(user => !user.IsDeleted);
        });

        modelBuilder.Entity<Room>(builder =>
        {
            builder.HasKey(room => room.Id);

            builder.Property(room => room.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(room => room.Location)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasQueryFilter(room => !room.IsDeleted);
        });

        modelBuilder.Entity<Booking>(builder =>
        {
            builder.HasKey(booking => booking.Id);

            builder.Property(booking => booking.Purpose)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(booking => booking.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(booking => booking.Room)
                .WithMany()
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(booking => booking.User)
                .WithMany()
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(booking => !booking.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker
            .Entries<BaseEntity>()
            .Where(entry =>
                entry.State == EntityState.Added ||
                entry.State == EntityState.Modified);

        var userId = _currentUserService?.UserId;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                entry.Entity.CreatedByUserId = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                entry.Entity.UpdatedByUserId = userId;
            }
        }
    }
}