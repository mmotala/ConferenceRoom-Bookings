using Microsoft.EntityFrameworkCore;

public static class CancelBooking
{
    public sealed record Command(Guid BookingId);

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

        public async Task<Result> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
            {
                return Result.Failure(UserErrors.Unauthorized);
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(booking => booking.Id == command.BookingId, cancellationToken);

            if (booking is null)
            {
                return Result.Failure(BookingErrors.NotFound);
            }

            var isOwner = booking.UserId == _currentUserService.UserId.Value;

            if (!isOwner && !_currentUserService.IsAdmin)
            {
                return Result.Failure(UserErrors.Forbidden);
            }

            if (!booking.IsActive)
            {
                return Result.Failure(BookingErrors.CannotCancel);
            }

            booking.Cancel();

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}