using AselDevBlazorArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AselDevBlazorArchitecture.Migrations.MySql;

public sealed class MySqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? "server=localhost;port=3306;database=aseldev_design;uid=design;pwd=design;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 0)),
                provider => provider.MigrationsAssembly(typeof(MySqlDesignTimeDbContextFactory).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
