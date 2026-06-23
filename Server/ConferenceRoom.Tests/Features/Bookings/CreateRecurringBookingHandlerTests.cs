using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class CreateRecurringBookingHandlerTests
{
    [Fact]
    public async Task Handle_WithWeeklyRecurrence_CreatesSeriesAndOccurrences()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var handler = CreateHandler(fixture);
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);

        var result = await handler.Handle(new CreateRecurringBooking.Command(
            fixture.Room.Id,
            start,
            end,
            "Weekly planning",
            RecurrenceType.Weekly,
            start.AddDays(14)));

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.CreatedBookingsCount);

        var series = await fixture.Context.RecurringBookingSeries.SingleAsync();
        var bookings = await fixture.Context.Bookings
            .OrderBy(booking => booking.StartTimeUtc)
            .ToListAsync();

        Assert.Equal(series.Id, result.Value.RecurringBookingSeriesId);
        Assert.All(bookings, booking => Assert.Equal(series.Id, booking.RecurringBookingSeriesId));
        Assert.Equal(start, bookings[0].StartTimeUtc);
        Assert.Equal(start.AddDays(7), bookings[1].StartTimeUtc);
        Assert.Equal(start.AddDays(14), bookings[2].StartTimeUtc);
    }

    [Fact]
    public async Task Handle_WhenAnyOccurrenceOverlaps_ReturnsRoomUnavailable()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var start = DateTime.UtcNow.AddDays(1);
        await fixture.AddBookingAsync(
            start.AddDays(7),
            start.AddDays(7).AddHours(1),
            "Existing booking");

        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new CreateRecurringBooking.Command(
            fixture.Room.Id,
            start,
            start.AddHours(1),
            "Weekly planning",
            RecurrenceType.Weekly,
            start.AddDays(14)));

        Assert.True(result.IsFailure);
        Assert.Equal(BookingErrors.RoomUnavailable, result.Error);
        Assert.Empty(fixture.Context.RecurringBookingSeries);
    }

    private static CreateRecurringBooking.Handler CreateHandler(BookingTestFixture fixture)
    {
        return new CreateRecurringBooking.Handler(
            fixture.Context,
            fixture.CurrentUser,
            new BookingAvailabilityService(fixture.Context),
            new CreateRecurringBooking.Validator());
    }
}
