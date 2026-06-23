using Microsoft.EntityFrameworkCore;

public static class GetBookingCalendar
{
    public sealed record Query(
        DateTime FromUtc,
        DateTime ToUtc,
        Guid? RoomId,
        bool IncludeCancelled = false);

    public sealed record Response(
        Guid BookingId,
        Guid RoomId,
        string RoomName,
        Guid UserId,
        string UserName,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        string Purpose,
        string Status,
        Guid? RecurringBookingSeriesId);

    public sealed class Handler
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Response>>> Handle(
            Query query,
            CancellationToken cancellationToken = default)
        {
            if (query.ToUtc <= query.FromUtc)
            {
                return Result.Failure<List<Response>>(
                    Error.Validation("Calendar end date must be after start date."));
            }

            var bookingsQuery = _context.Bookings
                .Include(booking => booking.Room)
                .Include(booking => booking.User)
                .Where(booking =>
                    booking.StartTimeUtc < query.ToUtc &&
                    query.FromUtc < booking.EndTimeUtc);

            if (query.RoomId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(booking => booking.RoomId == query.RoomId.Value);
            }

            if (!query.IncludeCancelled)
            {
                bookingsQuery = bookingsQuery.Where(booking => booking.Status == BookingStatus.Active);
            }

            var bookings = await bookingsQuery
                .OrderBy(booking => booking.StartTimeUtc)
                .Select(booking => new Response(
                    booking.Id,
                    booking.RoomId,
                    booking.Room.Name,
                    booking.UserId,
                    booking.User.Name,
                    booking.StartTimeUtc,
                    booking.EndTimeUtc,
                    booking.Purpose,
                    booking.Status.ToString(),
                    booking.RecurringBookingSeriesId))
                .ToListAsync(cancellationToken);

            return Result.Success(bookings);
        }
    }
}