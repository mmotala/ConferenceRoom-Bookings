using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class UpdateBookingHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_UpdatesExistingBooking()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var booking = await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Original purpose");

        var handler = CreateHandler(fixture);
        var newStart = DateTime.UtcNow.AddHours(4);
        var newEnd = DateTime.UtcNow.AddHours(5);

        var result = await handler.Handle(new UpdateBooking.Command(
            booking.Id,
            fixture.Room.Id,
            newStart,
            newEnd,
            "Updated purpose"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated purpose", result.Value.Purpose);
        Assert.Equal(newStart, result.Value.StartTimeUtc);
        Assert.Equal(newEnd, result.Value.EndTimeUtc);

        var updatedBooking = await fixture.Context.Bookings.SingleAsync();
        Assert.Equal("Updated purpose", updatedBooking.Purpose);
        Assert.Equal(newStart, updatedBooking.StartTimeUtc);
        Assert.Equal(newEnd, updatedBooking.EndTimeUtc);
    }

    [Fact]
    public async Task Handle_WhenNewTimeOverlapsAnotherBooking_ReturnsRoomUnavailableConflict()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var bookingToUpdate = await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Booking to update");

        await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(5),
            DateTime.UtcNow.AddHours(6),
            "Blocking booking");

        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdateBooking.Command(
            bookingToUpdate.Id,
            fixture.Room.Id,
            DateTime.UtcNow.AddHours(5.5),
            DateTime.UtcNow.AddHours(6.5),
            "Overlapping update"));

        Assert.True(result.IsFailure);
        Assert.Equal(BookingErrors.RoomUnavailable, result.Error);
    }

    [Fact]
    public async Task Handle_WhenBookingIsCancelled_ReturnsCannotUpdateCancelledBooking()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var booking = await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Cancelled booking");
        booking.Cancel();
        await fixture.Context.SaveChangesAsync();

        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdateBooking.Command(
            booking.Id,
            fixture.Room.Id,
            DateTime.UtcNow.AddHours(4),
            DateTime.UtcNow.AddHours(5),
            "Updated purpose"));

        Assert.True(result.IsFailure);
        Assert.Equal(BookingErrors.CannotUpdateCancelledBooking, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserDoesNotOwnBooking_ReturnsForbidden()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var otherUser = await fixture.AddUserAsync();
        var booking = await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Someone else's booking",
            otherUser.Id);

        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdateBooking.Command(
            booking.Id,
            fixture.Room.Id,
            DateTime.UtcNow.AddHours(4),
            DateTime.UtcNow.AddHours(5),
            "Updated purpose"));

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.Forbidden, result.Error);
    }

    private static UpdateBooking.Handler CreateHandler(BookingTestFixture fixture)
    {
        return new UpdateBooking.Handler(
            fixture.Context,
            fixture.CurrentUser,
            new UpdateBooking.Validator());
    }
}
