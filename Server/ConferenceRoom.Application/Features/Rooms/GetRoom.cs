using Microsoft.EntityFrameworkCore;

public static class GetRooms
{
    public sealed record Response(
        Guid Id,
        string Name,
        int Capacity,
        string Location);

    public sealed class Handler
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Response>>> Handle(
            CancellationToken cancellationToken = default)
        {
            var rooms = await _context.Rooms
                .OrderBy(room => room.Name)
                .Select(room => new Response(
                    room.Id,
                    room.Name,
                    room.Capacity,
                    room.Location))
                .ToListAsync(cancellationToken);

            return Result.Success(rooms);
        }
    }
}