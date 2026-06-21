using Microsoft.EntityFrameworkCore;

public static class GetBookings
{
    public sealed record Response(
        Guid Id,
        Guid RoomId,
        string RoomName,
        Guid UserId,
        string UserName,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        string Purpose,
        string Status);

    public sealed class Handler
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public Handler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<List<Response>>> Handle(
            CancellationToken cancellationToken = default)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
            {
                return Result.Failure<List<Response>>(UserErrors.Unauthorized);
            }

            var query = _context.Bookings
                .Include(booking => booking.Room)
                .Include(booking => booking.User)
                .AsQueryable();

            if (!_currentUserService.IsAdmin)
            {
                query = query.Where(booking => booking.UserId == _currentUserService.UserId.Value);
            }

            var bookings = await query
                .OrderByDescending(booking => booking.StartTimeUtc)
                .Select(booking => new Response(
                    booking.Id,
                    booking.RoomId,
                    booking.Room.Name,
                    booking.UserId,
                    booking.User.Name,
                    booking.StartTimeUtc,
                    booking.EndTimeUtc,
                    booking.Purpose,
                    booking.Status.ToString()))
                .ToListAsync(cancellationToken);

            return Result.Success(bookings);
        }
    }
}