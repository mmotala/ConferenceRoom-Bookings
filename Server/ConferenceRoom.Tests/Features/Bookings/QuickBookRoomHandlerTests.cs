using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class QuickBookRoomHandlerTests
{
    [Fact]
    public async Task Handle_SelectsSmallestAvailableRoomThatFitsCapacity()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var smallRoom = await fixture.AddRoomAsync("Small Room", 4);
        var largeRoom = await fixture.AddRoomAsync("Large Room", 20);
        await fixture.AddBookingAsync(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            roomId: smallRoom.Id);

        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new QuickBookRoom.Command(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            5,
            "Team sync"));

        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.Room.Id, result.Value.RoomId);

        var booking = await fixture.Context.Bookings
            .SingleAsync(booking => booking.Id == result.Value.BookingId);
        Assert.Equal(fixture.Room.Id, booking.RoomId);
        Assert.NotEqual(largeRoom.Id, booking.RoomId);
    }

    [Fact]
    public async Task Handle_WhenNoRoomCanFitCapacity_ReturnsNoSuitableRoomAvailable()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new QuickBookRoom.Command(
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(3),
            500,
            "Large event"));

        Assert.True(result.IsFailure);
        Assert.Equal(BookingErrors.NoSuitableRoomAvailable, result.Error);
    }

    private static QuickBookRoom.Handler CreateHandler(BookingTestFixture fixture)
    {
        return new QuickBookRoom.Handler(
            fixture.Context,
            fixture.CurrentUser,
            new QuickBookRoom.Validator());
    }
}
