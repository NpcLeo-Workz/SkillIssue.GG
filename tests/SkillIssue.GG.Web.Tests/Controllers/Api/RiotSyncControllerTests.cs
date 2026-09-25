using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Web.Controllers.Api;
using SkillIssue.GG.Web.Models.Api;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class RiotSyncControllerTests
{
    [Fact]
    public async Task SyncAsync_ForwardsRequestAndMapsResult()
    {
        var playerId = Guid.NewGuid();

        var service = new FakeSyncService
        {
            Result = new RiotPlayerAndMatchSyncResult(
                PlayerId: playerId,
                Puuid: "test-puuid",
                PlayerCreated: true,
                RequestedMatches: 20,
                ImportedMatches: 15,
                SkippedMatches: 5)
        };

        var controller = CreateController(service);

        var request = new RiotSyncRequest(
            GameName: "Player",
            TagLine: "EUW",
            Region: "euw1",
            Start: 10,
            Count: 20);

        var actionResult = await controller.SyncAsync(request);

        Assert.Equal("Player", service.GameName);
        Assert.Equal("EUW", service.TagLine);
        Assert.Equal("euw1", service.Region);
        Assert.Equal(10, service.Start);
        Assert.Equal(20, service.Count);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<RiotSyncResponse>(okResult.Value);

        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal(playerId, response.PlayerId);
        Assert.Equal("test-puuid", response.Puuid);
        Assert.True(response.PlayerCreated);
        Assert.Equal(20, response.RequestedMatches);
        Assert.Equal(15, response.ImportedMatches);
        Assert.Equal(5, response.SkippedMatches);
    }

    [Fact]
    public async Task SyncAsync_PassesRequestAbortedCancellationToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var service = new FakeSyncService
        {
            Result = new RiotPlayerAndMatchSyncResult(
                Guid.NewGuid(),
                "test-puuid",
                false,
                0,
                0,
                0)
        };

        var controller = CreateController(
            service,
            cancellationTokenSource.Token);

        var request = new RiotSyncRequest(
            "Player",
            "EUW",
            "euw1");

        await controller.SyncAsync(request);

        Assert.Equal(
            cancellationTokenSource.Token,
            service.CancellationToken);
    }

    private static RiotSyncController CreateController(
        FakeSyncService service,
        CancellationToken cancellationToken = default)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestAborted = cancellationToken
        };

        return new RiotSyncController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private sealed class FakeSyncService
        : IRiotPlayerAndMatchSyncService
    {
        public RiotPlayerAndMatchSyncResult Result { get; set; } =
            new(
                Guid.NewGuid(),
                "test-puuid",
                false,
                0,
                0,
                0);

        public string? GameName { get; private set; }

        public string? TagLine { get; private set; }

        public string? Region { get; private set; }

        public int Start { get; private set; }

        public int Count { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<RiotPlayerAndMatchSyncResult> SyncAsync(
            string gameName,
            string tagLine,
            string region,
            int start = 0,
            int count = 20,
            CancellationToken cancellationToken = default)
        {
            GameName = gameName;
            TagLine = tagLine;
            Region = region;
            Start = start;
            Count = count;
            CancellationToken = cancellationToken;

            return Task.FromResult(Result);
        }
    }
}
