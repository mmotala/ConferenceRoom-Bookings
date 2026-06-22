public interface INotificationService
{
    Task BookingCreatedAsync(
        Guid bookingId,
        Guid roomId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string purpose,
        CancellationToken cancellationToken = default);

    Task BookingUpdatedAsync(
        Guid bookingId,
        Guid roomId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string purpose,
        CancellationToken cancellationToken = default);

    Task BookingCancelledAsync(
        Guid bookingId,
        Guid roomId,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        CancellationToken cancellationToken = default);
}