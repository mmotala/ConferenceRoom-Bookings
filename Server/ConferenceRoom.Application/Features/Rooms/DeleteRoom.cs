using Microsoft.EntityFrameworkCore;

public static class DeleteRoom
{
    public sealed record Command(Guid RoomId);

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
            if (!_currentUserService.IsAuthenticated)
            {
                return Result.Failure(UserErrors.Unauthorized);
            }

            if (!_currentUserService.IsAdmin)
            {
                return Result.Failure(UserErrors.Forbidden);
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(room => room.Id == command.RoomId, cancellationToken);

            if (room is null)
            {
                return Result.Failure(RoomErrors.NotFound);
            }

            room.SoftDelete(_currentUserService.UserId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}