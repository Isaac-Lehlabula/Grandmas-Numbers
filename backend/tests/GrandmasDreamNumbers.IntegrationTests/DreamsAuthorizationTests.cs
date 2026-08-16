using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GrandmasDreamNumbers.IntegrationTests;

public class DreamsAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DreamsAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Analyse_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/dreams/analyse", new { dreamText = "a dream" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHistory_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/dreams");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AnalyseAgain_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/dreams/{Guid.NewGuid()}/analyse-again", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
