public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapGet("/dummy-logins", () =>
        {
            return Results.Ok(new
            {
                Message = "Use these headers to simulate login.",
                Headers = new[]
                {
                    new
                    {
                        Description = "Admin user",
                        UserId = SeedData.AdminUserId,
                        Role = "Admin"
                    },
                    new
                    {
                        Description = "Normal user",
                        UserId = SeedData.NormalUserId,
                        Role = "User"
                    },
                    new
                    {
                        Description = "Second normal user",
                        UserId = SeedData.SecondUserId,
                        Role = "User"
                    }
                }
            });
        });

        return app;
    }
}