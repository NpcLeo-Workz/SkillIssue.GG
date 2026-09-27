using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Application.Riot.Services;

namespace SkillIssue.GG.Application.Tests.Riot.Services;

public sealed class RiotMatchHistorySyncServiceTests
{
    [Fact]
    public async Task SyncAsync_ImportsAllReturnedMatchesInOrder()
    {
        var historyService = new FakeMatchHistoryService(
        [
            "EUW1_1",
            "EUW1_2",
            "EUW1_3"
        ]);

        var importService = new FakeMatchImportService(
            new Dictionary<string, bool>
            {
                ["EUW1_1"] = true,
                ["EUW1_2"] = false,
                ["EUW1_3"] = true
            });

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        var result = await service.SyncAsync(
            "test-puuid",
            start: 10,
            count: 3);

        Assert.Equal(3, result.Requested);
        Assert.Equal(2, result.Imported);
        Assert.Equal(1, result.Skipped);

        Assert.Equal(
            new[] { "EUW1_1", "EUW1_2", "EUW1_3" },
            importService.ImportedMatchIds);
    }

    [Fact]
    public async Task SyncAsync_PassesPuuidAndPaginationToHistoryService()
    {
        var historyService = new FakeMatchHistoryService([]);
        var importService = new FakeMatchImportService(new Dictionary<string, bool>());

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        await service.SyncAsync(
            "test-puuid",
            start: 20,
            count: 50);

        Assert.Equal("test-puuid", historyService.Puuid);
        Assert.Equal(20, historyService.Start);
        Assert.Equal(50, historyService.Count);
    }

    [Fact]
    public async Task SyncAsync_ReturnsZeroCounts_WhenHistoryIsEmpty()
    {
        var historyService = new FakeMatchHistoryService([]);
        var importService = new FakeMatchImportService(new Dictionary<string, bool>());

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        var result = await service.SyncAsync("test-puuid");

        Assert.Equal(0, result.Requested);
        Assert.Equal(0, result.Imported);
        Assert.Equal(0, result.Skipped);
        Assert.Empty(importService.ImportedMatchIds);
    }

    [Fact]
    public async Task SyncAsync_CountsAllImportedMatches()
    {
        var historyService = new FakeMatchHistoryService(
        [
            "EUW1_1",
            "EUW1_2"
        ]);

        var importService = new FakeMatchImportService(
            new Dictionary<string, bool>
            {
                ["EUW1_1"] = true,
                ["EUW1_2"] = true
            });

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        var result = await service.SyncAsync("test-puuid");

        Assert.Equal(2, result.Requested);
        Assert.Equal(2, result.Imported);
        Assert.Equal(0, result.Skipped);
    }

    [Fact]
    public async Task SyncAsync_CountsAllSkippedMatches()
    {
        var historyService = new FakeMatchHistoryService(
        [
            "EUW1_1",
            "EUW1_2"
        ]);

        var importService = new FakeMatchImportService(
            new Dictionary<string, bool>
            {
                ["EUW1_1"] = false,
                ["EUW1_2"] = false
            });

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        var result = await service.SyncAsync("test-puuid");

        Assert.Equal(2, result.Requested);
        Assert.Equal(0, result.Imported);
        Assert.Equal(2, result.Skipped);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SyncAsync_Throws_WhenPuuidIsInvalid(
        string? puuid)
    {
        var historyService = new FakeMatchHistoryService([]);
        var importService = new FakeMatchImportService(new Dictionary<string, bool>());

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => service.SyncAsync(puuid!));

        Assert.Equal(0, historyService.CallCount);
        Assert.Empty(importService.ImportedMatchIds);
    }

    [Fact]
    public async Task SyncAsync_Throws_WhenStartIsNegative()
    {
        var historyService = new FakeMatchHistoryService([]);
        var importService = new FakeMatchImportService(new Dictionary<string, bool>());

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.SyncAsync(
                "test-puuid",
                start: -1));

        Assert.Equal(0, historyService.CallCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task SyncAsync_Throws_WhenCountIsOutsideAllowedRange(
        int count)
    {
        var historyService = new FakeMatchHistoryService([]);
        var importService = new FakeMatchImportService(new Dictionary<string, bool>());

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.SyncAsync(
                "test-puuid",
                count: count));

        Assert.Equal(0, historyService.CallCount);
    }

    [Fact]
    public async Task SyncAsync_PropagatesHistoryFailure()
    {
        var expected = new InvalidOperationException("History failure");

        var historyService = new FakeMatchHistoryService(expected);
        var importService = new FakeMatchImportService(new Dictionary<string, bool>());

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncAsync("test-puuid"));

        Assert.Same(expected, exception);
        Assert.Empty(importService.ImportedMatchIds);
    }

    [Fact]
    public async Task SyncAsync_StopsWhenImportFails()
    {
        var historyService = new FakeMatchHistoryService(
        [
            "EUW1_1",
            "EUW1_2",
            "EUW1_3"
        ]);

        var expected = new InvalidOperationException("Import failure");

        var importService = new FakeMatchImportService(
            new Dictionary<string, bool>
            {
                ["EUW1_1"] = true
            },
            failingMatchId: "EUW1_2",
            exception: expected);

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncAsync("test-puuid"));

        Assert.Same(expected, exception);

        Assert.Equal(
            new[] { "EUW1_1", "EUW1_2" },
            importService.ImportedMatchIds);
    }

    [Fact]
    public async Task SyncAsync_PassesCancellationTokenToDependencies()
    {
        var historyService = new FakeMatchHistoryService(
        [
            "EUW1_1",
            "EUW1_2"
        ]);

        var importService = new FakeMatchImportService(
            new Dictionary<string, bool>
            {
                ["EUW1_1"] = true,
                ["EUW1_2"] = false
            });

        var service = new RiotMatchHistorySyncService(
            historyService,
            importService);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var token = cancellationTokenSource.Token;

        await service.SyncAsync(
            "test-puuid",
            cancellationToken: token);

        Assert.Equal(
            token,
            historyService.CancellationToken);

        Assert.All(
            importService.CancellationTokens,
            capturedToken => Assert.Equal(token, capturedToken));
    }

    private sealed class FakeMatchHistoryService
        : IRiotMatchHistoryService
    {
        private readonly IReadOnlyList<string>? _matchIds;
        private readonly Exception? _exception;

        public FakeMatchHistoryService(
            IReadOnlyList<string> matchIds)
        {
            _matchIds = matchIds;
        }

        public FakeMatchHistoryService(
            Exception exception)
        {
            _exception = exception;
        }

        public int CallCount { get; private set; }

        public string? Puuid { get; private set; }

        public int Start { get; private set; }

        public int Count { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<IReadOnlyList<string>> GetMatchIdsAsync(
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

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_matchIds!);
        }
    }

    private sealed class FakeMatchImportService(
        IReadOnlyDictionary<string, bool> results,
        string? failingMatchId = null,
        Exception? exception = null)
                : IRiotMatchImportService
    {
        private readonly IReadOnlyDictionary<string, bool> _results = results;
        private readonly string? _failingMatchId = failingMatchId;
        private readonly Exception? _exception = exception;

        public List<string> ImportedMatchIds { get; } = [];

        public List<CancellationToken> CancellationTokens { get; } = [];

        public Task<RiotMatchImportResult> ImportAsync(
            string matchId,
            CancellationToken cancellationToken = default)
        {
            ImportedMatchIds.Add(matchId);
            CancellationTokens.Add(cancellationToken);

            if (matchId == _failingMatchId)
            {
                throw _exception!;
            }

            var imported = _results[matchId];

            return Task.FromResult(
                new RiotMatchImportResult(
                    matchId,
                    imported,
                    imported ? Guid.NewGuid() : null));
        }
    }
}
