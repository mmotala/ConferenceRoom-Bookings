using Microsoft.EntityFrameworkCore;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IDomainEventDispatcher? domainEventDispatcher = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<RecurringBookingSeries> RecurringBookingSeries => Set<RecurringBookingSeries>();

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

            builder.Property(booking => booking.RecurringBookingSeriesId)
                .IsRequired(false);

            builder.HasIndex(booking => new
            {
                booking.RoomId,
                booking.StartTimeUtc,
                booking.EndTimeUtc
            });

            builder.HasOne(booking => booking.Room)
                .WithMany()
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(booking => booking.User)
                .WithMany()
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(booking => booking.RecurringBookingSeries)
                .WithMany()
                .HasForeignKey(booking => booking.RecurringBookingSeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(booking => !booking.IsDeleted);

            modelBuilder.Entity<RecurringBookingSeries>(builder =>
            {
                builder.HasKey(series => series.Id);

                builder.Property(series => series.Purpose)
                    .HasMaxLength(250)
                    .IsRequired();

                builder.Property(series => series.RecurrenceType)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                builder.HasQueryFilter(series => !series.IsDeleted);
            });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchDomainEventsAsync(cancellationToken);

        return result;
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker
            .Entries<IAuditable>()
            .Where(entry =>
                entry.State == EntityState.Added ||
                entry.State == EntityState.Modified);

        var currentUserId = _currentUserService?.UserId;
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

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_domainEventDispatcher is null)
        {
            return;
        }

        var entitiesWithEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count != 0)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
    }
}