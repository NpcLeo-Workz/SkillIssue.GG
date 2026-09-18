using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Infrastructure.Persistence.Repositories;


namespace SkillIssue.GG.Infrastructure.IntegrationTests.Persistence.Repositories;

public sealed class MatchRepositoryTests(PostgreSqlFixture fixture) : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture = fixture;

    [Fact]
    public async Task AddAsync_PersistsMatchAggregate()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var match = CreateMatch();

        await repository.AddAsync(match);

        dbContext.ChangeTracker.Clear();

        var persisted = await dbContext.Matches
            .Include(x => x.Participants)
            .SingleAsync(x => x.Id == match.Id);

        Assert.Equal(match.Id, persisted.Id);
        Assert.Equal(match.RiotMatchId, persisted.RiotMatchId);
        Assert.Equal(match.RiotGameId, persisted.RiotGameId);
        Assert.Equal(match.DataVersion, persisted.DataVersion);
        Assert.Equal(match.GameVersion, persisted.GameVersion);
        Assert.Equal(match.GameMode, persisted.GameMode);
        Assert.Equal(match.GameType, persisted.GameType);
        Assert.Equal(match.MapId, persisted.MapId);
        Assert.Equal(match.QueueId, persisted.QueueId);
        Assert.Equal(match.PlatformId, persisted.PlatformId);
        Assert.Equal(
            match.GameCreatedAt,
            persisted.GameCreatedAt,
            TimeSpan.FromMicroseconds(1));
        Assert.Equal(
            match.StartedAt,
            persisted.StartedAt,
            TimeSpan.FromMicroseconds(1));
        Assert.NotNull(match.EndedAt);
        Assert.NotNull(persisted.EndedAt);
        Assert.Equal(
            match.EndedAt.Value,
            persisted.EndedAt.Value,
            TimeSpan.FromMicroseconds(1));
        Assert.Equal(match.Duration, persisted.Duration);
        Assert.Equal(match.EndOfGameResult, persisted.EndOfGameResult);

        Assert.Single(persisted.Participants);
    }

    [Fact]
    public async Task AddAsync_PersistsParticipantCollections()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var match = CreateMatch();
        var participant = match.Participants.Single();

        await repository.AddAsync(match);

        dbContext.ChangeTracker.Clear();

        var persisted = await dbContext.MatchParticipants
            .SingleAsync(x => x.Id == participant.Id);

        Assert.Equal(
            [1001, 2003, 3006],
            [.. persisted.ItemIds]);

        Assert.Equal(
            [8005, 9111, 9104],
            [.. persisted.RuneIds]);
    }

    [Fact]
    public async Task ExistsByRiotMatchIdAsync_ReturnsFalse_WhenMatchDoesNotExist()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var exists = await repository.ExistsByRiotMatchIdAsync(
            $"EUW1_{Guid.NewGuid():N}");

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByRiotMatchIdAsync_ReturnsTrue_WhenMatchExists()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var match = CreateMatch();

        await repository.AddAsync(match);

        var exists = await repository.ExistsByRiotMatchIdAsync(
            match.RiotMatchId);

        Assert.True(exists);
    }

    [Fact]
    public async Task AddAsync_PreservesNullableCompletionFields()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

        var match = new Match(
            riotMatchId: $"EUW1_{Guid.NewGuid():N}",
            riotGameId: Random.Shared.NextInt64(1, long.MaxValue),
            dataVersion: "16.15.1",
            gameVersion: "16.15.123.4567",
            gameMode: "CLASSIC",
            gameType: "MATCHED_GAME",
            mapId: 11,
            queueId: 420,
            platformId: "EUW1",
            gameCreatedAt: startedAt.AddMinutes(-1),
            startedAt: startedAt,
            endedAt: null,
            duration: TimeSpan.FromMinutes(10),
            endOfGameResult: null);

        await repository.AddAsync(match);

        dbContext.ChangeTracker.Clear();

        var persisted = await dbContext.Matches
            .SingleAsync(x => x.Id == match.Id);

        Assert.Null(persisted.EndedAt);
        Assert.Null(persisted.EndOfGameResult);
    }

    [Fact]
    public async Task AddAsync_PersistsMultipleParticipants()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var match = CreateMatch();

        var secondParticipant = new MatchParticipant(
            matchId: match.Id,
            playerPuuid: $"puuid-{Guid.NewGuid():N}",
            participantId: 2,
            teamId: 200,
            championId: 103,
            teamPosition: "MIDDLE",
            kills: 3,
            deaths: 4,
            assists: 9,
            goldEarned: 11000,
            goldSpent: 10500,
            totalMinionsKilled: 165,
            neutralMinionsKilled: 8,
            visionScore: 20,
            wardsPlaced: 6,
            wardsKilled: 2,
            totalDamageDealtToChampions: 15000,
            totalDamageTaken: 14000,
            timePlayed: TimeSpan.FromMinutes(25),
            won: false);

        match.AddParticipant(secondParticipant);

        await repository.AddAsync(match);

        dbContext.ChangeTracker.Clear();

        var persisted = await dbContext.Matches
            .Include(x => x.Participants)
            .SingleAsync(x => x.Id == match.Id);

        Assert.Equal(2, persisted.Participants.Count);

        Assert.Contains(
            persisted.Participants,
            x => x.ParticipantId == 1);

        Assert.Contains(
            persisted.Participants,
            x => x.ParticipantId == 2);
    }

    [Fact]
    public async Task AddAsync_PersistsEmptyItemAndRuneCollections()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-30);

        var match = new Match(
            riotMatchId: $"EUW1_{Guid.NewGuid():N}",
            riotGameId: Random.Shared.NextInt64(1, long.MaxValue),
            dataVersion: "16.15.1",
            gameVersion: "16.15.123.4567",
            gameMode: "CLASSIC",
            gameType: "MATCHED_GAME",
            mapId: 11,
            queueId: 420,
            platformId: "EUW1",
            gameCreatedAt: startedAt.AddMinutes(-1),
            startedAt: startedAt,
            endedAt: startedAt.AddMinutes(25),
            duration: TimeSpan.FromMinutes(25),
            endOfGameResult: "GameComplete");

        var participant = new MatchParticipant(
            matchId: match.Id,
            playerPuuid: $"puuid-{Guid.NewGuid():N}",
            participantId: 1,
            teamId: 100,
            championId: 266,
            teamPosition: "TOP",
            kills: 1,
            deaths: 1,
            assists: 1,
            goldEarned: 10000,
            goldSpent: 9500,
            totalMinionsKilled: 150,
            neutralMinionsKilled: 5,
            visionScore: 15,
            wardsPlaced: 4,
            wardsKilled: 1,
            totalDamageDealtToChampions: 10000,
            totalDamageTaken: 12000,
            timePlayed: TimeSpan.FromMinutes(25),
            won: true);

        match.AddParticipant(participant);

        await repository.AddAsync(match);

        dbContext.ChangeTracker.Clear();

        var persisted = await dbContext.MatchParticipants
            .SingleAsync(x => x.Id == participant.Id);

        Assert.Empty(persisted.ItemIds);
        Assert.Empty(persisted.RuneIds);
    }

    [Fact]
    public async Task AddAsync_ThrowsDbUpdateException_WhenRiotMatchIdAlreadyExists()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        var first = CreateMatch();
        await repository.AddAsync(first);

        var duplicate = CreateMatchWithRiotMatchId(
            first.RiotMatchId);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => repository.AddAsync(duplicate));
    }

    [Fact]
    public async Task ExistsByRiotMatchIdAsync_ThrowsOperationCanceledException_WhenCancelled()
    {
        await using var dbContext = _fixture.CreateDbContext();
        var repository = new MatchRepository(dbContext);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => repository.ExistsByRiotMatchIdAsync(
                $"EUW1_{Guid.NewGuid():N}",
                cancellationTokenSource.Token));
    }

    private static Match CreateMatch()
    {
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-30);

        var match = new Match(
            riotMatchId: $"EUW1_{Guid.NewGuid():N}",
            riotGameId: Random.Shared.NextInt64(1, long.MaxValue),
            dataVersion: "16.15.1",
            gameVersion: "16.15.123.4567",
            gameMode: "CLASSIC",
            gameType: "MATCHED_GAME",
            mapId: 11,
            queueId: 420,
            platformId: "EUW1",
            gameCreatedAt: startedAt.AddMinutes(-1),
            startedAt: startedAt,
            endedAt: startedAt.AddMinutes(25),
            duration: TimeSpan.FromMinutes(25),
            endOfGameResult: "GameComplete");

        var participant = new MatchParticipant(
            matchId: match.Id,
            playerPuuid: $"puuid-{Guid.NewGuid():N}",
            participantId: 1,
            teamId: 100,
            championId: 266,
            teamPosition: "TOP",
            kills: 5,
            deaths: 2,
            assists: 7,
            goldEarned: 12000,
            goldSpent: 11500,
            totalMinionsKilled: 180,
            neutralMinionsKilled: 12,
            visionScore: 25,
            wardsPlaced: 8,
            wardsKilled: 3,
            totalDamageDealtToChampions: 18000,
            totalDamageTaken: 22000,
            timePlayed: TimeSpan.FromMinutes(25),
            won: true);

        participant.AddItem(1001);
        participant.AddItem(2003);
        participant.AddItem(3006);

        participant.AddRune(8005);
        participant.AddRune(9111);
        participant.AddRune(9104);

        match.AddParticipant(participant);

        return match;
    }

    private static Match CreateMatchWithRiotMatchId(
    string riotMatchId)
    {
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-30);

        return new Match(
            riotMatchId: riotMatchId,
            riotGameId: Random.Shared.NextInt64(1, long.MaxValue),
            dataVersion: "16.15.1",
            gameVersion: "16.15.123.4567",
            gameMode: "CLASSIC",
            gameType: "MATCHED_GAME",
            mapId: 11,
            queueId: 420,
            platformId: "EUW1",
            gameCreatedAt: startedAt.AddMinutes(-1),
            startedAt: startedAt,
            endedAt: startedAt.AddMinutes(25),
            duration: TimeSpan.FromMinutes(25),
            endOfGameResult: "GameComplete");
    }
}
