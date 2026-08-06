using ApiBr.Vagas.Infrastructure.DependencyInjection;
using ApiBr.Vagas.Infrastructure.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseApiVersionHeader();
app.MapVagasHealthEndpoints("/v1");

await app.RunAsync();
