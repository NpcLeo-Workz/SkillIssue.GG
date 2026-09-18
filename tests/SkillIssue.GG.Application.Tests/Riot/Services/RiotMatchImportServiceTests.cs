using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Application.Riot.Services;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Tests.Riot.Services;

public sealed class RiotMatchImportServiceTests
{
    [Fact]
    public async Task ImportAsync_ImportsMatch_WhenMatchDoesNotExist()
    {
        var riotMatch = CreateRiotMatchDetails();
        var riotMatchService = new FakeRiotMatchService(riotMatch);
        var repository = new FakeMatchRepository(exists: false);

        var service = new RiotMatchImportService(
            riotMatchService,
            repository);

        var result = await service.ImportAsync(riotMatch.RiotMatchId);

        Assert.True(result.Imported);
        Assert.Equal(riotMatch.RiotMatchId, result.RiotMatchId);
        Assert.NotNull(result.MatchId);

        Assert.Equal(1, repository.ExistsCallCount);
        Assert.Equal(1, riotMatchService.GetMatchCallCount);
        Assert.Equal(1, repository.AddCallCount);

        Assert.NotNull(repository.AddedMatch);
        Assert.Equal(result.MatchId, repository.AddedMatch.Id);
        Assert.Equal(riotMatch.RiotMatchId, repository.AddedMatch.RiotMatchId);
    }

    [Fact]
    public async Task ImportAsync_SkipsImport_WhenMatchAlreadyExists()
    {
        var riotMatchService = new FakeRiotMatchService(
            CreateRiotMatchDetails());

        var repository = new FakeMatchRepository(exists: true);

        var service = new RiotMatchImportService(
            riotMatchService,
            repository);

        var result = await service.ImportAsync("EUW1_1234567890");

        Assert.False(result.Imported);
        Assert.Equal("EUW1_1234567890", result.RiotMatchId);
        Assert.Null(result.MatchId);

        Assert.Equal(1, repository.ExistsCallCount);
        Assert.Equal(0, riotMatchService.GetMatchCallCount);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ImportAsync_Throws_WhenMatchIdIsInvalid(
        string? matchId)
    {
        var riotMatchService = new FakeRiotMatchService(
            CreateRiotMatchDetails());

        var repository = new FakeMatchRepository(exists: false);

        var service = new RiotMatchImportService(
            riotMatchService,
            repository);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.ImportAsync(matchId!));

        Assert.Equal(0, repository.ExistsCallCount);
        Assert.Equal(0, riotMatchService.GetMatchCallCount);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task ImportAsync_PropagatesRiotServiceFailure()
    {
        var expected = new InvalidOperationException("Riot failure");

        var riotMatchService = new FakeRiotMatchService(expected);
        var repository = new FakeMatchRepository(exists: false);

        var service = new RiotMatchImportService(
            riotMatchService,
            repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ImportAsync("EUW1_1234567890"));

        Assert.Same(expected, exception);

        Assert.Equal(1, repository.ExistsCallCount);
        Assert.Equal(1, riotMatchService.GetMatchCallCount);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task ImportAsync_PropagatesRepositoryFailure()
    {
        var riotMatch = CreateRiotMatchDetails();
        var riotMatchService = new FakeRiotMatchService(riotMatch);

        var expected = new InvalidOperationException("Persistence failure");

        var repository = new FakeMatchRepository(
            exists: false,
            addException: expected);

        var service = new RiotMatchImportService(
            riotMatchService,
            repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ImportAsync(riotMatch.RiotMatchId));

        Assert.Same(expected, exception);

        Assert.Equal(1, repository.ExistsCallCount);
        Assert.Equal(1, riotMatchService.GetMatchCallCount);
        Assert.Equal(1, repository.AddCallCount);
    }

    [Fact]
    public async Task ImportAsync_PassesCancellationTokenToDependencies()
    {
        var riotMatch = CreateRiotMatchDetails();
        var riotMatchService = new FakeRiotMatchService(riotMatch);
        var repository = new FakeMatchRepository(exists: false);

        var service = new RiotMatchImportService(
            riotMatchService,
            repository);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var token = cancellationTokenSource.Token;

        await service.ImportAsync(
            riotMatch.RiotMatchId,
            token);

        Assert.Equal(token, repository.ExistsCancellationToken);
        Assert.Equal(token, riotMatchService.GetMatchCancellationToken);
        Assert.Equal(token, repository.AddCancellationToken);
    }

    private static RiotMatchDetails CreateRiotMatchDetails()
    {
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-25);

        return new RiotMatchDetails(
            DataVersion: "2",
            RiotMatchId: "EUW1_1234567890",
            RiotGameId: 1234567890,
            GameVersion: "16.15.123.4567",
            GameMode: "CLASSIC",
            GameType: "MATCHED_GAME",
            MapId: 11,
            QueueId: 420,
            PlatformId: "EUW1",
            GameCreatedAt: startedAt.AddMinutes(-1),
            StartedAt: startedAt,
            EndedAt: startedAt.AddMinutes(25),
            Duration: TimeSpan.FromMinutes(25),
            EndOfGameResult: "GameComplete",
            Participants:
            [
                new RiotMatchParticipant(
                    Puuid: "test-puuid",
                    ParticipantId: 1,
                    TeamId: 100,
                    ChampionId: 266,
                    ChampionName: "Aatrox",
                    TeamPosition: "TOP",
                    Kills: 5,
                    Deaths: 2,
                    Assists: 7,
                    GoldEarned: 12000,
                    GoldSpent: 11500,
                    TotalMinionsKilled: 180,
                    NeutralMinionsKilled: 12,
                    VisionScore: 25,
                    WardsPlaced: 8,
                    WardsKilled: 3,
                    TotalDamageDealt: 30000,
                    TotalDamageDealtToChampions: 18000,
                    TotalDamageTaken: 22000,
                    TimePlayed: TimeSpan.FromMinutes(25),
                    Won: true,
                    ItemIds: [1001, 2003, 3006],
                    RuneIds: [8005, 9111, 9104])
            ]);
    }

    private sealed class FakeRiotMatchService : IRiotMatchService
    {
        private readonly RiotMatchDetails? _result;
        private readonly Exception? _exception;

        public FakeRiotMatchService(RiotMatchDetails result)
        {
            _result = result;
        }

        public FakeRiotMatchService(Exception exception)
        {
            _exception = exception;
        }

        public int GetMatchCallCount { get; private set; }

        public CancellationToken GetMatchCancellationToken { get; private set; }

        public Task<RiotMatchDetails> GetMatchAsync(
            string matchId,
            CancellationToken cancellationToken = default)
        {
            GetMatchCallCount++;
            GetMatchCancellationToken = cancellationToken;

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_result!);
        }
    }

    private sealed class FakeMatchRepository(
        bool exists,
        Exception? addException = null) : IMatchRepository
    {
        private readonly bool _exists = exists;
        private readonly Exception? _addException = addException;

        public int ExistsCallCount { get; private set; }

        public int AddCallCount { get; private set; }

        public Match? AddedMatch { get; private set; }

        public CancellationToken ExistsCancellationToken { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public Task<bool> ExistsByRiotMatchIdAsync(
            string riotMatchId,
            CancellationToken cancellationToken = default)
        {
            ExistsCallCount++;
            ExistsCancellationToken = cancellationToken;

            return Task.FromResult(_exists);
        }

        public Task AddAsync(
            Match match,
            CancellationToken cancellationToken = default)
        {
            AddCallCount++;
            AddedMatch = match;
            AddCancellationToken = cancellationToken;

            if (_addException is not null)
            {
                throw _addException;
            }

            return Task.CompletedTask;
        }
    }
}
