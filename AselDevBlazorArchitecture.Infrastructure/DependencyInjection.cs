using AselDevBlazorArchitecture.Application.Common.Interfaces.AuthServices;

using AselDevBlazorArchitecture.Application.Features.Auth;
using AselDevBlazorArchitecture.Application.Features.Users;

using AselDevBlazorArchitecture.Domain.Entities;

using AselDevBlazorArchitecture.Infrastructure.Auth;
using AselDevBlazorArchitecture.Infrastructure.Data;

using AselDevBlazorArchitecture.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace AselDevBlazorArchitecture.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. Serilog ──
        Log.Logger = LoggingConfiguration.CreateLogger(configuration);
        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        // ── 2. Optional persistence + Identity ──
        var database = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.Configure<DatabaseOptions>(
            configuration.GetSection(DatabaseOptions.SectionName));

        if (database.Enabled)
        {
            AddDatabaseAndIdentity(services, database);
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserAdministrationService, IdentityUserAdministrationService>();
            services.AddScoped<IBrowserSignInService, IdentityBrowserSignInService>();
        }
        else
        {
            Log.Warning(
                "Database capability is disabled. The application will start without persistence or local Identity.");
            services.AddScoped<IAuthService, UnavailableAuthService>();
            services.AddScoped<IUserAdministrationService, UnavailableUserAdministrationService>();
            services.AddScoped<IBrowserSignInService, UnavailableBrowserSignInService>();
        }

        // ── 4. JWT ──
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings not configured.");

        var key = database.Enabled
            ? GetConfiguredJwtKey(jwtSettings)
            : RandomNumberGenerator.GetBytes(32);

        var browserScheme = database.Enabled
            ? IdentityConstants.ApplicationScheme
            : JwtBearerDefaults.AuthenticationScheme;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = browserScheme;
            options.DefaultChallengeScheme = browserScheme;
            options.DefaultScheme = browserScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });



        services.AddAuthorization();


        services.AddScoped<IUserSessionService, BrowserCookieUserSessionService>();

       
        services.AddScoped<IAuthGuardService, AuthGuardService>();

     

        return services;
    }

    private static void AddDatabaseAndIdentity(
        IServiceCollection services,
        DatabaseOptions database)
    {
        if (string.IsNullOrWhiteSpace(database.ConnectionString))
        {
            throw new InvalidOperationException(
                "Database:ConnectionString is required when Database:Enabled is true.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (database.Provider.Trim().ToLowerInvariant())
            {
                case "mysql":
                case "mariadb":
                    options.UseMySql(
                        database.ConnectionString,
                        new MySqlServerVersion(new Version(8, 0, 0)),
                        provider =>
                        {
                            provider.EnableRetryOnFailure(3);
                            provider.MigrationsAssembly("AselDevBlazorArchitecture.Migrations.MySql");
                        });
                    break;

                case "postgresql":
                case "postgres":
                case "npgsql":
                    options.UseNpgsql(
                        database.ConnectionString,
                        provider =>
                        {
                            provider.EnableRetryOnFailure(3);
                            provider.MigrationsAssembly("AselDevBlazorArchitecture.Migrations.PostgreSql");
                        });
                    break;

                default:
                    throw new NotSupportedException(
                        $"Database provider '{database.Provider}' is not supported. Supported providers: mysql, postgresql.");
            }
        });

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "AselDev.Session";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/unauthorized";
        });

        Log.Information("Database capability enabled — Provider: {Provider}", database.Provider);
    }

    private static byte[] GetConfiguredJwtKey(JwtSettings jwtSettings)
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:Key must be supplied through secrets or environment variables and contain at least 32 characters when the database is enabled.");
        }

        return Encoding.UTF8.GetBytes(jwtSettings.Key);
    }
}
