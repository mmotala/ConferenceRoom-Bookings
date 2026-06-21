using Microsoft.EntityFrameworkCore;

public static class GetUsers
{
    public sealed record Response(
        Guid Id,
        string Name,
        string Email,
        string Role);

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
            if (!_currentUserService.IsAuthenticated)
            {
                return Result.Failure<List<Response>>(UserErrors.Unauthorized);
            }

            if (!_currentUserService.IsAdmin)
            {
                return Result.Failure<List<Response>>(UserErrors.Forbidden);
            }

            var users = await _context.Users
                .OrderBy(user => user.Name)
                .Select(user => new Response(
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role.ToString()))
                .ToListAsync(cancellationToken);

            return Result.Success(users);
        }
    }
}