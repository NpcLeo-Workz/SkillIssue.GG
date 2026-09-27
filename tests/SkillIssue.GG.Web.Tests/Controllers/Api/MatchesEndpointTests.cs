using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Models.Matches;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class MatchesEndpointTests
{
    [Fact]
    public async Task Get_WhenMatchExists_ReturnsOkWithMatch()
    {
        var match = CreateMatch();

        var fakeService = new FakeMatchQueryService
        {
            MatchToReturn = match
        };

        await using var factory = CreateFactory(fakeService);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/matches/{match.RiotMatchId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body =
            await response.Content.ReadFromJsonAsync<PlayerMatchResponse>();

        Assert.NotNull(body);
        Assert.Equal(match.RiotMatchId, body.RiotMatchId);
        Assert.Equal(match.GameVersion, body.GameVersion);
        Assert.Equal(match.GameMode, body.GameMode);
        Assert.Equal(match.QueueId, body.QueueId);
        Assert.Equal(match.PlatformId, body.PlatformId);
    }

    [Fact]
    public async Task Get_ForwardsRiotMatchId()
    {
        var match = CreateMatch();

        var fakeService = new FakeMatchQueryService
        {
            MatchToReturn = match
        };

        await using var factory = CreateFactory(fakeService);
        using var client = factory.CreateClient();

        await client.GetAsync("/api/matches/EUW1_987654321");

        Assert.Equal(
            "EUW1_987654321",
            fakeService.RiotMatchId);
    }

    [Fact]
    public async Task Get_MapsParticipantItemsAndRunes()
    {
        var match = CreateMatch();

        var participant = new MatchParticipant(
            matchId: match.Id,
            playerPuuid: "pipeline-test-puuid",
            participantId: 1,
            teamId: 100,
            championId: 266,
            teamPosition: "TOP",
            kills: 8,
            deaths: 3,
            assists: 7,
            goldEarned: 13000,
            goldSpent: 12000,
            totalMinionsKilled: 190,
            neutralMinionsKilled: 5,
            visionScore: 30,
            wardsPlaced: 9,
            wardsKilled: 4,
            totalDamageDealtToChampions: 27000,
            totalDamageTaken: 20000,
            timePlayed: TimeSpan.FromMinutes(32),
            won: true);

        participant.AddItem(1001);
        participant.AddItem(3078);
        participant.AddRune(8005);
        participant.AddRune(9111);

        match.AddParticipant(participant);

        var fakeService = new FakeMatchQueryService
        {
            MatchToReturn = match
        };

        await using var factory = CreateFactory(fakeService);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/matches/{match.RiotMatchId}");

        var body =
            await response.Content.ReadFromJsonAsync<PlayerMatchResponse>();

        Assert.NotNull(body);

        var returnedParticipant =
            Assert.Single(body.Participants);

        Assert.Equal(
            "pipeline-test-puuid",
            returnedParticipant.PlayerPuuid);

        Assert.Equal(
            [1001, 3078],
            returnedParticipant.ItemIds);

        Assert.Equal(
            [8005, 9111],
            returnedParticipant.RuneIds);
    }

    [Fact]
    public async Task Get_WhenMatchDoesNotExist_ReturnsNotFound()
    {
        var fakeService = new FakeMatchQueryService();

        await using var factory = CreateFactory(fakeService);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/matches/EUW1_missing");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        IMatchQueryService service)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // These must be set before application startup.
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
                    services.RemoveAll<IMatchQueryService>();
                    services.AddSingleton(service);
                });
            });
    }

    private static Match CreateMatch()
    {
        var startedAt =
            DateTimeOffset.UtcNow.AddMinutes(-30);

        return new Match(
            riotMatchId: "EUW1_123456789",
            riotGameId: 123456789,
            dataVersion: "2",
            gameVersion: "16.15.1.1234",
            gameMode: "CLASSIC",
            gameType: "MATCHED_GAME",
            mapId: 11,
            queueId: 420,
            platformId: "EUW1",
            gameCreatedAt: startedAt.AddMinutes(-1),
            startedAt: startedAt,
            endedAt: startedAt.AddMinutes(30),
            duration: TimeSpan.FromMinutes(30),
            endOfGameResult: "GameComplete");
    }

    private sealed class FakeMatchQueryService
        : IMatchQueryService
    {
        public Match? MatchToReturn { get; init; }

        public string? RiotMatchId { get; private set; }

        public Task<Match?> GetByRiotMatchIdAsync(
            string riotMatchId,
            CancellationToken cancellationToken = default)
        {
            RiotMatchId = riotMatchId;

            return Task.FromResult(MatchToReturn);
        }
    }
}
