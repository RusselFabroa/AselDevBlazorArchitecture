using AselDevBlazorArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AselDevBlazorArchitecture.Migrations.PostgreSql;

public sealed class PostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=aseldev_design;Username=design;Password=design";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                connectionString,
                provider => provider.MigrationsAssembly(typeof(PostgreSqlDesignTimeDbContextFactory).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
