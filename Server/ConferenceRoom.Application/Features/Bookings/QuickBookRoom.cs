using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class QuickBookRoom
{
    public sealed record Command(
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        int NumberOfPeople,
        string Purpose);

    public sealed record Response(
        Guid BookingId,
        Guid RoomId,
        string RoomName,
        int RoomCapacity,
        Guid UserId,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        int NumberOfPeople,
        string Purpose,
        string Status);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.StartTimeUtc)
                .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
                .WithMessage("Booking start time must be in the future.");

            RuleFor(command => command.EndTimeUtc)
                .GreaterThan(command => command.StartTimeUtc)
                .WithMessage("Booking end time must be after start time.");

            RuleFor(command => command)
                .Must(command => command.EndTimeUtc <= command.StartTimeUtc.AddHours(8))
                .WithMessage("A booking cannot be longer than 8 hours.");

            RuleFor(command => command.NumberOfPeople)
                .GreaterThan(0)
                .WithMessage("Number of people must be greater than zero.");

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

            var userExists = await _context.Users
                .AnyAsync(user => user.Id == _currentUserService.UserId.Value, cancellationToken);

            if (!userExists)
            {
                return Result.Failure<Response>(UserErrors.NotFound);
            }

            var bookedRoomIds = await _context.Bookings
                .Where(booking =>
                    booking.Status == BookingStatus.Active &&
                    booking.StartTimeUtc < command.EndTimeUtc &&
                    command.StartTimeUtc < booking.EndTimeUtc)
                .Select(booking => booking.RoomId)
                .ToListAsync(cancellationToken);

            var bestRoom = await _context.Rooms
                .Where(room =>
                    room.Capacity >= command.NumberOfPeople &&
                    !bookedRoomIds.Contains(room.Id))
                .OrderBy(room => room.Capacity)
                .ThenBy(room => room.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (bestRoom is null)
            {
                return Result.Failure<Response>(BookingErrors.NoSuitableRoomAvailable);
            }

            var booking = new Booking(
                bestRoom.Id,
                _currentUserService.UserId.Value,
                command.StartTimeUtc,
                command.EndTimeUtc,
                command.Purpose);

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new Response(
                booking.Id,
                bestRoom.Id,
                bestRoom.Name,
                bestRoom.Capacity,
                booking.UserId,
                booking.StartTimeUtc,
                booking.EndTimeUtc,
                command.NumberOfPeople,
                booking.Purpose,
                booking.Status.ToString()));
        }
    }
}