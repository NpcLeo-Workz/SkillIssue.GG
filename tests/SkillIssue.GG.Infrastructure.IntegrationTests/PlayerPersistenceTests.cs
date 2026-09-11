using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class PlayerPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public PlayerPersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanPersistAndLoadPlayer()
    {
        var player = new Player(
            "test-puuid-123",
            "TestPlayer",
            "EUW"
        );

        await using (var dbContext = _fixture.CreateDbContext())
        {
            dbContext.Players.Add(player);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var storedPlayer = await dbContext.Players
                .SingleAsync(x => x.Id == player.Id);

            Assert.Equal(player.Puuid, storedPlayer.Puuid);
            Assert.Equal(player.Name, storedPlayer.Name);
            Assert.Equal(player.Region, storedPlayer.Region);
        }
    }

    [Fact]
    public async Task DuplicatePuuidThrowsDbUpdateException()
    {
        var firstPlayer = new Player(
            "duplicate-puuid",
            "PlayerOne",
            "EUW"
        );

        var secondPlayer = new Player(
            "duplicate-puuid",
            "PlayerTwo",
            "EUW"
        );

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Players.Add(firstPlayer);
        await dbContext.SaveChangesAsync();

        dbContext.Players.Add(secondPlayer);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());
    }
}
