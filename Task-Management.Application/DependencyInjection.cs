using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace Task_Management.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // Sends a live notification for every task-history entry a command saved.
            cfg.AddOpenBehavior(typeof(Task_Management.Application.Features.Tasks.TaskChangeNotificationBehavior<,>));
        });
        
        return services;
    }
}
