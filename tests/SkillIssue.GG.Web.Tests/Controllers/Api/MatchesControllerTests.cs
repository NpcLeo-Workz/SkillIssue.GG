using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Controllers.Api;
using SkillIssue.GG.Web.Models.Matches;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class MatchesControllerTests
{
    [Fact]
    public async Task GetByRiotMatchId_WhenMatchExists_ReturnsOk()
    {
        var match = CreateMatch();
        var service = new FakeMatchQueryService
        {
            MatchToReturn = match
        };

        var controller = new MatchesController(service);

        var result = await controller.GetByRiotMatchId(
            match.RiotMatchId,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PlayerMatchResponse>(okResult.Value);

        Assert.Equal(match.RiotMatchId, response.RiotMatchId);
        Assert.Equal(match.GameVersion, response.GameVersion);
        Assert.Equal(match.GameMode, response.GameMode);
        Assert.Equal(match.GameType, response.GameType);
        Assert.Equal(match.MapId, response.MapId);
        Assert.Equal(match.QueueId, response.QueueId);
        Assert.Equal(match.PlatformId, response.PlatformId);
        Assert.Equal(match.StartedAt, response.StartedAt);
        Assert.Equal(match.Duration, response.Duration);
    }

    [Fact]
    public async Task GetByRiotMatchId_ForwardsRiotMatchId()
    {
        var match = CreateMatch();
        var service = new FakeMatchQueryService
        {
            MatchToReturn = match
        };

        var controller = new MatchesController(service);

        await controller.GetByRiotMatchId(
            "EUW1_987654321",
            CancellationToken.None);

        Assert.Equal("EUW1_987654321", service.RiotMatchId);
    }

    [Fact]
    public async Task GetByRiotMatchId_MapsParticipants()
    {
        var match = CreateMatch();

        var participant = new MatchParticipant(
            matchId: match.Id,
            playerPuuid: "test-puuid",
            participantId: 1,
            teamId: 100,
            championId: 266,
            teamPosition: "TOP",
            kills: 10,
            deaths: 2,
            assists: 5,
            goldEarned: 12000,
            goldSpent: 11000,
            totalMinionsKilled: 180,
            neutralMinionsKilled: 10,
            visionScore: 25,
            wardsPlaced: 8,
            wardsKilled: 3,
            totalDamageDealtToChampions: 25000,
            totalDamageTaken: 18000,
            timePlayed: TimeSpan.FromMinutes(30),
            won: true);

        participant.AddItem(1001);
        participant.AddItem(2003);
        participant.AddRune(8005);
        participant.AddRune(9111);

        match.AddParticipant(participant);

        var service = new FakeMatchQueryService
        {
            MatchToReturn = match
        };

        var controller = new MatchesController(service);

        var result = await controller.GetByRiotMatchId(
            match.RiotMatchId,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PlayerMatchResponse>(okResult.Value);

        var responseParticipant = Assert.Single(response.Participants);

        Assert.Equal("test-puuid", responseParticipant.PlayerPuuid);
        Assert.Equal(266, responseParticipant.ChampionId);
        Assert.Equal(10, responseParticipant.Kills);
        Assert.Equal(2, responseParticipant.Deaths);
        Assert.Equal(5, responseParticipant.Assists);
        Assert.Equal(25000, responseParticipant.TotalDamageDealtToChampions);
        Assert.Equal(18000, responseParticipant.TotalDamageTaken);

        Assert.Equal(
            [1001, 2003],
            responseParticipant.ItemIds);

        Assert.Equal(
            [8005, 9111],
            responseParticipant.RuneIds);
    }

    [Fact]
    public async Task GetByRiotMatchId_WhenMatchDoesNotExist_ReturnsNotFound()
    {
        var service = new FakeMatchQueryService();
        var controller = new MatchesController(service);

        var result = await controller.GetByRiotMatchId(
            "EUW1_missing",
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task GetByRiotMatchId_WithInvalidMatchId_ReturnsBadRequest(
        string riotMatchId)
    {
        var service = new FakeMatchQueryService();
        var controller = new MatchesController(service);

        var result = await controller.GetByRiotMatchId(
            riotMatchId,
            CancellationToken.None);

        Assert.IsType<BadRequestResult>(result.Result);
        Assert.Equal(0, service.CallCount);
    }

    [Fact]
    public async Task GetByRiotMatchId_ForwardsCancellationToken()
    {
        var match = CreateMatch();
        var service = new FakeMatchQueryService
        {
            MatchToReturn = match
        };

        var controller = new MatchesController(service);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await controller.GetByRiotMatchId(
            match.RiotMatchId,
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            service.CancellationToken);
    }

    private static Match CreateMatch()
    {
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-30);

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

    private sealed class FakeMatchQueryService : IMatchQueryService
    {
        public Match? MatchToReturn { get; init; }

        public string? RiotMatchId { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public int CallCount { get; private set; }

        public Task<Match?> GetByRiotMatchIdAsync(
            string riotMatchId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            RiotMatchId = riotMatchId;
            CancellationToken = cancellationToken;

            return Task.FromResult(MatchToReturn);
        }
    }
}
