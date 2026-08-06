using System.Net;
using ApiBr.Vagas.Api;
using ApiBr.Vagas.Webhook;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiBr.Vagas.Integration.Tests;

public class HealthEndpointsTests
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/version")]
    public async Task Webhook_ReturnsSuccess(string path)
    {
        await using var factory = new WebApplicationFactory<WebhookEntryPoint>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/v1/health")]
    [InlineData("/v1/health/ready")]
    [InlineData("/v1/version")]
    public async Task Api_ReturnsSuccess(string path)
    {
        await using var factory = new WebApplicationFactory<ApiEntryPoint>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
