using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class ReferenceDataPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public ReferenceDataPersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanPersistAndLoadReferenceData()
    {
        var champion = new Champion(
            266,
            "Aatrox"
        );

        var item = new Item(
            3078,
            "Trinity Force"
        );

        var rune = new Rune(
            8005,
            "Press the Attack",
            "Test description",
            "/runes/8005.png",
            8000,
            "Precision"
        );

        var patch = new Patch(
            "16.15",
            "16.15.1"
        );

        await using (var dbContext = _fixture.CreateDbContext())
        {
            dbContext.Champions.Add(champion);
            dbContext.Items.Add(item);
            dbContext.Runes.Add(rune);
            dbContext.Patches.Add(patch);

            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var storedChampion = await dbContext.Champions
                .SingleAsync(x => x.Id == champion.Id);

            var storedItem = await dbContext.Items
                .SingleAsync(x => x.Id == item.Id);

            var storedRune = await dbContext.Runes
                .SingleAsync(x => x.Id == rune.Id);

            var storedPatch = await dbContext.Patches
                .SingleAsync(x => x.Id == patch.Id);

            Assert.Equal(266, storedChampion.RiotChampionId);
            Assert.Equal("Aatrox", storedChampion.Name);

            Assert.Equal(3078, storedItem.RiotItemId);
            Assert.Equal("Trinity Force", storedItem.Name);

            Assert.Equal(8005, storedRune.RiotRuneId);
            Assert.Equal("Precision", storedRune.RuneTreeName);

            Assert.Equal("16.15", storedPatch.Version);
            Assert.Equal("16.15.1", storedPatch.DataDragonVersion);
        }
    }

    [Fact]
    public async Task DuplicateChampionRiotIdThrowsDbUpdateException()
    {
        var first = new Champion(266, "Aatrox");
        var second = new Champion(266, "AnotherAatrox");

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Champions.AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DuplicateItemRiotIdThrowsDbUpdateException()
    {
        var first = new Item(3078, "Trinity Force");
        var second = new Item(3078, "Duplicate Item");

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Items.AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DuplicateRuneRiotIdThrowsDbUpdateException()
    {
        var first = new Rune(
            8005,
            "Press the Attack",
            "Description",
            "/runes/8005.png",
            8000,
            "Precision"
        );

        var second = new Rune(
            8005,
            "Duplicate Rune",
            "Description",
            "/runes/duplicate.png",
            8000,
            "Precision"
        );

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Runes.AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DuplicatePatchVersionThrowsDbUpdateException()
    {
        var first = new Patch(
            "16.15",
            "16.15.1"
        );

        var second = new Patch(
            "16.15",
            "16.15.2"
        );

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Patches.AddRange(first, second);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());
    }
}
