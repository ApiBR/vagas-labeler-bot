using ApiBr.Vagas.Infrastructure.DependencyInjection;
using ApiBr.Vagas.Infrastructure.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseApiVersionHeader();
app.MapVagasHealthEndpoints();

await app.RunAsync();
