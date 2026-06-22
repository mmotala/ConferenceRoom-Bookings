using Microsoft.Extensions.Logging;

public sealed class DummyNotificationService : INotificationService
{
    private readonly ILogger<DummyNotificationService> _logger;

    public DummyNotificationService(ILogger<DummyNotificationService> logger)
    {
        _logger = logger;
    }

    public Task BookingCreatedAsync(
        Guid bookingId,
        Guid roomId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Dummy Email notification: Booking created. BookingId={BookingId}, RoomId={RoomId}, UserId={UserId}, Start={StartTimeUtc}, End={EndTimeUtc}, Purpose={Purpose}",
            bookingId,
            roomId,
            userId,
            startTimeUtc,
            endTimeUtc,
            purpose);

        _logger.LogInformation(
            "Dummy SignalR notification: Booking created. BookingId={BookingId}, RoomId={RoomId}, UserId={UserId}",
            bookingId,
            roomId,
            userId);

        _logger.LogInformation(
            "Dummy SMS notification: Booking created. BookingId={BookingId}, UserId={UserId}",
            bookingId,
            userId);

        return Task.CompletedTask;
    }

    public Task BookingUpdatedAsync(
        Guid bookingId,
        Guid roomId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Dummy Email notification: Booking updated. BookingId={BookingId}, RoomId={RoomId}, UserId={UserId}, Start={StartTimeUtc}, End={EndTimeUtc}, Purpose={Purpose}",
            bookingId,
            roomId,
            userId,
            startTimeUtc,
            endTimeUtc,
            purpose);

        _logger.LogInformation(
            "Dummy SignalR notification: Booking updated. BookingId={BookingId}, RoomId={RoomId}, UserId={UserId}",
            bookingId,
            roomId,
            userId);

        _logger.LogInformation(
            "Dummy SMS notification: Booking updated. BookingId={BookingId}, UserId={UserId}",
            bookingId,
            userId);

        return Task.CompletedTask;
    }

    public Task BookingCancelledAsync(
        Guid bookingId,
        Guid roomId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Dummy Email notification: Booking cancelled. BookingId={BookingId}, RoomId={RoomId}, UserId={UserId}, Start={StartTimeUtc}, End={EndTimeUtc}",
            bookingId,
            roomId,
            userId,
            startTimeUtc,
            endTimeUtc);

        _logger.LogInformation(
            "Dummy SignalR notification: Booking cancelled. BookingId={BookingId}, RoomId={RoomId}, UserId={UserId}",
            bookingId,
            roomId,
            userId);

        _logger.LogInformation(
            "Dummy SMS notification: Booking cancelled. BookingId={BookingId}, UserId={UserId}",
            bookingId,
            userId);

        return Task.CompletedTask;
    }
}