using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder
    .AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume();

var database = postgres.AddDatabase("oficina");
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);

builder
    .AddProject<Projects.FIAP_Tech_Challenge_API>("api")
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WithEnvironment("DbProvider", "PostgreSQL")
    .WithEnvironment("ConnectionStrings__DefaultConnection", database)
    .WithEnvironment("JWT_SECRET_KEY", jwtSecret)
    .WaitFor(database);

builder.Build().Run();
