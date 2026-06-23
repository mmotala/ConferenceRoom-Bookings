using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateRecurringBooking
{
    public sealed record Command(
        Guid RoomId,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        string Purpose,
        RecurrenceType RecurrenceType,
        DateTime RecurrenceUntilUtc);

    public sealed record Response(
        Guid RecurringBookingSeriesId,
        int CreatedBookingsCount,
        List<CreatedBookingResponse> Bookings);

    public sealed record CreatedBookingResponse(
        Guid BookingId,
        Guid RoomId,
        Guid UserId,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        string Purpose,
        string Status);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.RoomId)
                .NotEmpty()
                .WithMessage("Please select a room.");

            RuleFor(command => command.StartTimeUtc)
                .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
                .WithMessage("Start time must be in the future.");

            RuleFor(command => command.EndTimeUtc)
                .GreaterThan(command => command.StartTimeUtc)
                .WithMessage("End time must be after start time.");

            RuleFor(command => command)
                .Must(command => command.EndTimeUtc <= command.StartTimeUtc.AddHours(8))
                .WithMessage("A booking cannot be longer than 8 hours.");

            RuleFor(command => command.Purpose)
                .NotEmpty()
                .WithMessage("Please enter a purpose for the booking.")
                .MaximumLength(250)
                .WithMessage("Purpose cannot be longer than 250 characters.");

            RuleFor(command => command.RecurrenceType)
                .Must(type => type is RecurrenceType.Daily or RecurrenceType.Weekly or RecurrenceType.Monthly)
                .WithMessage("Please select a valid recurrence type.");

            RuleFor(command => command.RecurrenceUntilUtc)
                .GreaterThan(command => command.StartTimeUtc)
                .WithMessage("Recurring bookings must end after the first booking date.");

            RuleFor(command => command)
                .Must(command => command.RecurrenceUntilUtc <= command.StartTimeUtc.AddMonths(6))
                .WithMessage("Recurring bookings can only be created up to 6 months ahead.");
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

            var roomExists = await _context.Rooms
                .AnyAsync(room => room.Id == command.RoomId, cancellationToken);

            if (!roomExists)
            {
                return Result.Failure<Response>(RoomErrors.NotFound);
            }

            var userExists = await _context.Users
                .AnyAsync(user => user.Id == _currentUserService.UserId.Value, cancellationToken);

            if (!userExists)
            {
                return Result.Failure<Response>(UserErrors.NotFound);
            }

            var occurrences = BuildOccurrences(command).ToList();

            if (occurrences.Count == 0)
            {
                return Result.Failure<Response>(Error.Validation("No recurring booking dates were generated."));
            }

            var hasOverlap = await HasAnyOverlap(
                command.RoomId,
                occurrences,
                cancellationToken);

            if (hasOverlap)
            {
                return Result.Failure<Response>(BookingErrors.RoomUnavailable);
            }

            var series = new RecurringBookingSeries(
                _currentUserService.UserId.Value,
                command.RoomId,
                command.StartTimeUtc,
                command.EndTimeUtc,
                command.RecurrenceType,
                command.RecurrenceUntilUtc,
                command.Purpose);

            _context.RecurringBookingSeries.Add(series);

            var bookings = occurrences
                .Select(occurrence => new Booking(
                    command.RoomId,
                    _currentUserService.UserId.Value,
                    occurrence.StartTimeUtc,
                    occurrence.EndTimeUtc,
                    command.Purpose,
                    series.Id))
                .ToList();

            _context.Bookings.AddRange(bookings);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new Response(
                series.Id,
                bookings.Count,
                bookings.Select(booking => new CreatedBookingResponse(
                    booking.Id,
                    booking.RoomId,
                    booking.UserId,
                    booking.StartTimeUtc,
                    booking.EndTimeUtc,
                    booking.Purpose,
                    booking.Status.ToString())).ToList()));
        }

        private static IEnumerable<(DateTime StartTimeUtc, DateTime EndTimeUtc)> BuildOccurrences(
            Command command)
        {
            var currentStart = command.StartTimeUtc;
            var currentEnd = command.EndTimeUtc;

            while (currentStart <= command.RecurrenceUntilUtc)
            {
                yield return (currentStart, currentEnd);

                (currentStart, currentEnd) = command.RecurrenceType switch
                {
                    RecurrenceType.Daily => (currentStart.AddDays(1), currentEnd.AddDays(1)),
                    RecurrenceType.Weekly => (currentStart.AddDays(7), currentEnd.AddDays(7)),
                    RecurrenceType.Monthly => (currentStart.AddMonths(1), currentEnd.AddMonths(1)),
                    _ => throw new InvalidOperationException("Invalid recurrence type.")
                };
            }
        }

        private async Task<bool> HasAnyOverlap(
            Guid roomId,
            List<(DateTime StartTimeUtc, DateTime EndTimeUtc)> occurrences,
            CancellationToken cancellationToken)
        {
            foreach (var occurrence in occurrences)
            {
                var hasOverlap = await _context.Bookings
                    .AnyAsync(booking =>
                        booking.RoomId == roomId &&
                        booking.Status == BookingStatus.Active &&
                        booking.StartTimeUtc < occurrence.EndTimeUtc &&
                        occurrence.StartTimeUtc < booking.EndTimeUtc,
                        cancellationToken);

                if (hasOverlap)
                {
                    return true;
                }
            }

            return false;
        }
    }
}