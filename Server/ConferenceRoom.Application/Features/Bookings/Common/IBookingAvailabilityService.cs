public interface IBookingAvailabilityService
{
    Task<bool> HasRoomOverlapAsync(
        Guid roomId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        Guid? bookingIdToExclude = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyRoomOverlapAsync(
        Guid roomId,
        IReadOnlyCollection<BookingTimeRange> bookingTimeRanges,
        CancellationToken cancellationToken = default);
}

public sealed record BookingTimeRange(DateTime StartTimeUtc, DateTime EndTimeUtc);
