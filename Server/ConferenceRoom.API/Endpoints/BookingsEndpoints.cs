public static class BookingsEndpoints
{
    public static IEndpointRouteBuilder MapBookingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapGet("/", async (
            GetBookings.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPost("/", async (
            CreateBooking.Command command,
            CreateBooking.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPost("/quick", async (
            QuickBookRoom.Command command,
            QuickBookRoom.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPut("/{bookingId:guid}", async (
            Guid bookingId,
            UpdateBookingRequest request,
            UpdateBooking.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateBooking.Command(
                bookingId,
                request.RoomId,
                request.StartTimeUtc,
                request.EndTimeUtc,
                request.Purpose);

            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPost("/{bookingId:guid}/cancel", async (
            Guid bookingId,
            CancelBooking.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new CancelBooking.Command(bookingId),
                cancellationToken);

            return result.ToHttpResult();
        });

        return app;
    }

    private sealed record UpdateBookingRequest(
        Guid RoomId,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        string Purpose);
}