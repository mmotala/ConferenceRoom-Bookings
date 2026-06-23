using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class CreateBookingHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_CreatesActiveBooking()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var handler = CreateHandler(fixture);
        var command = new CreateBooking.Command(
            fixture.Room.Id,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Planning session");

        var result = await handler.Handle(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(command.RoomId, result.Value.RoomId);
        Assert.Equal(fixture.User.Id, result.Value.UserId);
        Assert.Equal("Planning session", result.Value.Purpose);
        Assert.Equal(BookingStatus.Active.ToString(), result.Value.Status);

        var booking = await fixture.Context.Bookings.SingleAsync();
        Assert.Equal(command.RoomId, booking.RoomId);
        Assert.Equal(fixture.User.Id, booking.UserId);
        Assert.True(booking.IsActive);
    }

    [Fact]
    public async Task Handle_WhenRoomAlreadyBooked_ReturnsRoomUnavailableConflict()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Existing booking");

        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new CreateBooking.Command(
            fixture.Room.Id,
            DateTime.UtcNow.AddHours(2.5),
            DateTime.UtcNow.AddHours(3.5),
            "Overlapping booking"));

        Assert.True(result.IsFailure);
        Assert.Equal(BookingErrors.RoomUnavailable, result.Error);
    }

    [Fact]
    public async Task Handle_WithInvalidTimeRange_ReturnsValidationError()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new CreateBooking.Command(
            fixture.Room.Id,
            DateTime.UtcNow.AddHours(3),
            DateTime.UtcNow.AddHours(2),
            "Invalid booking"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Contains("Booking end time must be after start time.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        fixture.CurrentUser.SetUser(null, null);
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new CreateBooking.Command(
            fixture.Room.Id,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            "Planning session"));

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.Unauthorized, result.Error);
    }

    private static CreateBooking.Handler CreateHandler(BookingTestFixture fixture)
    {
        return new CreateBooking.Handler(
            fixture.Context,
            fixture.CurrentUser,
            new BookingAvailabilityService(fixture.Context),
            new CreateBooking.Validator());
    }
}
