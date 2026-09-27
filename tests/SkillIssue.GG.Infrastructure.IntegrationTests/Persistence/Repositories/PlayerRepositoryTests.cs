using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Infrastructure.Persistence.Repositories;

namespace SkillIssue.GG.Infrastructure.IntegrationTests.Persistence.Repositories;

public sealed class PlayerRepositoryTests(PostgreSqlFixture fixture)
        : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture = fixture;

    [Fact]
    public async Task AddAsync_PersistsPlayer()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PlayerRepository(dbContext);

        var player = new Player(
            "test-puuid-1",
            "TestPlayer",
            "EUW");

        await repository.AddAsync(player);

        await using var verificationContext =
            _fixture.CreateDbContext();

        var persistedPlayer = await verificationContext.Players
            .SingleAsync(x => x.Puuid == player.Puuid);

        Assert.Equal(player.Id, persistedPlayer.Id);
        Assert.Equal(player.Puuid, persistedPlayer.Puuid);
        Assert.Equal(player.Name, persistedPlayer.Name);
        Assert.Equal(player.Region, persistedPlayer.Region);
    }

    [Fact]
    public async Task GetByPuuidAsync_ReturnsPlayer_WhenPlayerExists()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var player = new Player(
            "test-puuid-2",
            "ExistingPlayer",
            "EUW");

        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var repository = new PlayerRepository(dbContext);

        var result = await repository.GetByPuuidAsync(
            player.Puuid);

        Assert.NotNull(result);
        Assert.Equal(player.Id, result.Id);
        Assert.Equal(player.Puuid, result.Puuid);
        Assert.Equal(player.Name, result.Name);
        Assert.Equal(player.Region, result.Region);
    }

    [Fact]
    public async Task GetByPuuidAsync_ReturnsNull_WhenPlayerDoesNotExist()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PlayerRepository(dbContext);

        var result = await repository.GetByPuuidAsync(
            "unknown-puuid");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Throws_WhenPuuidAlreadyExists()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PlayerRepository(dbContext);

        var first = new Player(
            "duplicate-puuid",
            "FirstPlayer",
            "EUW");

        var second = new Player(
            "duplicate-puuid",
            "SecondPlayer",
            "EUW");

        await repository.AddAsync(first);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => repository.AddAsync(second));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByPuuidAsync_Throws_WhenPuuidIsInvalid(
        string? puuid)
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PlayerRepository(dbContext);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => repository.GetByPuuidAsync(puuid!));
    }

    [Fact]
    public async Task AddAsync_Throws_WhenPlayerIsNull()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PlayerRepository(dbContext);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.AddAsync(null!));
    }

    [Fact]
    public async Task GetByPuuidAsync_PropagatesCancellation()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PlayerRepository(dbContext);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => repository.GetByPuuidAsync(
                "test-puuid",
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task AddAsync_PropagatesCancellation()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PlayerRepository(dbContext);

        var player = new Player(
            "test-puuid-cancel",
            "CancelledPlayer",
            "EUW");

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => repository.AddAsync(
                player,
                cancellationTokenSource.Token));
    }
    [Fact]
    public async Task GetByPuuidAsync_WithMatchingPuuid_ReturnsPlayer()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var puuid = $"target-{Guid.NewGuid():N}";

        var player = new Player(
            puuid,
            "Target Player",
            "EUW");

        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var repository = new PlayerRepository(dbContext);

        var result = await repository.GetByPuuidAsync(puuid);

        Assert.NotNull(result);
        Assert.Equal(player.Id, result.Id);
        Assert.Equal(puuid, result.Puuid);
        Assert.Equal("Target Player", result.Name);
        Assert.Equal("EUW", result.Region);
    }

    [Fact]
    public async Task GetByPuuidAsync_DoesNotReturnUnrelatedPlayer()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var persistedPuuid = $"other-{Guid.NewGuid():N}";
        var requestedPuuid = $"target-{Guid.NewGuid():N}";
        var player = new Player(
            persistedPuuid,
            "Other Player",
            "EUW");

        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var repository = new PlayerRepository(dbContext);

        var result = await repository.GetByPuuidAsync(requestedPuuid);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPuuidAsync_WhenPlayerDoesNotExist_ReturnsNull()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new PlayerRepository(dbContext);
        var puuid = $"missing-{Guid.NewGuid():N}";
        var result = await repository.GetByPuuidAsync(puuid);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPuuidAsync_DoesNotTrackReturnedPlayer()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var puuid = $"tracking-{Guid.NewGuid():N}";
        var player = new Player(
            puuid,
            "Target Player",
            "EUW");

        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var repository = new PlayerRepository(dbContext);

        var result = await repository.GetByPuuidAsync(puuid);

        Assert.NotNull(result);

        Assert.Empty(
            dbContext.ChangeTracker.Entries<Player>());
    }

    [Fact]
    public async Task GetByPuuidAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new PlayerRepository(dbContext);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => repository.GetByPuuidAsync(
                "player-puuid",
                cancellationTokenSource.Token));
    }
}
