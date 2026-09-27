using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Models.Matches;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class PlayerMatchesEndpointTests
{
    [Fact]
    public async Task Get_WithValidRequest_ReturnsOkAndForwardsPagination()
    {
        var fake = new FakePlayerMatchHistoryService([]);

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/player-puuid/matches?skip=10&take=25");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("player-puuid", fake.Puuid);
        Assert.Equal(10, fake.Skip);
        Assert.Equal(25, fake.Take);
    }

    [Fact]
    public async Task Get_WithoutPagination_UsesDefaults()
    {
        var fake = new FakePlayerMatchHistoryService([]);

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/player-puuid/matches");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, fake.Skip);
        Assert.Equal(20, fake.Take);
    }

    [Fact]
    public async Task Get_WhenHistoryIsEmpty_ReturnsOkWithEmptyCollection()
    {
        var fake = new FakePlayerMatchHistoryService([]);

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/player-puuid/matches");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<
                PlayerMatchResponse[]>();

        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task Get_WithWhitespacePuuid_ReturnsBadRequest()
    {
        var fake = new FakePlayerMatchHistoryService([]);

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/%20/matches");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, fake.CallCount);
    }

    [Fact]
    public async Task Get_WithNegativeSkip_ReturnsBadRequest()
    {
        var fake = new FakePlayerMatchHistoryService([]);

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/player-puuid/matches?skip=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, fake.CallCount);
    }

    [Fact]
    public async Task Get_WithZeroTake_ReturnsBadRequest()
    {
        var fake = new FakePlayerMatchHistoryService([]);

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/player-puuid/matches?take=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, fake.CallCount);
    }

    [Fact]
    public async Task Get_WithTakeAboveMaximum_ReturnsBadRequest()
    {
        var fake = new FakePlayerMatchHistoryService([]);

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/player-puuid/matches?take=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, fake.CallCount);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        FakePlayerMatchHistoryService fake)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // These satisfy Infrastructure's fail-fast startup
                // configuration. No real database or Riot call is made.
                builder.UseSetting(
                    "ConnectionStrings:PostgreSQL",
                    "Host=localhost;Database=skillissuegg_test;Username=test;Password=test");

                builder.UseSetting(
                    "RiotApi:ApiKey",
                    "test-api-key");

                builder.UseSetting(
                    "RiotApi:PlatformRoute",
                    "euw1");

                builder.UseSetting(
                    "RiotApi:RegionalRoute",
                    "europe");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IPlayerMatchHistoryService>();

                    services.AddSingleton<
                        IPlayerMatchHistoryService>(fake);
                });
            });
    }

    private sealed class FakePlayerMatchHistoryService(
        IReadOnlyList<Match> matches)
                : IPlayerMatchHistoryService
    {
        private readonly IReadOnlyList<Match> _matches = matches;

        public int CallCount { get; private set; }

        public string? Puuid { get; private set; }

        public int? Skip { get; private set; }

        public int? Take { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<IReadOnlyList<Match>> GetAsync(
            string puuid,
            int skip = 0,
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Puuid = puuid;
            Skip = skip;
            Take = take;
            CancellationToken = cancellationToken;

            return Task.FromResult(_matches);
        }
    }
}
