using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateUser
{
    public sealed record Command(
        string Name,
        string Email,
        UserRole Role);

    public sealed record Response(
        Guid Id,
        string Name,
        string Email,
        string Role);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(command => command.Role)
                .IsInEnum();
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

            var emailExists = await _context.Users
                .AnyAsync(user => user.Email == command.Email, cancellationToken);

            if (emailExists)
            {
                return Result.Failure<Response>(UserErrors.EmailAlreadyExists);
            }

            var user = new User(
                command.Name,
                command.Email,
                command.Role);

            _context.Users.Add(user);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new Response(
                user.Id,
                user.Name,
                user.Email,
                user.Role.ToString()));
        }
    }
}