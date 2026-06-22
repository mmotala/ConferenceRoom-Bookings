public sealed class BookingCreatedDomainEventHandler
    : IDomainEventHandler<BookingCreatedDomainEvent>
{
    private readonly INotificationService _notificationService;

    public BookingCreatedDomainEventHandler(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(
        BookingCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await _notificationService.BookingCreatedAsync(
            domainEvent.BookingId,
            domainEvent.RoomId,
            domainEvent.UserId,
            domainEvent.StartTimeUtc,
            domainEvent.EndTimeUtc,
            domainEvent.Purpose,
            cancellationToken);
    }
}

public sealed class BookingUpdatedDomainEventHandler
    : IDomainEventHandler<BookingUpdatedDomainEvent>
{
    private readonly INotificationService _notificationService;

    public BookingUpdatedDomainEventHandler(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(
        BookingUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await _notificationService.BookingUpdatedAsync(
            domainEvent.BookingId,
            domainEvent.RoomId,
            domainEvent.UserId,
            domainEvent.StartTimeUtc,
            domainEvent.EndTimeUtc,
            domainEvent.Purpose,
            cancellationToken);
    }
}

public sealed class BookingCancelledDomainEventHandler
    : IDomainEventHandler<BookingCancelledDomainEvent>
{
    private readonly INotificationService _notificationService;

    public BookingCancelledDomainEventHandler(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(
        BookingCancelledDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await _notificationService.BookingCancelledAsync(
            domainEvent.BookingId,
            domainEvent.RoomId,
            domainEvent.UserId,
            domainEvent.StartTimeUtc,
            domainEvent.EndTimeUtc,
            cancellationToken);
    }
}