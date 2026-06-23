using ConferenceRoomBooking.Application.Features.Bookings;

public static class BookingsEndpoints
{
    public static IEndpointRouteBuilder MapBookingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapGet("/", async (
            BookingStatus? status,
            GetBookings.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(status, cancellationToken);

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
            UpdateBookings.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateBookings.Command(
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

        group.MapGet("/calendar", async (
            DateTime fromUtc,
            DateTime toUtc,
            Guid? roomId,
            bool includeCancelled,
            GetBookingCalendar.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new GetBookingCalendar.Query(
                    fromUtc,
                    toUtc,
                    roomId,
                    includeCancelled),
                cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPost("/recurring", async (
            CreateRecurringBooking.Command command,
            CreateRecurringBooking.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPost("/series/{seriesId:Guid}/cancel", async (
            Guid seriesId,
            CancelRecurringBookingSeries.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new CancelRecurringBookingSeries.Command(seriesId),
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