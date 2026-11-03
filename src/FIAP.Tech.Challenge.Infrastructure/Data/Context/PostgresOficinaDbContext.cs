using Microsoft.EntityFrameworkCore;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Context;

public class PostgresOficinaDbContext(DbContextOptions<OficinaDbContext> options)
    : OficinaDbContext(options);
