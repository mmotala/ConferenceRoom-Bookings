using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateBooking
{
    public sealed record Command(
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
            RuleFor(command => command.RoomId)
                .NotEmpty();

            RuleFor(command => command.StartTimeUtc)
                .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
                .WithMessage("Booking start time must be in the future.");

            RuleFor(command => command.EndTimeUtc)
                .GreaterThan(command => command.StartTimeUtc)
                .WithMessage("Booking end time must be after start time.");

            RuleFor(command => command)
                .Must(command => command.EndTimeUtc <= command.StartTimeUtc.AddHours(8))
                .WithMessage("A booking cannot be longer than 8 hours.");

            RuleFor(command => command.Purpose)
                .NotEmpty()
                .MaximumLength(250);
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

            var hasOverlap = await _context.Bookings
                .AnyAsync(booking =>
                    booking.RoomId == command.RoomId &&
                    booking.Status == BookingStatus.Active &&
                    booking.StartTimeUtc < command.EndTimeUtc &&
                    command.StartTimeUtc < booking.EndTimeUtc,
                    cancellationToken);

            if (hasOverlap)
            {
                return Result.Failure<Response>(BookingErrors.RoomUnavailable);
            }

            var booking = new Booking(
                command.RoomId,
                _currentUserService.UserId.Value,
                command.StartTimeUtc,
                command.EndTimeUtc,
                command.Purpose);

            _context.Bookings.Add(booking);

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