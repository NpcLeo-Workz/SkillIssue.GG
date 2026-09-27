using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Application.Matches.Services;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Tests.Matches.Services;

public sealed class MatchQueryServiceTests
{
    [Fact]
    public async Task GetByRiotMatchIdAsync_ForwardsMatchIdAndReturnsMatch()
    {
        var match = CreateMatch();

        var repository = new FakeMatchRepository
        {
            MatchToReturn = match
        };

        var service = new MatchQueryService(repository);

        var result = await service.GetByRiotMatchIdAsync("EUW1_123456");

        Assert.Same(match, result);
        Assert.Equal("EUW1_123456", repository.RiotMatchId);
        Assert.Equal(1, repository.GetByRiotMatchIdCallCount);
    }

    [Fact]
    public async Task GetByRiotMatchIdAsync_WhenMatchDoesNotExist_ReturnsNull()
    {
        var repository = new FakeMatchRepository();
        var service = new MatchQueryService(repository);

        var result = await service.GetByRiotMatchIdAsync("EUW1_missing");

        Assert.Null(result);
        Assert.Equal(1, repository.GetByRiotMatchIdCallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task GetByRiotMatchIdAsync_WithInvalidMatchId_ThrowsArgumentException(
        string riotMatchId)
    {
        var repository = new FakeMatchRepository();
        var service = new MatchQueryService(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetByRiotMatchIdAsync(riotMatchId));

        Assert.Equal(0, repository.GetByRiotMatchIdCallCount);
    }

    [Fact]
    public async Task GetByRiotMatchIdAsync_WithNullMatchId_ThrowsArgumentNullException()
    {
        var repository = new FakeMatchRepository();
        var service = new MatchQueryService(repository);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GetByRiotMatchIdAsync(null!));

        Assert.Equal(0, repository.GetByRiotMatchIdCallCount);
    }

    [Fact]
    public async Task GetByRiotMatchIdAsync_WhenRepositoryFails_PropagatesException()
    {
        var expected = new InvalidOperationException("Repository failure");

        var repository = new FakeMatchRepository
        {
            ExceptionToThrow = expected
        };

        var service = new MatchQueryService(repository);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetByRiotMatchIdAsync("EUW1_123456"));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetByRiotMatchIdAsync_PropagatesCancellationToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var repository = new FakeMatchRepository();
        var service = new MatchQueryService(repository);

        await service.GetByRiotMatchIdAsync(
            "EUW1_123456",
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            repository.CancellationToken);
    }

    private static Match CreateMatch()
    {
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-30);

        return new Match(
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
    }

    private sealed class FakeMatchRepository : IMatchRepository
    {
        public Match? MatchToReturn { get; init; }

        public Exception? ExceptionToThrow { get; init; }

        public string? RiotMatchId { get; private set; }

        public int GetByRiotMatchIdCallCount { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<Match?> GetByRiotMatchIdAsync(
            string riotMatchId,
            CancellationToken cancellationToken = default)
        {
            GetByRiotMatchIdCallCount++;
            RiotMatchId = riotMatchId;
            CancellationToken = cancellationToken;

            if (ExceptionToThrow is not null)
            {
                return Task.FromException<Match?>(ExceptionToThrow);
            }

            return Task.FromResult(MatchToReturn);
        }

        public Task<bool> ExistsByRiotMatchIdAsync(
            string riotMatchId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Match>> GetByPlayerPuuidAsync(
            string puuid,
            int skip,
            int take,
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
