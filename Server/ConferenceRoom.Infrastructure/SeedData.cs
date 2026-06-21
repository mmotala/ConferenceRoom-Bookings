public static class SeedData
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new User { Id = Guid.NewGuid(), Username = "admin", Role = UserRole.Admin };
        var john = new User { Id = Guid.NewGuid(), Username = "john", Role = UserRole.User };
        var sarah = new User { Id = Guid.NewGuid(), Username = "sarah", Role = UserRole.User };

        db.Users.AddRange(admin, john, sarah);

        db.Rooms.AddRange(
            new Room { Id = Guid.NewGuid(), Name = "Board Room", Capacity = 4, Description = "Exec room" },
            new Room { Id = Guid.NewGuid(), Name = "Ocean View", Capacity = 8, Description = "Collab room" },
            new Room { Id = Guid.NewGuid(), Name = "Titans", Capacity = 8, Description = "Teams room" },
            new Room { Id = Guid.NewGuid(), Name = "Conference Hall", Capacity = 30, Description = "Large events" }
        );

        db.SaveChanges();
    }
}