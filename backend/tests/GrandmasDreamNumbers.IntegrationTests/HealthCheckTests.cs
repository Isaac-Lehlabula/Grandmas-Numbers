using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GrandmasDreamNumbers.IntegrationTests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsAResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }
}
