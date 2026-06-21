var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddApplication()
    .AddInfrastructure();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwagger();
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

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    SeedData.Seed(dbContext);
}

public partial class Program;