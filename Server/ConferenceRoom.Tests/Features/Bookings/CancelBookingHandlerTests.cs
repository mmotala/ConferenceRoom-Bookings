using Xunit;

public sealed class CancelBookingHandlerTests
{
    [Fact]
    public async Task Handle_WhenCurrentUserOwnsBooking_CancelsBooking()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var booking = await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Booking to cancel");

        var handler = new CancelBooking.Handler(fixture.Context, fixture.CurrentUser);

        var result = await handler.Handle(new CancelBooking.Command(booking.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
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

        var handler = new CancelBooking.Handler(fixture.Context, fixture.CurrentUser);

        var result = await handler.Handle(new CancelBooking.Command(booking.Id));

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.Forbidden, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsAdmin_CancelsAnotherUsersBooking()
    {
        await using var fixture = await BookingTestFixture.CreateAsync(UserRole.Admin);
        var otherUser = await fixture.AddUserAsync();
        var booking = await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Another user's booking",
            otherUser.Id);

        var handler = new CancelBooking.Handler(fixture.Context, fixture.CurrentUser);

        var result = await handler.Handle(new CancelBooking.Command(booking.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public async Task Handle_WhenBookingIsAlreadyCancelled_ReturnsCannotCancel()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var booking = await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Already cancelled booking");
        booking.Cancel();
        await fixture.Context.SaveChangesAsync();

        var handler = new CancelBooking.Handler(fixture.Context, fixture.CurrentUser);

        var result = await handler.Handle(new CancelBooking.Command(booking.Id));

        Assert.True(result.IsFailure);
        Assert.Equal(BookingErrors.CannotCancel, result.Error);
    }
}
