public static class SeedData
{
    public static readonly Guid AdminUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid NormalUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid SecondUserId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid BoardRoomId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid SmallRoomId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static void Seed(AppDbContext context)
    {
        if (!context.Users.Any())
        {
            var admin = new User(
                "Admin User",
                "admin@demo.com",
                UserRole.Admin);

            SetId(admin, AdminUserId);

            var user = new User(
                "Normal User",
                "user@demo.com",
                UserRole.User);

            SetId(user, NormalUserId);

            var secondUser = new User(
                "Second User",
                "second@demo.com",
                UserRole.User);

            SetId(secondUser, SecondUserId);

            context.Users.AddRange(admin, user, secondUser);
        }

        if (!context.Rooms.Any())
        {
            var boardRoom = new Room(
                "Board Room",
                12,
                "First Floor");

            SetId(boardRoom, BoardRoomId);

            var smallRoom = new Room(
                "Small Meeting Room",
                4,
                "Ground Floor");

            SetId(smallRoom, SmallRoomId);

            context.Rooms.AddRange(boardRoom, smallRoom);
        }

        context.SaveChanges();
    }

    private static void SetId<T>(T entity, Guid id)
    {
        var property = typeof(T).GetProperty("Id");

        property?.SetValue(entity, id);
    }
}