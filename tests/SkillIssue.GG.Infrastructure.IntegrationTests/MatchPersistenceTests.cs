using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class MatchPersistenceTests
{
    private readonly PostgreSqlFixture _fixture;

    public MatchPersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanPersistMatchWithParticipant()
    {
        var match = new Match(
            "EUW1_1234567890",
            1234567890,
            "2",
            "16.15.1.1234",
            "CLASSIC",
            "MATCHED_GAME",
            11,
            420,
            "EUW1",
            DateTimeOffset.UtcNow.AddMinutes(-35),
            DateTimeOffset.UtcNow.AddMinutes(-30),
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(30),
            "GameComplete"
        );

        var participant = new MatchParticipant(
            match.Id,
            "test-player-puuid",
            1,
            100,
            266,
            "TOP",
            5,
            2,
            7,
            12000,
            11000,
            180,
            10,
            20,
            2,
            1,
            21000,
            22000,
            TimeSpan.FromMinutes(30),
            true
        );

        participant.AddItem(3078);
        participant.AddItem(3047);
        participant.AddItem(3078);

        participant.AddRune(8005);
        participant.AddRune(9111);

        match.AddParticipant(participant);

        await using (var dbContext = _fixture.CreateDbContext())
        {
            dbContext.Matches.Add(match);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var storedMatch = await dbContext.Matches
                .Include(x => x.Participants)
                .SingleAsync(x => x.Id == match.Id);

            var storedParticipant = Assert.Single(storedMatch.Participants);

            Assert.Equal("test-player-puuid", storedParticipant.PlayerPuuid);
            Assert.Equal(266, storedParticipant.ChampionId);

            Assert.Equal(
                [3078, 3047, 3078],
                storedParticipant.ItemIds);

            Assert.Equal(
                [8005, 9111],
                storedParticipant.RuneIds);
        }
    }

    [Fact]
    public async Task DeletingMatchCascadesToParticipants()
    {
        var match = new Match(
            "EUW1_9876543210",
            9876543210,
            "2",
            "16.15.1.1234",
            "CLASSIC",
            "MATCHED_GAME",
            11,
            420,
            "EUW1",
            DateTimeOffset.UtcNow.AddMinutes(-35),
            DateTimeOffset.UtcNow.AddMinutes(-30),
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(30),
            "GameComplete"
        );

        var participant = new MatchParticipant(
            match.Id,
            "cascade-test-puuid",
            1,
            100,
            266,
            "TOP",
            5,
            2,
            7,
            12000,
            11000,
            180,
            10,
            20,
            2,
            1,
            21000,
            22000,
            TimeSpan.FromMinutes(30),
            true
        );

        match.AddParticipant(participant);

        await using (var dbContext = _fixture.CreateDbContext())
        {
            dbContext.Matches.Add(match);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var storedMatch = await dbContext.Matches
                .SingleAsync(x => x.Id == match.Id);

            dbContext.Matches.Remove(storedMatch);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var participantExists = await dbContext.MatchParticipants
                .AnyAsync(x => x.MatchId == match.Id);

            Assert.False(participantExists);
        }
    }

    [Fact]
    public async Task DuplicateParticipantIdWithinSameMatchThrowsDbUpdateException()
    {
        var match = new Match(
            "EUW1_5555555555",
            5555555555,
            "2",
            "16.15.1.1234",
            "CLASSIC",
            "MATCHED_GAME",
            11,
            420,
            "EUW1",
            DateTimeOffset.UtcNow.AddMinutes(-35),
            DateTimeOffset.UtcNow.AddMinutes(-30),
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(30),
            "GameComplete"
        );

        var firstParticipant = new MatchParticipant(
            match.Id,
            "duplicate-participant-one",
            1,
            100,
            266,
            "TOP",
            5,
            2,
            7,
            12000,
            11000,
            180,
            10,
            20,
            2,
            1,
            21000,
            22000,
            TimeSpan.FromMinutes(30),
            true
        );

        var secondParticipant = new MatchParticipant(
            match.Id,
            "duplicate-participant-two",
            1,
            200,
            103,
            "MIDDLE",
            3,
            4,
            6,
            10500,
            9800,
            160,
            5,
            18,
            1,
            1,
            18000,
            19000,
            TimeSpan.FromMinutes(30),
            false
        );

        await using var dbContext = _fixture.CreateDbContext();

        dbContext.Matches.Add(match);
        dbContext.MatchParticipants.Add(firstParticipant);
        dbContext.MatchParticipants.Add(secondParticipant);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task CanPersistMatchWithNullableEndFields()
    {
        var match = new Match(
            "EUW1_4444444444",
            4444444444,
            "2",
            "16.15.1.1234",
            "CLASSIC",
            "MATCHED_GAME",
            11,
            420,
            "EUW1",
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddMinutes(-5),
            null,
            TimeSpan.FromMinutes(5),
            null
        );

        await using (var dbContext = _fixture.CreateDbContext())
        {
            dbContext.Matches.Add(match);
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = _fixture.CreateDbContext())
        {
            var storedMatch = await dbContext.Matches
                .SingleAsync(x => x.Id == match.Id);

            Assert.Null(storedMatch.EndedAt);
            Assert.Null(storedMatch.EndOfGameResult);
        }
    }
}
