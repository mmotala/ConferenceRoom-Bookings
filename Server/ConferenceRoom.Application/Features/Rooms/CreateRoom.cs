using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateRoom
{
    public sealed record Command(
        string Name,
        int Capacity,
        string Location);

    public sealed record Response(
        Guid Id,
        string Name,
        int Capacity,
        string Location);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(command => command.Capacity)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(command => command.Location)
                .NotEmpty()
                .MaximumLength(100);
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

            if (!_currentUserService.IsAuthenticated)
            {
                return Result.Failure<Response>(UserErrors.Unauthorized);
            }

            if (!_currentUserService.IsAdmin)
            {
                return Result.Failure<Response>(UserErrors.Forbidden);
            }

            var roomExists = await _context.Rooms
                .AnyAsync(room => room.Name == command.Name, cancellationToken);

            if (roomExists)
            {
                return Result.Failure<Response>(RoomErrors.AlreadyExists);
            }

            var room = new Room(
                command.Name,
                command.Capacity,
                command.Location);

            _context.Rooms.Add(room);

            await _context.SaveChangesAsync(cancellationToken);

            var response = new Response(
                room.Id,
                room.Name,
                room.Capacity,
                room.Location);

            return Result.Success(response);
        }
    }
}