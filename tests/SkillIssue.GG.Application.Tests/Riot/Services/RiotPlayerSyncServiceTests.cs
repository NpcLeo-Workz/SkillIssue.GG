using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Application.Riot.Services;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Tests.Riot.Services;

public sealed class RiotPlayerSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_PassesRiotIdToAccountService()
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("test-puuid", "TestPlayer", "EUW"));

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        await service.SyncAsync(
            "TestPlayer",
            "EUW",
            "euw1");

        Assert.Equal("TestPlayer", accountService.GameName);
        Assert.Equal("EUW", accountService.TagLine);
    }

    [Fact]
    public async Task SyncAsync_UsesRiotPuuidForRepositoryLookup()
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("riot-puuid", "TestPlayer", "EUW"));

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        await service.SyncAsync(
            "TestPlayer",
            "EUW",
            "euw1");

        Assert.Equal("riot-puuid", repository.LookedUpPuuid);
    }

    [Fact]
    public async Task SyncAsync_ReturnsExistingPlayerWithoutPersisting()
    {
        var existingPlayer = new Player(
            "existing-puuid",
            "ExistingPlayer",
            "euw1");

        var accountService = new FakeRiotAccountService(
            new RiotAccount(
                "existing-puuid",
                "CurrentRiotName",
                "EUW"));

        var repository = new FakePlayerRepository(existingPlayer);

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        var result = await service.SyncAsync(
            "CurrentRiotName",
            "EUW",
            "euw1");

        Assert.False(result.Created);
        Assert.Equal(existingPlayer.Id, result.PlayerId);
        Assert.Equal(existingPlayer.Puuid, result.Puuid);

        Assert.Null(repository.AddedPlayer);

        // Existing Player data must not be updated by this workflow.
        Assert.Equal("ExistingPlayer", existingPlayer.Name);
        Assert.Equal("euw1", existingPlayer.Region);
    }

    [Fact]
    public async Task SyncAsync_CreatesAndPersistsMissingPlayer()
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount(
                "new-puuid",
                "ResolvedName",
                "EUW"));

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        var result = await service.SyncAsync(
            "RequestedName",
            "EUW",
            "euw1");

        var player = Assert.IsType<Player>(
            repository.AddedPlayer);

        Assert.Equal("new-puuid", player.Puuid);
        Assert.Equal("ResolvedName", player.Name);
        Assert.Equal("euw1", player.Region);

        Assert.True(result.Created);
        Assert.Equal(player.Id, result.PlayerId);
        Assert.Equal(player.Puuid, result.Puuid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SyncAsync_Throws_WhenGameNameIsInvalid(
        string? gameName)
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("puuid", "Player", "TAG"));

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.SyncAsync(
                gameName!,
                "TAG",
                "euw1"));

        Assert.Equal(0, accountService.CallCount);
        Assert.Equal(0, repository.LookupCallCount);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SyncAsync_Throws_WhenTagLineIsInvalid(
        string? tagLine)
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("puuid", "Player", "TAG"));

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.SyncAsync(
                "Player",
                tagLine!,
                "euw1"));

        Assert.Equal(0, accountService.CallCount);
        Assert.Equal(0, repository.LookupCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SyncAsync_Throws_WhenRegionIsInvalid(
        string? region)
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("puuid", "Player", "TAG"));

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.SyncAsync(
                "Player",
                "TAG",
                region!));

        Assert.Equal(0, accountService.CallCount);
        Assert.Equal(0, repository.LookupCallCount);
    }

    [Fact]
    public async Task SyncAsync_PropagatesAccountLookupFailure()
    {
        var expected =
            new InvalidOperationException("Account lookup failed.");

        var accountService =
            new FakeRiotAccountService(expected);

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SyncAsync(
                    "Player",
                    "TAG",
                    "euw1"));

        Assert.Same(expected, exception);
        Assert.Equal(0, repository.LookupCallCount);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task SyncAsync_PropagatesRepositoryLookupFailure()
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("puuid", "Player", "TAG"));

        var expected =
            new InvalidOperationException("Repository lookup failed.");

        var repository = new FakePlayerRepository(
            lookupException: expected);

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SyncAsync(
                    "Player",
                    "TAG",
                    "euw1"));

        Assert.Same(expected, exception);
        Assert.Equal(0, repository.AddCallCount);
    }

    [Fact]
    public async Task SyncAsync_PropagatesRepositoryPersistenceFailure()
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("puuid", "Player", "TAG"));

        var expected =
            new InvalidOperationException("Persistence failed.");

        var repository = new FakePlayerRepository(
            addException: expected);

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SyncAsync(
                    "Player",
                    "TAG",
                    "euw1"));

        Assert.Same(expected, exception);
        Assert.Equal(1, repository.AddCallCount);
    }

    [Fact]
    public async Task SyncAsync_PassesCancellationTokenToDependencies()
    {
        var accountService = new FakeRiotAccountService(
            new RiotAccount("puuid", "Player", "TAG"));

        var repository = new FakePlayerRepository();

        var service = new RiotPlayerSyncService(
            accountService,
            repository);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var token = cancellationTokenSource.Token;

        await service.SyncAsync(
            "Player",
            "TAG",
            "euw1",
            token);

        Assert.Equal(
            token,
            accountService.CancellationToken);

        Assert.Equal(
            token,
            repository.LookupCancellationToken);

        Assert.Equal(
            token,
            repository.AddCancellationToken);
    }

    private sealed class FakeRiotAccountService
        : IRiotAccountService
    {
        private readonly RiotAccount? _account;
        private readonly Exception? _exception;

        public FakeRiotAccountService(RiotAccount account)
        {
            _account = account;
        }

        public FakeRiotAccountService(Exception exception)
        {
            _exception = exception;
        }

        public int CallCount { get; private set; }

        public string? GameName { get; private set; }

        public string? TagLine { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<RiotAccount> GetByRiotIdAsync(
            string gameName,
            string tagLine,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            GameName = gameName;
            TagLine = tagLine;
            CancellationToken = cancellationToken;

            if (_exception is not null)
                throw _exception;

            return Task.FromResult(_account!);
        }
    }

    private sealed class FakePlayerRepository(
        Player? existingPlayer = null,
        Exception? lookupException = null,
        Exception? addException = null)
                : IPlayerRepository
    {
        private readonly Player? _existingPlayer = existingPlayer;
        private readonly Exception? _lookupException = lookupException;
        private readonly Exception? _addException = addException;

        public int LookupCallCount { get; private set; }

        public int AddCallCount { get; private set; }

        public string? LookedUpPuuid { get; private set; }

        public Player? AddedPlayer { get; private set; }

        public CancellationToken LookupCancellationToken { get; private set; }

        public CancellationToken AddCancellationToken { get; private set; }

        public Task<Player?> GetByPuuidAsync(
            string puuid,
            CancellationToken cancellationToken = default)
        {
            LookupCallCount++;
            LookedUpPuuid = puuid;
            LookupCancellationToken = cancellationToken;

            if (_lookupException is not null)
                throw _lookupException;

            return Task.FromResult(_existingPlayer);
        }

        public Task AddAsync(
            Player player,
            CancellationToken cancellationToken = default)
        {
            AddCallCount++;
            AddedPlayer = player;
            AddCancellationToken = cancellationToken;

            if (_addException is not null)
                throw _addException;

            return Task.CompletedTask;
        }
    }
}
