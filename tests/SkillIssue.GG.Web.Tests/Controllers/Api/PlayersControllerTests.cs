using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Controllers.Api;
using SkillIssue.GG.Web.Models.Players;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class PlayersControllerTests
{
    [Fact]
    public async Task GetAsync_WithPersistedPlayer_ReturnsOkAndMapsPlayer()
    {
        var player = new Player(
            "player-puuid",
            "Example Player",
            "EUW");

        var service = new FakePlayerQueryService
        {
            PlayerToReturn = player
        };

        var controller = new PlayersController(service);

        var result = await controller.GetAsync(
            "player-puuid",
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PlayerResponse>(okResult.Value);

        Assert.Equal(player.Id, response.Id);
        Assert.Equal("player-puuid", response.Puuid);
        Assert.Equal("Example Player", response.Name);
        Assert.Equal("EUW", response.Region);

        Assert.Equal("player-puuid", service.Puuid);
        Assert.Equal(1, service.CallCount);
    }

    [Fact]
    public async Task GetAsync_WhenPlayerDoesNotExist_ReturnsNotFound()
    {
        var service = new FakePlayerQueryService();
        var controller = new PlayersController(service);

        var result = await controller.GetAsync(
            "missing-puuid",
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);

        Assert.Equal("missing-puuid", service.Puuid);
        Assert.Equal(1, service.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task GetAsync_WithInvalidPuuid_ReturnsBadRequest(
        string puuid)
    {
        var service = new FakePlayerQueryService();
        var controller = new PlayersController(service);

        var result = await controller.GetAsync(
            puuid,
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(0, service.CallCount);
    }

    [Fact]
    public async Task GetAsync_PropagatesCancellationToken()
    {
        using var cancellationTokenSource =
            new CancellationTokenSource();

        var service = new FakePlayerQueryService();

        var controller = new PlayersController(service);

        await controller.GetAsync(
            "player-puuid",
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            service.CancellationToken);
    }

    private sealed class FakePlayerQueryService : IPlayerQueryService
    {
        public Player? PlayerToReturn { get; init; }

        public string? Puuid { get; private set; }

        public int CallCount { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<Player?> GetByPuuidAsync(
            string puuid,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Puuid = puuid;
            CancellationToken = cancellationToken;

            return Task.FromResult(PlayerToReturn);
        }
    }
}
