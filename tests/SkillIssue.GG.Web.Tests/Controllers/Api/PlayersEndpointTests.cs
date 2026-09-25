using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Models.Players;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class PlayersEndpointTests
{
    [Fact]
    public async Task Get_WithPersistedPlayer_ReturnsOkAndMapsResponse()
    {
        var player = new Player(
            "player-puuid",
            "Example Player",
            "EUW");

        var fake = new FakePlayerQueryService
        {
            PlayerToReturn = player
        };

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/player-puuid");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<PlayerResponse>();

        Assert.NotNull(body);
        Assert.Equal(player.Id, body.Id);
        Assert.Equal("player-puuid", body.Puuid);
        Assert.Equal("Example Player", body.Name);
        Assert.Equal("EUW", body.Region);

        Assert.Equal("player-puuid", fake.Puuid);
        Assert.Equal(1, fake.CallCount);
    }

    [Fact]
    public async Task Get_WhenPlayerDoesNotExist_ReturnsNotFound()
    {
        var fake = new FakePlayerQueryService();

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/missing-puuid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.Equal("missing-puuid", fake.Puuid);
        Assert.Equal(1, fake.CallCount);
    }

    [Fact]
    public async Task Get_WithWhitespacePuuid_ReturnsBadRequest()
    {
        var fake = new FakePlayerQueryService();

        await using var factory = CreateFactory(fake);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/players/%20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, fake.CallCount);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        FakePlayerQueryService fake)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
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
                    services.RemoveAll<IPlayerQueryService>();

                    services.AddSingleton<IPlayerQueryService>(fake);
                });
            });
    }

    private sealed class FakePlayerQueryService : IPlayerQueryService
    {
        public Player? PlayerToReturn { get; init; }

        public string? Puuid { get; private set; }

        public int CallCount { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<Player?> GetByPuuidAsync(
            string puuid,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Puuid = puuid;
            CancellationToken = cancellationToken;

            return Task.FromResult(PlayerToReturn);
        }
    }
}
