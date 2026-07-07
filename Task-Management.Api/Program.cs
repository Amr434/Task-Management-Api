using Task_Management.Infrastructure;
using Task_Management.Application;
using Task_Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger is enabled in all environments so the deployed API can be verified.
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

// Only force HTTPS locally. On Azure App Service, TLS is terminated at the
// platform edge and traffic is forwarded over HTTP, so redirecting here can
// cause redirect loops / 502s.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("CorsPolicy");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<TaskManagementDbContext>();
        await context.Database.MigrateAsync();
        await TaskManagementContextSeed.SeedAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

app.Run();
