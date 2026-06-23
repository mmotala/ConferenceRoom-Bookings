using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CancelRecurringBookingSeries
{
    public sealed record Command(Guid RecurringBookingSeriesId);

    public sealed record Response(
        Guid RecurringBookingSeriesId,
        int CancelledBookingsCount);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.RecurringBookingSeriesId)
                .NotEmpty()
                .WithMessage("Recurring booking series id is required.");
        }
    }

    public sealed class Handler
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<Command> _validator;

        public Handler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IValidator<Command> validator)
        {
            _context = context;
            _currentUserService = currentUserService;
            _validator = validator;
        }

        public async Task<Result<Response>> Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Result.Failure<Response>(validationResult.ToValidationError());
            }

            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
            {
                return Result.Failure<Response>(UserErrors.Unauthorized);
            }

            var series = await _context.RecurringBookingSeries
                .FirstOrDefaultAsync(
                    series => series.Id == command.RecurringBookingSeriesId,
                    cancellationToken);

            if (series is null)
            {
                return Result.Failure<Response>(BookingErrors.NotFound);
            }

            var isOwner = series.UserId == _currentUserService.UserId.Value;

            if (!isOwner && !_currentUserService.IsAdmin)
            {
                return Result.Failure<Response>(UserErrors.Forbidden);
            }

            var bookings = await _context.Bookings
                .Where(booking =>
                    booking.RecurringBookingSeriesId == command.RecurringBookingSeriesId &&
                    booking.Status == BookingStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var booking in bookings)
            {
                booking.Cancel();
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new Response(
                command.RecurringBookingSeriesId,
                bookings.Count));
        }
    }
}