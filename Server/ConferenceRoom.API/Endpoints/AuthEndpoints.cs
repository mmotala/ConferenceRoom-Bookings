public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapGet("/dummy-users", () =>
        {
            return Results.Ok(new[]
            {
                new
                {
                    UserId = SeedData.AdminUserId,
                    Name = "Admin User",
                    Email = "admin@demo.com",
                    Role = "Admin"
                },
                new
                {
                    UserId = SeedData.NormalUserId,
                    Name = "Normal User",
                    Email = "user@demo.com",
                    Role = "User"
                },
                new
                {
                    UserId = SeedData.SecondUserId,
                    Name = "Second User",
                    Email = "second@demo.com",
                    Role = "User"
                }
            });
        });

        group.MapPost("/dummy-login", async (
            DummyLogin.Command command,
            DummyLogin.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult();
        });

        return app;
    }
}