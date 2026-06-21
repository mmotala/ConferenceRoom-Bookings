using Microsoft.EntityFrameworkCore;

public sealed class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
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
                .HasMaxLength(50)
                .IsRequired();

            builder.HasQueryFilter(user => !user.IsDeleted);
        });

        modelBuilder.Entity<Room>(builder =>
        {
            builder.HasKey(room => room.Id);

            builder.Property(room => room.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(room => room.Capacity)
                .IsRequired();

            builder.Property(room => room.Location)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasQueryFilter(room => !room.IsDeleted);
        });

        modelBuilder.Entity<Booking>(builder =>
        {
            builder.HasKey(booking => booking.Id);

            builder.Property(booking => booking.StartTimeUtc)
                .IsRequired();

            builder.Property(booking => booking.EndTimeUtc)
                .IsRequired();

            builder.Property(booking => booking.Purpose)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(booking => booking.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

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
            .Entries<IAuditable>()
            .Where(entry =>
                entry.State == EntityState.Added ||
                entry.State == EntityState.Modified);

        var currentUserId = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.CreatedByUserId = currentUserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = utcNow;
                entry.Entity.UpdatedByUserId = currentUserId;
            }
        }
    }
}