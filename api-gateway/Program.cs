using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("yarp.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("GatewayPolicy", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // 1. Register authentication (JWT example — swap scheme if you use cookies/OAuth)
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer    = builder.Configuration["Jwt:Issuer"],
                ValidAudience  = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    // 2. Register authorization + define the two policies YARP routes reference
    builder.Services.AddAuthorization(options =>
    {
        // Any valid, authenticated user
        options.AddPolicy("authenticated", policy =>
            policy.RequireAuthenticatedUser());

        // Authenticated AND must carry the "admin" role
        options.AddPolicy("admin-only", policy =>
            policy.RequireAuthenticatedUser()
                  .RequireRole("admin"));
    });

    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseCors("GatewayPolicy");

    // 3. Middleware order matters — auth must come before the proxy
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapReverseProxy();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway failed to start");
}
finally
{
    Log.CloseAndFlush();
}