using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class MatchParticipantTests
{
    [Fact]
    public void CreatesMatchParticipantWithValidInformation()
    {
        var matchId = Guid.NewGuid();

        var participant = new MatchParticipant(
            matchId,
            "player-puuid-123",
            1,
            100,
            266,
            "Aatrox",
            "TOP",
            10,
            3,
            7,
            14500,
            13200,
            210,
            18,
            22,
            8,
            3,
            28500,
            24000,
            TimeSpan.FromMinutes(32),
            true);

        Assert.NotEqual(Guid.Empty, participant.Id);
        Assert.Equal(matchId, participant.MatchId);
        Assert.Equal("player-puuid-123", participant.PlayerPuuid);
        Assert.Equal(1, participant.ParticipantId);
        Assert.Equal(100, participant.TeamId);
        Assert.Equal(266, participant.ChampionId);
        Assert.Equal("Aatrox", participant.ChampionName);
        Assert.Equal("TOP", participant.TeamPosition);
        Assert.Equal(10, participant.Kills);
        Assert.Equal(3, participant.Deaths);
        Assert.Equal(7, participant.Assists);
        Assert.Equal(14500, participant.GoldEarned);
        Assert.Equal(13200, participant.GoldSpent);
        Assert.Equal(210, participant.TotalMinionsKilled);
        Assert.Equal(18, participant.NeutralMinionsKilled);
        Assert.Equal(22, participant.VisionScore);
        Assert.Equal(8, participant.WardsPlaced);
        Assert.Equal(3, participant.WardsKilled);
        Assert.Equal(28500, participant.TotalDamageDealtToChampions);
        Assert.Equal(24000, participant.TotalDamageTaken);
        Assert.Equal(TimeSpan.FromMinutes(32), participant.TimePlayed);
        Assert.True(participant.Won);
    }

    [Fact]
    public void ThrowsWhenMatchIdIsMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateParticipant(matchId: Guid.Empty));
    }

    [Fact]
    public void ThrowsWhenPlayerPuuidIsMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateParticipant(playerPuuid: ""));
    }

    [Fact]
    public void ThrowsWhenParticipantIdIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(participantId: 0));
    }

    [Fact]
    public void ThrowsWhenTeamIdIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(teamId: 0));
    }

    [Fact]
    public void ThrowsWhenChampionIdIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(championId: 0));
    }

    [Fact]
    public void ThrowsWhenChampionNameIsMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateParticipant(championName: ""));
    }

    [Fact]
    public void ThrowsWhenKillsAreNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(kills: -1));
    }

    [Fact]
    public void ThrowsWhenDeathsAreNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(deaths: -1));
    }

    [Fact]
    public void ThrowsWhenAssistsAreNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(assists: -1));
    }

    [Fact]
    public void ThrowsWhenGoldEarnedIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(goldEarned: -1));
    }

    [Fact]
    public void ThrowsWhenMinionCountIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(totalMinionsKilled: -1));
    }

    [Fact]
    public void ThrowsWhenVisionScoreIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(visionScore: -1));
    }

    [Fact]
    public void ThrowsWhenDamageIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(totalDamageDealtToChampions: -1));
    }

    [Fact]
    public void ThrowsWhenTimePlayedIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateParticipant(timePlayed: TimeSpan.Zero));
    }

    private static MatchParticipant CreateParticipant(
        Guid? matchId = null,
        string playerPuuid = "player-puuid-123",
        int participantId = 1,
        int teamId = 100,
        int championId = 266,
        string championName = "Aatrox",
        int kills = 10,
        int deaths = 3,
        int assists = 7,
        int goldEarned = 14500,
        int totalMinionsKilled = 210,
        int visionScore = 22,
        int totalDamageDealtToChampions = 28500,
        TimeSpan? timePlayed = null)
    {
        return new MatchParticipant(
            matchId ?? Guid.NewGuid(),
            playerPuuid,
            participantId,
            teamId,
            championId,
            championName,
            "TOP",
            kills,
            deaths,
            assists,
            goldEarned,
            13200,
            totalMinionsKilled,
            18,
            visionScore,
            8,
            3,
            totalDamageDealtToChampions,
            24000,
            timePlayed ?? TimeSpan.FromMinutes(32),
            true);
    }
}
