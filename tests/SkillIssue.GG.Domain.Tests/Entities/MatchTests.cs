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
}
