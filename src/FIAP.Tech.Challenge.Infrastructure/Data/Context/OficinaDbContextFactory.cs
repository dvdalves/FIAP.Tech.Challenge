using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Context;

public class OficinaDbContextFactory : IDesignTimeDbContextFactory<PostgresOficinaDbContext>
{
    public PostgresOficinaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OficinaDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=oficina;Username=postgres;Password=design-time",
            options => options.MigrationsAssembly(typeof(OficinaDbContext).Assembly.FullName));

        return new PostgresOficinaDbContext(optionsBuilder.Options);
    }
}
