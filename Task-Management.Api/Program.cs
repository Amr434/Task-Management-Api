using System.Security.Claims;
using System.Text;
using Task_Management.Infrastructure;
using Task_Management.Application;
using Task_Management.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Task_Management.Api.Hubs;
using Task_Management.Api.Services;
using Task_Management.Application.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste the access token from POST /api/Auth/login (without the 'Bearer ' prefix).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// JWT bearer auth: tokens are validated locally against the signing key,
// no external identity provider — works fully offline on the LAN.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManagement",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TaskManagement",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            // Tokens live up to 60 minutes, so re-check the user on every request:
            // a deactivated user is rejected right away (401 -> the apps try to
            // refresh, that fails too, and they sign the user out), and a role
            // change by the Super Admin applies immediately instead of at next login.
            OnTokenValidated = async context =>
            {
                var idValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(idValue, out var userId))
                {
                    context.Fail("Invalid token.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<TaskManagementDbContext>();
                var user = await db.Users.AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.IsActive, u.Role })
                    .FirstOrDefaultAsync();

                if (user is null || !user.IsActive)
                {
                    context.Fail("This account is deactivated.");
                    return;
                }

                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    foreach (var roleClaim in identity.FindAll(ClaimTypes.Role).ToList())
                    {
                        identity.RemoveClaim(roleClaim);
                    }
                    identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
                }
            },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/invitation")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddSignalR();
// Notifications go out live (SignalR) and to the saved list + email (see CompositeNotifiers).
builder.Services.AddScoped<SignalRInvitationNotifier>();
builder.Services.AddScoped<SignalRCommentNotifier>();
builder.Services.AddScoped<SignalRTaskNotifier>();
builder.Services.AddScoped<IInvitationNotifier, CompositeInvitationNotifier>();
builder.Services.AddScoped<ICommentNotifier, CompositeCommentNotifier>();
builder.Services.AddScoped<ITaskNotifier, CompositeTaskNotifier>();
builder.Services.AddSingleton<INotificationPusher, SignalRNotificationPusher>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger is enabled in all environments so the deployed API can be verified.
// app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

// Only force HTTPS locally. On Azure App Service, TLS is terminated at the
// platform edge and traffic is forwarded over HTTP, so redirecting here can
// cause redirect loops / 502s.
//
// Locally, only redirect requests to localhost (the browser). A phone on the
// Wi-Fi calls http://<this PC's IP>:5013 and can't follow a redirect to the
// dev HTTPS port, which only listens on localhost.
if (app.Environment.IsDevelopment())
{
    app.UseWhen(
        ctx => ctx.Request.Host.Host is "localhost" or "127.0.0.1" or "[::1]",
        branch => branch.UseHttpsRedirection());
}

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<InvitationHub>("/hubs/invitation");

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
