using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Controllers.Api;
using SkillIssue.GG.Web.Models.Matches;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class PlayerMatchesControllerTests
{
    [Fact]
    public async Task GetAsync_ForwardsRequestAndReturnsMappedMatch()
    {
        var match = CreateMatch();
        var service = new FakePlayerMatchHistoryService([match]);

        var controller = CreateController(service);

        var result = await controller.GetAsync(
            "player-puuid",
            skip: 10,
            take: 25);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var response = Assert.IsType<
            IReadOnlyList<PlayerMatchResponse>>(okResult.Value, exactMatch: false);

        var returnedMatch = Assert.Single(response);

        Assert.Equal("player-puuid", service.Puuid);
        Assert.Equal(10, service.Skip);
        Assert.Equal(25, service.Take);

        Assert.Equal(match.RiotMatchId, returnedMatch.RiotMatchId);
        Assert.Equal(match.GameVersion, returnedMatch.GameVersion);
        Assert.Equal(match.GameMode, returnedMatch.GameMode);
        Assert.Equal(match.GameType, returnedMatch.GameType);
        Assert.Equal(match.MapId, returnedMatch.MapId);
        Assert.Equal(match.QueueId, returnedMatch.QueueId);
        Assert.Equal(match.PlatformId, returnedMatch.PlatformId);
        Assert.Equal(match.GameCreatedAt, returnedMatch.GameCreatedAt);
        Assert.Equal(match.StartedAt, returnedMatch.StartedAt);
        Assert.Equal(match.EndedAt, returnedMatch.EndedAt);
        Assert.Equal(match.Duration, returnedMatch.Duration);
        Assert.Equal(match.EndOfGameResult, returnedMatch.EndOfGameResult);

        var participant = Assert.Single(returnedMatch.Participants);

        Assert.Equal("player-puuid", participant.PlayerPuuid);
        Assert.Equal(1, participant.ParticipantId);
        Assert.Equal(100, participant.TeamId);
        Assert.Equal(266, participant.ChampionId);
        Assert.Equal("TOP", participant.TeamPosition);
        Assert.Equal(10, participant.Kills);
        Assert.Equal(2, participant.Deaths);
        Assert.Equal(5, participant.Assists);
        Assert.Equal(12000, participant.GoldEarned);
        Assert.Equal(11000, participant.GoldSpent);
        Assert.Equal(180, participant.TotalMinionsKilled);
        Assert.Equal(10, participant.NeutralMinionsKilled);
        Assert.Equal(25, participant.VisionScore);
        Assert.Equal(8, participant.WardsPlaced);
        Assert.Equal(3, participant.WardsKilled);
        Assert.Equal(25000, participant.TotalDamageDealtToChampions);
        Assert.Equal(18000, participant.TotalDamageTaken);
        Assert.Equal(TimeSpan.FromMinutes(30), participant.TimePlayed);
        Assert.True(participant.Won);

        Assert.Equal(
            [1001, 2003],
            participant.ItemIds);

        Assert.Equal(
            [8005, 9111],
            participant.RuneIds);
    }

    [Fact]
    public async Task GetAsync_WhenHistoryIsEmpty_ReturnsOkWithEmptyCollection()
    {
        var service = new FakePlayerMatchHistoryService([]);
        var controller = CreateController(service);

        var result = await controller.GetAsync(
            "player-puuid",
            skip: 0,
            take: 20);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var response = Assert.IsType<
            IReadOnlyList<PlayerMatchResponse>>(okResult.Value, exactMatch: false);

        Assert.Empty(response);
    }

    [Fact]
    public async Task GetAsync_PropagatesRequestCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var service = new FakePlayerMatchHistoryService([]);
        var controller = CreateController(
            service,
            cancellationTokenSource.Token);

        await controller.GetAsync(
            "player-puuid",
            skip: 0,
            take: 20);

        Assert.Equal(
            cancellationTokenSource.Token,
            service.CancellationToken);
    }

    private static PlayerMatchesController CreateController(
        FakePlayerMatchHistoryService service,
        CancellationToken cancellationToken = default)
    {
        return new PlayerMatchesController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestAborted = cancellationToken
                }
            }
        };
    }

    private static Match CreateMatch()
    {
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-30);

        var match = new Match(
            riotMatchId: "EUW1_123456",
            riotGameId: 123456,
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

        var participant = new MatchParticipant(
            matchId: match.Id,
            playerPuuid: "player-puuid",
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

        return match;
    }

    private sealed class FakePlayerMatchHistoryService(
        IReadOnlyList<Match> matches)
                : IPlayerMatchHistoryService
    {
        private readonly IReadOnlyList<Match> _matches = matches;

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
            Puuid = puuid;
            Skip = skip;
            Take = take;
            CancellationToken = cancellationToken;

            return Task.FromResult(_matches);
        }
    }
}
