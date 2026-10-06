using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Domain.Interfaces;
using Task_Management.Infrastructure.Data;
using Task_Management.Infrastructure.Email;
using Task_Management.Infrastructure.Services;

namespace Task_Management.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskManagementDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true"));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Email notifications: queued, then sent by a background service.
        // The notifiers are combined with the SignalR ones in the Api layer.
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddSingleton<EmailQueue>();
        services.AddSingleton<IEmailQueue>(sp => sp.GetRequiredService<EmailQueue>());
        services.AddHostedService<EmailBackgroundSender>();
        services.AddScoped<EmailTaskNotifier>();
        services.AddScoped<EmailInvitationNotifier>();
        services.AddScoped<EmailCommentNotifier>();
        services.AddScoped<IAccountNotifier, EmailAccountNotifier>();

        return services;
    }
}
