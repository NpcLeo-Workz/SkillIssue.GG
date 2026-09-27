using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Application.Riot.Services;

namespace SkillIssue.GG.Application.Tests.Riot.Services;

public sealed class RiotPlayerAndMatchSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_ReturnsCombinedResult()
    {
        var playerId = Guid.NewGuid();

        var playerSync = new FakePlayerSyncService(
            new RiotPlayerSyncResult(
                playerId,
                "test-puuid",
                Created: true));

        var matchSync = new FakeMatchHistorySyncService(
            new RiotMatchHistorySyncResult(
                Requested: 5,
                Imported: 3,
                Skipped: 2));

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        var result = await service.SyncAsync(
            "TestPlayer",
            "EUW",
            "euw1");

        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal("test-puuid", result.Puuid);
        Assert.True(result.PlayerCreated);
        Assert.Equal(5, result.RequestedMatches);
        Assert.Equal(3, result.ImportedMatches);
        Assert.Equal(2, result.SkippedMatches);
    }

    [Fact]
    public async Task SyncAsync_PassesPlayerInputToPlayerSync()
    {
        var playerSync = CreatePlayerSync();
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await service.SyncAsync(
            "TestPlayer",
            "EUW",
            "euw1");

        Assert.Equal("TestPlayer", playerSync.GameName);
        Assert.Equal("EUW", playerSync.TagLine);
        Assert.Equal("euw1", playerSync.Region);
    }

    [Fact]
    public async Task SyncAsync_PassesReturnedPuuidAndPaginationToMatchSync()
    {
        var playerSync = new FakePlayerSyncService(
            new RiotPlayerSyncResult(
                Guid.NewGuid(),
                "resolved-puuid",
                Created: false));

        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await service.SyncAsync(
            "TestPlayer",
            "EUW",
            "euw1",
            start: 20,
            count: 50);

        Assert.Equal("resolved-puuid", matchSync.Puuid);
        Assert.Equal(20, matchSync.Start);
        Assert.Equal(50, matchSync.Count);
    }

    [Fact]
    public async Task SyncAsync_SynchronizesMatches_WhenPlayerAlreadyExists()
    {
        var playerSync = new FakePlayerSyncService(
            new RiotPlayerSyncResult(
                Guid.NewGuid(),
                "existing-puuid",
                Created: false));

        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        var result = await service.SyncAsync(
            "Player",
            "TAG",
            "euw1");

        Assert.False(result.PlayerCreated);
        Assert.Equal(1, matchSync.CallCount);
    }

    [Fact]
    public async Task SyncAsync_CallsPlayerSyncBeforeMatchSync()
    {
        var calls = new List<string>();

        var playerSync = new FakePlayerSyncService(
            new RiotPlayerSyncResult(
                Guid.NewGuid(),
                "test-puuid",
                Created: true),
            calls);

        var matchSync = new FakeMatchHistorySyncService(
            new RiotMatchHistorySyncResult(1, 1, 0),
            calls);

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await service.SyncAsync(
            "Player",
            "TAG",
            "euw1");

        Assert.Equal(
            new[] { "player", "matches" },
            calls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SyncAsync_Throws_WhenGameNameIsInvalid(
        string? gameName)
    {
        var playerSync = CreatePlayerSync();
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.SyncAsync(
                gameName!,
                "TAG",
                "euw1"));

        AssertNoDependencyCalls(playerSync, matchSync);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SyncAsync_Throws_WhenTagLineIsInvalid(
        string? tagLine)
    {
        var playerSync = CreatePlayerSync();
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.SyncAsync(
                "Player",
                tagLine!,
                "euw1"));

        AssertNoDependencyCalls(playerSync, matchSync);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SyncAsync_Throws_WhenRegionIsInvalid(
        string? region)
    {
        var playerSync = CreatePlayerSync();
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.SyncAsync(
                "Player",
                "TAG",
                region!));

        AssertNoDependencyCalls(playerSync, matchSync);
    }

    [Fact]
    public async Task SyncAsync_Throws_WhenStartIsNegative()
    {
        var playerSync = CreatePlayerSync();
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.SyncAsync(
                "Player",
                "TAG",
                "euw1",
                start: -1));

        AssertNoDependencyCalls(playerSync, matchSync);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task SyncAsync_Throws_WhenCountIsInvalid(
        int count)
    {
        var playerSync = CreatePlayerSync();
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.SyncAsync(
                "Player",
                "TAG",
                "euw1",
                count: count));

        AssertNoDependencyCalls(playerSync, matchSync);
    }

    [Fact]
    public async Task SyncAsync_DoesNotSynchronizeMatches_WhenPlayerSyncFails()
    {
        var expected =
            new InvalidOperationException("Player sync failed.");

        var playerSync = new FakePlayerSyncService(expected);
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SyncAsync(
                    "Player",
                    "TAG",
                    "euw1"));

        Assert.Same(expected, exception);
        Assert.Equal(1, playerSync.CallCount);
        Assert.Equal(0, matchSync.CallCount);
    }

    [Fact]
    public async Task SyncAsync_PropagatesMatchSyncFailure()
    {
        var expected =
            new InvalidOperationException("Match sync failed.");

        var playerSync = CreatePlayerSync();

        var matchSync =
            new FakeMatchHistorySyncService(expected);

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SyncAsync(
                    "Player",
                    "TAG",
                    "euw1"));

        Assert.Same(expected, exception);
        Assert.Equal(1, playerSync.CallCount);
        Assert.Equal(1, matchSync.CallCount);
    }

    [Fact]
    public async Task SyncAsync_PassesCancellationTokenToBothServices()
    {
        var playerSync = CreatePlayerSync();
        var matchSync = CreateMatchSync();

        var service = new RiotPlayerAndMatchSyncService(
            playerSync,
            matchSync);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var token = cancellationTokenSource.Token;

        await service.SyncAsync(
            "Player",
            "TAG",
            "euw1",
            cancellationToken: token);

        Assert.Equal(token, playerSync.CancellationToken);
        Assert.Equal(token, matchSync.CancellationToken);
    }

    private static FakePlayerSyncService CreatePlayerSync()
    {
        return new FakePlayerSyncService(
            new RiotPlayerSyncResult(
                Guid.NewGuid(),
                "test-puuid",
                Created: true));
    }

    private static FakeMatchHistorySyncService CreateMatchSync()
    {
        return new FakeMatchHistorySyncService(
            new RiotMatchHistorySyncResult(
                Requested: 2,
                Imported: 1,
                Skipped: 1));
    }

    private static void AssertNoDependencyCalls(
        FakePlayerSyncService playerSync,
        FakeMatchHistorySyncService matchSync)
    {
        Assert.Equal(0, playerSync.CallCount);
        Assert.Equal(0, matchSync.CallCount);
    }

    private sealed class FakePlayerSyncService
        : IRiotPlayerSyncService
    {
        private readonly RiotPlayerSyncResult? _result;
        private readonly Exception? _exception;
        private readonly List<string>? _calls;

        public FakePlayerSyncService(
            RiotPlayerSyncResult result,
            List<string>? calls = null)
        {
            _result = result;
            _calls = calls;
        }

        public FakePlayerSyncService(Exception exception)
        {
            _exception = exception;
        }

        public int CallCount { get; private set; }

        public string? GameName { get; private set; }

        public string? TagLine { get; private set; }

        public string? Region { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<RiotPlayerSyncResult> SyncAsync(
            string gameName,
            string tagLine,
            string region,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            GameName = gameName;
            TagLine = tagLine;
            Region = region;
            CancellationToken = cancellationToken;

            _calls?.Add("player");

            if (_exception is not null)
                throw _exception;

            return Task.FromResult(_result!);
        }
    }

    private sealed class FakeMatchHistorySyncService
        : IRiotMatchHistorySyncService
    {
        private readonly RiotMatchHistorySyncResult? _result;
        private readonly Exception? _exception;
        private readonly List<string>? _calls;

        public FakeMatchHistorySyncService(
            RiotMatchHistorySyncResult result,
            List<string>? calls = null)
        {
            _result = result;
            _calls = calls;
        }

        public FakeMatchHistorySyncService(Exception exception)
        {
            _exception = exception;
        }

        public int CallCount { get; private set; }

        public string? Puuid { get; private set; }

        public int Start { get; private set; }

        public int Count { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<RiotMatchHistorySyncResult> SyncAsync(
            string puuid,
            int start = 0,
            int count = 20,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Puuid = puuid;
            Start = start;
            Count = count;
            CancellationToken = cancellationToken;

            _calls?.Add("matches");

            if (_exception is not null)
                throw _exception;

            return Task.FromResult(_result!);
        }
    }
}
