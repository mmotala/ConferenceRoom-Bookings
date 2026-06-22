public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin");

        group.MapGet("/users", async (
            GetUsers.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(cancellationToken);

            return result.ToHttpResult();
        });

        group.MapPost("/users", async (
            CreateUser.Command command,
            CreateUser.Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.ToHttpResult();
        });

        return app;
    }
}