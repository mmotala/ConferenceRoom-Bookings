using Microsoft.EntityFrameworkCore;

public sealed class BookingTestFixture : IAsyncDisposable
{
    private BookingTestFixture(
        ApplicationDbContext context,
        TestCurrentUserService currentUser,
        User user,
        Room room)
    {
        Context = context;
        CurrentUser = currentUser;
        User = user;
        Room = room;
    }

    public ApplicationDbContext Context { get; }
    public TestCurrentUserService CurrentUser { get; }
    public User User { get; }
    public Room Room { get; }

    public static async Task<BookingTestFixture> CreateAsync(UserRole role = UserRole.User)
    {
        var user = new User("Test User", "test@example.com", role);
        var currentUser = new TestCurrentUserService(user.Id, role);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options, currentUser);
        var room = new Room("Boardroom", 12, "First Floor");

        context.Users.Add(user);
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        return new BookingTestFixture(context, currentUser, user, room);
    }

    public async Task<Booking> AddBookingAsync(
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string purpose = "Test booking",
        Guid? userId = null,
        Guid? roomId = null)
    {
        var booking = new Booking(
            roomId ?? Room.Id,
            userId ?? User.Id,
            startTimeUtc,
            endTimeUtc,
            purpose);

        Context.Bookings.Add(booking);
        await Context.SaveChangesAsync();

        return booking;
    }

    public async Task<User> AddUserAsync(
        string name = "Other User",
        string email = "other@example.com",
        UserRole role = UserRole.User)
    {
        var user = new User(name, email, role);

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        return user;
    }

    public async Task<Room> AddRoomAsync(
        string name = "Focus Room",
        int capacity = 4,
        string location = "Second Floor")
    {
        var room = new Room(name, capacity, location);

        Context.Rooms.Add(room);
        await Context.SaveChangesAsync();

        return room;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
    }
}
