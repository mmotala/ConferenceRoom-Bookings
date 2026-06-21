using ConferenceRoomBooking.Application.Features.Bookings;
using ConferenceRoomBooking.Application.Features.Rooms;
using ConferenceRoomBooking.Application.Features.Users;
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

        return services;
    }
}