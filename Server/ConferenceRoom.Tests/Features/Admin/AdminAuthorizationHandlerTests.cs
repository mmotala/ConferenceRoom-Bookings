using Xunit;

public sealed class AdminAuthorizationHandlerTests
{
    [Fact]
    public async Task CreateRoom_WhenCurrentUserIsNotAdmin_ReturnsForbidden()
    {
        await using var fixture = await BookingTestFixture.CreateAsync();
        var handler = new CreateRoom.Handler(
            fixture.Context,
            fixture.CurrentUser,
            new CreateRoom.Validator());

        var result = await handler.Handle(new CreateRoom.Command(
            "Workshop Room",
            8,
            "Third Floor"));

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.Forbidden, result.Error);
    }

    [Fact]
    public async Task CreateRoom_WhenCurrentUserIsAdmin_CreatesRoom()
    {
        await using var fixture = await BookingTestFixture.CreateAsync(UserRole.Admin);
        var handler = new CreateRoom.Handler(
            fixture.Context,
            fixture.CurrentUser,
            new CreateRoom.Validator());

        var result = await handler.Handle(new CreateRoom.Command(
            "Workshop Room",
            8,
            "Third Floor"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Workshop Room", result.Value.Name);
    }

    [Fact]
    public async Task CreateUser_WhenEmailAlreadyExists_ReturnsConflict()
    {
        await using var fixture = await BookingTestFixture.CreateAsync(UserRole.Admin);
        var handler = new CreateUser.Handler(
            fixture.Context,
            fixture.CurrentUser,
            new CreateUser.Validator());

        var result = await handler.Handle(new CreateUser.Command(
            "Duplicate User",
            fixture.User.Email,
            UserRole.User));

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.EmailAlreadyExists, result.Error);
    }
}
