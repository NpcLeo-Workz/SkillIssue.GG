using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Application.Matches.Services;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Tests.Matches.Services;

public sealed class PlayerMatchHistoryServiceTests
{
    [Fact]
    public async Task GetAsync_ForwardsPuuidSkipAndTake()
    {
        var repository = new FakeMatchRepository();
        var service = new PlayerMatchHistoryService(repository);

        await service.GetAsync(
            "test-puuid",
            skip: 10,
            take: 25);

        Assert.Equal("test-puuid", repository.Puuid);
        Assert.Equal(10, repository.Skip);
        Assert.Equal(25, repository.Take);
        Assert.Equal(1, repository.GetByPlayerPuuidCallCount);
    }

    [Fact]
    public async Task GetAsync_ReturnsRepositoryResult()
    {
        var match = CreateMatch();

        var repository = new FakeMatchRepository
        {
            GetByPlayerPuuidResult = [match]
        };

        var service = new PlayerMatchHistoryService(repository);

        var result = await service.GetAsync("test-puuid");

        Assert.Single(result);
        Assert.Same(match, result[0]);
    }

    [Fact]
    public async Task GetAsync_WhenHistoryIsEmpty_ReturnsEmptyCollection()
    {
        var repository = new FakeMatchRepository
        {
            GetByPlayerPuuidResult = []
        };

        var service = new PlayerMatchHistoryService(repository);

        var result = await service.GetAsync("test-puuid");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_WhenPuuidIsNull_ThrowsBeforeRepositoryAccess()
    {
        var repository = new FakeMatchRepository();
        var service = new PlayerMatchHistoryService(repository);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GetAsync(null!));

        Assert.Equal(0, repository.GetByPlayerPuuidCallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAsync_WhenPuuidIsEmptyOrWhitespace_ThrowsBeforeRepositoryAccess(
        string puuid)
    {
        var repository = new FakeMatchRepository();
        var service = new PlayerMatchHistoryService(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetAsync(puuid));

        Assert.Equal(0, repository.GetByPlayerPuuidCallCount);
    }

    [Fact]
    public async Task GetAsync_WhenRepositoryFails_PropagatesException()
    {
        var expectedException = new InvalidOperationException(
            "Persistence failed.");

        var repository = new FakeMatchRepository
        {
            Exception = expectedException
        };

        var service = new PlayerMatchHistoryService(repository);

        var actualException =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetAsync("test-puuid"));

        Assert.Same(expectedException, actualException);
    }

    [Fact]
    public async Task GetAsync_PassesCancellationToken()
    {
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var repository = new FakeMatchRepository();
        var service = new PlayerMatchHistoryService(repository);

        await service.GetAsync(
            "test-puuid",
            cancellationToken: cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            repository.CancellationToken);
    }

    private static Match CreateMatch()
    {
        var startedAt = DateTimeOffset.UtcNow;

        return new Match(
            riotMatchId: "EUW1_TEST",
            riotGameId: 1,
            dataVersion: "2",
            gameVersion: "16.15.1.1234",
            gameMode: "CLASSIC",
            gameType: "MATCHED_GAME",
            mapId: 11,
            queueId: 420,
            platformId: "EUW1",
            gameCreatedAt: startedAt,
            startedAt: startedAt,
            endedAt: startedAt.AddMinutes(30),
            duration: TimeSpan.FromMinutes(30),
            endOfGameResult: "GameComplete");
    }

    private sealed class FakeMatchRepository : IMatchRepository
    {
        public IReadOnlyList<Match> GetByPlayerPuuidResult { get; set; } = [];

        public Exception? Exception { get; set; }

        public string? Puuid { get; private set; }

        public int Skip { get; private set; }

        public int Take { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public int GetByPlayerPuuidCallCount { get; private set; }

        public Task<IReadOnlyList<Match>> GetByPlayerPuuidAsync(
            string puuid,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            GetByPlayerPuuidCallCount++;

            Puuid = puuid;
            Skip = skip;
            Take = take;
            CancellationToken = cancellationToken;

            if (Exception is not null)
            {
                return Task.FromException<IReadOnlyList<Match>>(Exception);
            }

            return Task.FromResult(GetByPlayerPuuidResult);
        }

        public Task<bool> ExistsByRiotMatchIdAsync(
            string riotMatchId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddAsync(
            Match match,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
