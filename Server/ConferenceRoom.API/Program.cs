using ConferenceRoomBooking.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseCors("ClientApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

SeedDatabase(app);

app.MapAuthEndpoints();
app.MapRoomsEndpoints();
app.MapBookingsEndpoints();
app.MapAdminEndpoints();

app.Run();

static void SeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup");

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    SeedData.Seed(dbContext);

    logger.LogInformation("Database seeded successfully.");
    logger.LogInformation("Dummy admin: admin@demo.com");
    logger.LogInformation("Dummy user: user@demo.com");
    logger.LogInformation("Dummy second user: second@demo.com");
}

public partial class Program;