using ApiBr.Vagas.Application.Ports;
using ApiBr.Vagas.Infrastructure.DependencyInjection;
using ApiBr.Vagas.Infrastructure.Endpoints;
using ApiBr.Vagas.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddHostedService<VagasWorker>();

var app = builder.Build();

app.UseApiVersionHeader();
app.MapVagasHealthEndpoints();

var versionProvider = app.Services.GetRequiredService<IVersionProvider>();
app.Logger.LogInformation("API BR Vagas Labeler Worker starting, version {Version}", versionProvider.InformationalVersion);

await app.RunAsync();
