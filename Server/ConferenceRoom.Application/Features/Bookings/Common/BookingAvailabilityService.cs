using Microsoft.EntityFrameworkCore;

public sealed class BookingAvailabilityService : IBookingAvailabilityService
{
    private readonly IApplicationDbContext _context;

    public BookingAvailabilityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasRoomOverlapAsync(
        Guid roomId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        Guid? bookingIdToExclude = null,
        CancellationToken cancellationToken = default)
    {
        return _context.Bookings
            .AnyAsync(booking =>
                booking.RoomId == roomId &&
                booking.Status == BookingStatus.Active &&
                (!bookingIdToExclude.HasValue || booking.Id != bookingIdToExclude.Value) &&
                booking.StartTimeUtc < endTimeUtc &&
                startTimeUtc < booking.EndTimeUtc,
                cancellationToken);
    }

    public async Task<bool> HasAnyRoomOverlapAsync(
        Guid roomId,
        IReadOnlyCollection<BookingTimeRange> bookingTimeRanges,
        CancellationToken cancellationToken = default)
    {
        foreach (var bookingTimeRange in bookingTimeRanges)
        {
            var hasOverlap = await HasRoomOverlapAsync(
                roomId,
                bookingTimeRange.StartTimeUtc,
                bookingTimeRange.EndTimeUtc,
                cancellationToken: cancellationToken);

            if (hasOverlap)
            {
                return true;
            }
        }

        return false;
    }
}
