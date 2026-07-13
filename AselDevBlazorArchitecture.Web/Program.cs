using AselDevBlazorArchitecture.Application;
using AselDevBlazorArchitecture.Infrastructure;
using AselDevBlazorArchitecture.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.OpenApi;
using MudBlazor.Services;
using Serilog;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net;
using Microsoft.Extensions.Hosting;
using AselDevBlazorArchitecture.Web.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AselDevBlazorArchitecture application...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──
    builder.Services.AddSerilog((services, config) =>
        config.ReadFrom.Configuration(builder.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // ── Blazor ──
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // ── MudBlazor ──
    builder.Services.AddMudServices();

    // ── Controllers ──
    builder.Services.AddControllers();
    builder.Services.AddScoped<SsoNavigationService>();

    // ── Swagger ──
    // ── Swagger ──
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "AselDev Enterprise API",
            Version = "v1",
            Description = "Enterprise API for AselDev services"
        });

        // ── JWT Authentication ──
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Enter JWT token like: Bearer {your token}",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,   // ✅ FIX (more correct than ApiKey)
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    });

    // ── Cascading Auth ──
    builder.Services.AddCascadingAuthenticationState();

    // ── Application + Infrastructure ──
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);


    // ════════════════════════════════════
    var app = builder.Build();
    // ════════════════════════════════════



    // ── Middleware pipeline ──
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AselDev API v1");
            c.RoutePrefix = "swagger";
        });
    }
    else
    {
        app.UseExceptionHandler("/system-error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();    // ← must be before UseAuthorization
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers();
    app.MapGet("/system-error", () => Results.Content(
        BuildSystemErrorPage(
            "Runtime Error",
            "An unhandled error occurred while processing the request. Check the application logs for the full exception details.",
            app.Environment.EnvironmentName,
            app.Environment.ContentRootPath),
        "text/html"));

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    var server = app.Services.GetRequiredService<IServer>();
    var addresses = server.Features.Get<IServerAddressesFeature>();

    if (addresses != null)
    {
        foreach (var address in addresses.Addresses)
        {
            Log.Information("Now listening on: {Address}", address);
        }
    }

    Log.Information("AselDevBlazorArchitecture started successfully.");
    app.Run();
}
catch (Exception ex)
{
    if (ex.GetType().Name == "HostAbortedException")
    {
        throw;
    }

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("========== FATAL ERROR ==========");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("=================================");
    Console.ResetColor();
    Console.ResetColor();

    Log.Fatal(ex, "Application terminated unexpectedly.");
    await RunStartupErrorPageAsync(args, ex);
}
finally
{
    Log.CloseAndFlush();
}

static async Task RunStartupErrorPageAsync(string[] args, Exception startupException)
{
    try
    {
        var fallbackBuilder = WebApplication.CreateBuilder(args);
        fallbackBuilder.WebHost.UseSetting(WebHostDefaults.PreventHostingStartupKey, "true");

        var fallbackApp = fallbackBuilder.Build();
        var html = BuildSystemErrorPage(
            "Startup Error",
            startupException.ToString(),
            fallbackApp.Environment.EnvironmentName,
            fallbackApp.Environment.ContentRootPath);

        fallbackApp.MapGet("/", () => Results.Content(html, "text/html"));
        fallbackApp.MapGet("/startup-error", () => Results.Content(html, "text/html"));

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Startup fallback UI is running. Open the configured application URL to view the error.");
        Console.ResetColor();

        await fallbackApp.RunAsync();
    }
    catch (Exception fallbackException)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("========== FALLBACK UI ERROR ==========");
        Console.WriteLine(fallbackException.ToString());
        Console.WriteLine("=======================================");
        Console.ResetColor();
    }
}

static string BuildSystemErrorPage(
    string title,
    string detail,
    string environmentName,
    string contentRootPath)
{
    var guidance = BuildStartupGuidance(detail);
    var encodedTitle = WebUtility.HtmlEncode(title);
    var encodedDetail = WebUtility.HtmlEncode(detail);
    var encodedEnvironment = WebUtility.HtmlEncode(environmentName);
    var encodedContentRoot = WebUtility.HtmlEncode(contentRootPath);
    var encodedTimestamp = WebUtility.HtmlEncode(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"));
    var encodedGuidanceTitle = WebUtility.HtmlEncode(guidance.Title);
    var encodedGuidanceSummary = WebUtility.HtmlEncode(guidance.Summary);
    var encodedGuidanceSteps = string.Join(
        "",
        guidance.Steps.Select(step => $"<li>{WebUtility.HtmlEncode(step)}</li>"));

    return $$"""
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>{{encodedTitle}} - AselDevBlazorArchitecture</title>
    <style>
        :root {
            --blue: #0046ad;
            --blue-dark: #002d72;
            --line: #cdd8eb;
            --text: #172033;
            --muted: #536174;
            --danger: #b42318;
            --danger-bg: #fff1f0;
        }

        * {
            box-sizing: border-box;
        }

        body {
            margin: 0;
            background: #f7faff;
            color: var(--text);
            font-family: Arial, Helvetica, sans-serif;
            line-height: 1.5;
        }

        main {
            display: grid;
            min-height: 100vh;
            place-items: center;
            padding: 24px;
        }

        .error-shell {
            width: min(100%, 980px);
            background: #ffffff;
            border: 1px solid var(--line);
            border-top: 6px solid var(--danger);
            border-radius: 2px;
            box-shadow: 0 18px 42px rgba(23, 32, 51, .08);
        }

        .header {
            border-bottom: 1px solid var(--line);
            padding: 22px 24px;
        }

        .eyebrow {
            color: var(--danger);
            font-size: 12px;
            font-weight: 800;
            letter-spacing: .08em;
            margin-bottom: 8px;
            text-transform: uppercase;
        }

        h1 {
            color: var(--blue-dark);
            font-size: clamp(28px, 4vw, 42px);
            line-height: 1.1;
            margin: 0;
        }

        .body {
            display: grid;
            gap: 16px;
            padding: 20px 24px 24px;
        }

        .meta {
            display: grid;
            gap: 10px;
            grid-template-columns: repeat(3, minmax(0, 1fr));
        }

        .meta div,
        pre {
            background: #f8fbff;
            border: 1px solid var(--line);
            border-radius: 2px;
        }

        .meta div {
            padding: 12px;
        }

        .meta strong {
            color: var(--blue-dark);
            display: block;
            font-size: 12px;
            margin-bottom: 4px;
            text-transform: uppercase;
        }

        .meta span {
            color: var(--muted);
            overflow-wrap: anywhere;
        }

        pre {
            color: var(--danger);
            max-height: 460px;
            margin: 0;
            overflow: auto;
            padding: 16px;
            white-space: pre-wrap;
        }

        .hint {
            background: var(--danger-bg);
            border-left: 5px solid var(--danger);
            color: #7a271a;
            padding: 12px 14px;
        }

        .guidance {
            background: #f8fbff;
            border: 1px solid var(--line);
            border-left: 5px solid var(--blue);
            padding: 14px 16px;
        }

        .guidance h2 {
            color: var(--blue-dark);
            font-size: 18px;
            margin: 0 0 6px;
        }

        .guidance p {
            color: var(--muted);
            margin: 0 0 10px;
        }

        .guidance ol {
            margin: 0;
            padding-left: 22px;
        }

        .guidance li {
            margin: 6px 0;
            overflow-wrap: anywhere;
        }

        @media (max-width: 760px) {
            .meta {
                grid-template-columns: 1fr;
            }

            main {
                padding: 12px;
            }
        }
    </style>
</head>
<body>
    <main>
        <section class="error-shell">
            <div class="header">
                <div class="eyebrow">Application Diagnostics</div>
                <h1>{{encodedTitle}}</h1>
            </div>
            <div class="body">
                <div class="hint">
                    The normal application could not complete startup. This fallback page is intentionally shown so setup issues are visible on new devices.
                </div>
                <div class="guidance">
                    <h2>{{encodedGuidanceTitle}}</h2>
                    <p>{{encodedGuidanceSummary}}</p>
                    <ol>
                        {{encodedGuidanceSteps}}
                    </ol>
                </div>
                <div class="meta">
                    <div><strong>Environment</strong><span>{{encodedEnvironment}}</span></div>
                    <div><strong>Content Root</strong><span>{{encodedContentRoot}}</span></div>
                    <div><strong>Timestamp</strong><span>{{encodedTimestamp}}</span></div>
                </div>
                <pre>{{encodedDetail}}</pre>
            </div>
        </section>
    </main>
</body>
</html>
""";
}

static StartupGuidance BuildStartupGuidance(string detail)
{
    var isDatabaseError =
        detail.Contains("DbContext", StringComparison.OrdinalIgnoreCase) ||
        detail.Contains("database", StringComparison.OrdinalIgnoreCase) ||
        detail.Contains("migration", StringComparison.OrdinalIgnoreCase) ||
        detail.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
        detail.Contains("Pomelo", StringComparison.OrdinalIgnoreCase) ||
        detail.Contains("connection string", StringComparison.OrdinalIgnoreCase) ||
        detail.Contains("Unknown column", StringComparison.OrdinalIgnoreCase) ||
        detail.Contains("SSL Authentication Error", StringComparison.OrdinalIgnoreCase);

    if (isDatabaseError)
    {
        return new StartupGuidance(
            "Database setup recommendation",
            "The startup error looks related to database connection, migration, or schema setup.",
            new[]
            {
                "Confirm Database:Enabled is true only when persistence is needed and provide Database:ConnectionString through secrets or environment variables.",
                "Apply pending migrations: dotnet ef database update --project AselDevBlazorArchitecture.Infrastructure --startup-project AselDevBlazorArchitecture.Web",
                "If the app is locking build files, stop the running app, then run: dotnet build AselDevBlazorArchitecture.Web\\AselDevBlazorArchitecture.Web.csproj",
                "If a column is missing, confirm the migration exists and appears in: dotnet ef migrations list --project AselDevBlazorArchitecture.Infrastructure --startup-project AselDevBlazorArchitecture.Web",
                "For local/dev MySQL SSL issues, add SslMode=None to the connection string only when your environment allows it.",
                "Run schema migration and administrator provisioning as explicit deployment steps; the web application does not perform them during startup."
            });
    }

    return new StartupGuidance(
        "General startup recommendation",
        "The startup error does not look specifically database-related. Check configuration, services, and environment setup first.",
        new[]
        {
            "Run a clean build: dotnet build AselDevBlazorArchitecture.Web\\AselDevBlazorArchitecture.Web.csproj --no-restore",
            "Check appsettings.json and appsettings.Development.json for invalid Database, JwtSettings, Sso, or Serilog sections.",
            "Confirm required secrets and environment variables are available on this machine.",
            "Check SystemLogs/aseldevlogs-.log for the full structured exception details.",
            "If the error started after adding a service, verify it is registered in Program.cs or the correct DependencyInjection class.",
            "If ports are already in use, close the old app process or change launchSettings.json."
        });
}

record StartupGuidance(string Title, string Summary, IReadOnlyList<string> Steps);
