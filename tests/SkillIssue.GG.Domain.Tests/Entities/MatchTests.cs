using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class MatchTests
{
    [Fact]
    public void CreatesMatchWithValidInformation()
    {
        var gameCreatedAt = DateTimeOffset.UtcNow.AddMinutes(-35);
        var startedAt = gameCreatedAt.AddMinutes(1);
        var endedAt = startedAt.AddMinutes(30);
        var duration = TimeSpan.FromMinutes(30);

        var match = new Match(
            "EUW1_1234567890",
            1234567890,
            "2",
            "15.17.123.4567",
            "CLASSIC",
            "MATCHED_GAME",
            11,
            410,
            "EUW1",
            gameCreatedAt,
            startedAt,
            endedAt,
            duration,
            "GameComplete");

        Assert.NotEqual(Guid.Empty, match.Id);
        Assert.Equal("EUW1_1234567890", match.RiotMatchId);
        Assert.Equal(1234567890, match.RiotGameId);
        Assert.Equal("2", match.DataVersion);
        Assert.Equal("15.17.123.4567", match.GameVersion);
        Assert.Equal("CLASSIC", match.GameMode);
        Assert.Equal("MATCHED_GAME", match.GameType);
        Assert.Equal(11, match.MapId);
        Assert.Equal(410, match.QueueId);
        Assert.Equal("EUW1", match.PlatformId);
        Assert.Equal(gameCreatedAt, match.GameCreatedAt);
        Assert.Equal(startedAt, match.StartedAt);
        Assert.Equal(endedAt, match.EndedAt);
        Assert.Equal(duration, match.Duration);
        Assert.Equal("GameComplete", match.EndOfGameResult);
    }

    [Fact]
    public void ThrowsWhenRiotMatchIdIsMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateMatch(riotMatchId: ""));
    }

    [Fact]
    public void ThrowsWhenRiotGameIdIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateMatch(riotGameId: 0));
    }

    [Fact]
    public void ThrowsWhenDurationIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateMatch(duration: TimeSpan.Zero));
    }

    [Fact]
    public void ThrowsWhenEndTimeIsBeforeStartTime()
    {
        var startedAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            CreateMatch(
                startedAt: startedAt,
                endedAt: startedAt.AddMinutes(-1)));
    }
    [Fact]
    public void MatchesPatchWhenGameVersionBelongsToPatch()
    {
        var match = CreateMatch();

        var patch = new Patch(
            version: "15.17",
            dataDragonVersion: "15.17.1");

        var matches = match.WasPlayedOnPatch(patch);

        Assert.True(matches);
    }

    [Fact]
    public void DoesNotMatchDifferentPatch()
    {
        var match = CreateMatch();

        var patch = new Patch(
            version: "15.16",
            dataDragonVersion: "15.16.1");

        var matches = match.WasPlayedOnPatch(patch);

        Assert.False(matches);
    }

    [Fact]
    public void ThrowsWhenPatchIsNull()
    {
        var match = CreateMatch();

        Assert.Throws<ArgumentNullException>(() =>
            match.WasPlayedOnPatch(null!));
    }


    [Fact]
    public void AddsParticipantToMatch()
    {
        var match = CreateMatch();
        var participant = CreateParticipant(match.Id);

        match.AddParticipant(participant);

        Assert.Single(match.Participants);
        Assert.Contains(participant, match.Participants);
    }

    [Fact]
    public void ThrowsWhenParticipantBelongsToDifferentMatch()
    {
        var match = CreateMatch();
        var differentMatch = CreateMatch(
            riotMatchId: "EUW1_9876543210",
            riotGameId: 9876543210);

        var participant = CreateParticipant(differentMatch.Id);

        Assert.Throws<ArgumentException>(() =>
            match.AddParticipant(participant));
    }

    [Fact]
    public void ThrowsWhenParticipantIsAddedTwice()
    {
        var match = CreateMatch();
        var participant = CreateParticipant(match.Id);

        match.AddParticipant(participant);

        Assert.Throws<InvalidOperationException>(() =>
            match.AddParticipant(participant));
    }

    [Fact]
    public void ThrowsWhenParticipantIsNull()
    {
        var match = CreateMatch();

        Assert.Throws<ArgumentNullException>(() =>
            match.AddParticipant(null!));
    }

    [Fact]
    public void ThrowsWhenGameCreatedAtIsAfterStartedAt()
    {
        var startedAt = DateTimeOffset.UtcNow;
        var gameCreatedAt = startedAt.AddMinutes(1);
        var endedAt = startedAt.AddMinutes(30);

        Assert.Throws<ArgumentException>(() =>
            new Match(
                "EUW1_1234567890",
                1234567890,
                "2",
                "15.17.123.4567",
                "CLASSIC",
                "MATCHED_GAME",
                11,
                410,
                "EUW1",
                gameCreatedAt,
                startedAt,
                endedAt,
                TimeSpan.FromMinutes(30),
                "GameComplete"));
    }

    [Fact]
    public void AllowsGameCreatedAtBeforeStartedAt()
    {
        var startedAt = DateTimeOffset.UtcNow;
        var gameCreatedAt = startedAt.AddMinutes(-1);
        var endedAt = startedAt.AddMinutes(30);

        var match = new Match(
            "EUW1_1234567890",
            1234567890,
            "2",
            "15.17.123.4567",
            "CLASSIC",
            "MATCHED_GAME",
            11,
            410,
            "EUW1",
            gameCreatedAt,
            startedAt,
            endedAt,
            TimeSpan.FromMinutes(30),
            "GameComplete");

        Assert.Equal(gameCreatedAt, match.GameCreatedAt);
    }

    private static Match CreateMatch(
        string riotMatchId = "EUW1_1234567890",
        long riotGameId = 1234567890,
        TimeSpan? duration = null,
        DateTimeOffset? startedAt = null,
        DateTimeOffset? endedAt = null)
    {
        var matchStartedAt = startedAt ?? DateTimeOffset.UtcNow.AddMinutes(-30);
        var matchEndedAt = endedAt ?? matchStartedAt.AddMinutes(30);

        return new Match(
            riotMatchId,
            riotGameId,
            "2",
            "15.17.123.4567",
            "CLASSIC",
            "MATCHED_GAME",
            11,
            410,
            "EUW1",
            matchStartedAt.AddMinutes(-1),
            matchStartedAt,
            matchEndedAt,
            duration ?? TimeSpan.FromMinutes(30),
            "GameComplete");
    }

    private static MatchParticipant CreateParticipant(
    Guid matchId,
    int participantId = 1)
    {
        return new MatchParticipant(
            matchId: matchId,
            playerPuuid: "test-puuid",
            participantId: participantId,
            teamId: 100,
            championId: 266,
            teamPosition: "TOP",
            kills: 5,
            deaths: 2,
            assists: 7,
            goldEarned: 12000,
            goldSpent: 11000,
            totalMinionsKilled: 180,
            neutralMinionsKilled: 10,
            visionScore: 25,
            wardsPlaced: 8,
            wardsKilled: 3,
            totalDamageDealtToChampions: 22000,
            totalDamageTaken: 18000,
            timePlayed: TimeSpan.FromMinutes(30),
            won: true);
    }
}
