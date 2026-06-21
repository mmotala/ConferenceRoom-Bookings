

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<CreateRoom.Handler>();
        services.AddScoped<GetRooms.Handler>();
        services.AddScoped<DeleteRoom.Handler>();

        services.AddScoped<CreateBooking.Handler>();
        services.AddScoped<GetBookings.Handler>();
        services.AddScoped<UpdateBooking.Handler>();
        services.AddScoped<CancelBooking.Handler>();

        services.AddScoped<GetUsers.Handler>();

        return services;
    }
}