using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Application.Players.Services;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Tests.Players.Services;

public sealed class PlayerQueryServiceTests
{
    [Fact]
    public async Task GetByPuuidAsync_ForwardsPuuidAndReturnsPlayer()
    {
        var player = new Player(
            "player-puuid",
            "Example Player",
            "EUW");

        var repository = new FakePlayerRepository
        {
            PlayerToReturn = player
        };

        var service = new PlayerQueryService(repository);

        var result = await service.GetByPuuidAsync("player-puuid");

        Assert.Same(player, result);
        Assert.Equal("player-puuid", repository.Puuid);
        Assert.Equal(1, repository.GetByPuuidCallCount);
    }

    [Fact]
    public async Task GetByPuuidAsync_WhenPlayerDoesNotExist_ReturnsNull()
    {
        var repository = new FakePlayerRepository();
        var service = new PlayerQueryService(repository);

        var result = await service.GetByPuuidAsync("missing-puuid");

        Assert.Null(result);
        Assert.Equal(1, repository.GetByPuuidCallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task GetByPuuidAsync_WithInvalidPuuid_ThrowsArgumentException(
        string puuid)
    {
        var repository = new FakePlayerRepository();
        var service = new PlayerQueryService(repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetByPuuidAsync(puuid));

        Assert.Equal(0, repository.GetByPuuidCallCount);
    }

    [Fact]
    public async Task GetByPuuidAsync_WithNullPuuid_ThrowsArgumentNullException()
    {
        var repository = new FakePlayerRepository();
        var service = new PlayerQueryService(repository);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GetByPuuidAsync(null!));

        Assert.Equal(0, repository.GetByPuuidCallCount);
    }

    [Fact]
    public async Task GetByPuuidAsync_WhenRepositoryFails_PropagatesException()
    {
        var expected = new InvalidOperationException("Repository failure");

        var repository = new FakePlayerRepository
        {
            ExceptionToThrow = expected
        };

        var service = new PlayerQueryService(repository);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetByPuuidAsync("player-puuid"));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetByPuuidAsync_PropagatesCancellationToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var repository = new FakePlayerRepository();
        var service = new PlayerQueryService(repository);

        await service.GetByPuuidAsync(
            "player-puuid",
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            repository.CancellationToken);
    }

    private sealed class FakePlayerRepository : IPlayerRepository
    {
        public Player? PlayerToReturn { get; init; }

        public Exception? ExceptionToThrow { get; init; }

        public string? Puuid { get; private set; }

        public int GetByPuuidCallCount { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<Player?> GetByPuuidAsync(
            string puuid,
            CancellationToken cancellationToken = default)
        {
            GetByPuuidCallCount++;
            Puuid = puuid;
            CancellationToken = cancellationToken;

            if (ExceptionToThrow is not null)
            {
                return Task.FromException<Player?>(ExceptionToThrow);
            }

            return Task.FromResult(PlayerToReturn);
        }

        public Task AddAsync(
            Player player,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
