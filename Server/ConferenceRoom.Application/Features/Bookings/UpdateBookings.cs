using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateBookings
{
    public sealed record Command(
        Guid BookingId,
        Guid RoomId,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        string Purpose);

    public sealed record Response(
        Guid Id,
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
            RuleFor(command => command.BookingId)
                .NotEmpty();

            RuleFor(command => command.RoomId)
                .NotEmpty();

            RuleFor(command => command.StartTimeUtc)
                .MustStartInFuture();

            RuleFor(command => command)
                .MustEndAfterStart(
                    command => command.StartTimeUtc,
                    command => command.EndTimeUtc);

            RuleFor(command => command)
                .MustNotExceedMaximumDuration(
                    command => command.StartTimeUtc,
                    command => command.EndTimeUtc);

            RuleFor(command => command.Purpose)
                .NotEmpty()
                .MaximumLength(250);
        }
    }

    public sealed class Handler
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBookingAvailabilityService _bookingAvailabilityService;
        private readonly IValidator<Command> _validator;

        public Handler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IBookingAvailabilityService bookingAvailabilityService,
            IValidator<Command> validator)
        {
            _context = context;
            _currentUserService = currentUserService;
            _bookingAvailabilityService = bookingAvailabilityService;
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

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(booking => booking.Id == command.BookingId, cancellationToken);

            if (booking is null)
            {
                return Result.Failure<Response>(BookingErrors.NotFound);
            }

            var isOwner = booking.UserId == _currentUserService.UserId.Value;

            if (!isOwner && !_currentUserService.IsAdmin)
            {
                return Result.Failure<Response>(UserErrors.Forbidden);
            }

            if (!booking.IsActive)
            {
                return Result.Failure<Response>(BookingErrors.CannotUpdateCancelledBooking);
            }

            var roomExists = await _context.Rooms
                .AnyAsync(room => room.Id == command.RoomId, cancellationToken);

            if (!roomExists)
            {
                return Result.Failure<Response>(RoomErrors.NotFound);
            }

            var hasOverlap = await _bookingAvailabilityService.HasRoomOverlapAsync(
                command.RoomId,
                command.StartTimeUtc,
                command.EndTimeUtc,
                command.BookingId,
                cancellationToken);

            if (hasOverlap)
            {
                return Result.Failure<Response>(BookingErrors.RoomUnavailable);
            }

            booking.Update(
                command.RoomId,
                command.StartTimeUtc,
                command.EndTimeUtc,
                command.Purpose);

            await _context.SaveChangesAsync(cancellationToken);

            var response = new Response(
                booking.Id,
                booking.RoomId,
                booking.UserId,
                booking.StartTimeUtc,
                booking.EndTimeUtc,
                booking.Purpose,
                booking.Status.ToString());

            return Result.Success(response);
        }
    }
}
