using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class DummyLogin
{
    public sealed record Command(string Email);

    public sealed record Response(
        Guid UserId,
        string Name,
        string Email,
        string Role);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }

    public sealed class Handler
    {
        private readonly IApplicationDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(
            IApplicationDbContext context,
            IValidator<Command> validator)
        {
            _context = context;
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

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    user => user.Email.ToLower() == command.Email.ToLower(),
                    cancellationToken);

            if (user is null)
            {
                return Result.Failure<Response>(UserErrors.NotFound);
            }

            return Result.Success(new Response(
                user.Id,
                user.Name,
                user.Email,
                user.Role.ToString()));
        }
    }
}