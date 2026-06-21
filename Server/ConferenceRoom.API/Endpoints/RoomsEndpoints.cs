public static class RoomsEndpoints
{
    public static IEndpointRouteBuilder MapRoomsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rooms")
            .WithTags("Rooms");

        group.MapGet("/", async (
            GetRooms.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPost("/", async (
            CreateRoom.Command command,
            CreateRoom.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult();
        });

        group.MapDelete("/{roomId:guid}", async (
            Guid roomId,
            DeleteRoom.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new DeleteRoom.Command(roomId),
                cancellationToken);

            return result.ToHttpResult();
        });

        return app;
    }
}