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
    [Fact]
    public void MatchesChampionWhenRiotChampionIdMatches()
    {
        var participant = CreateParticipant(championId: 266);

        var champion = new Champion(
            riotChampionId: 266,
            name: "Aatrox");

        var matches = participant.PlayedChampion(champion);

        Assert.True(matches);
    }

    [Fact]
    public void DoesNotMatchChampionWhenRiotChampionIdDiffers()
    {
        var participant = CreateParticipant(championId: 266);

        var champion = new Champion(
            riotChampionId: 103,
            name: "Ahri");

        var matches = participant.PlayedChampion(champion);

        Assert.False(matches);
    }

    [Fact]
    public void ThrowsWhenChampionIsNull()
    {
        var participant = CreateParticipant();

        Assert.Throws<ArgumentNullException>(() =>
            participant.PlayedChampion(null!));
    }

    [Fact]
    public void AddsItemId()
    {
        var participant = CreateParticipant();

        participant.AddItem(3078);

        Assert.Single(participant.ItemIds);
        Assert.Contains(3078, participant.ItemIds);
    }

    [Fact]
    public void AllowsMultipleItemIds()
    {
        var participant = CreateParticipant();

        participant.AddItem(3078);
        participant.AddItem(3047);
        participant.AddItem(3053);

        Assert.Equal(3, participant.ItemIds.Count);
        Assert.Contains(3078, participant.ItemIds);
        Assert.Contains(3047, participant.ItemIds);
        Assert.Contains(3053, participant.ItemIds);
    }

    [Fact]
    public void IgnoresEmptyItemSlot()
    {
        var participant = CreateParticipant();

        participant.AddItem(0);

        Assert.Empty(participant.ItemIds);
    }

    [Fact]
    public void ThrowsWhenItemIdIsNegative()
    {
        var participant = CreateParticipant();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            participant.AddItem(-1));
    }
    [Fact]
    public void AddsRuneId()
    {
        var participant = CreateParticipant();

        participant.AddRune(8005);

        Assert.Single(participant.RuneIds);
        Assert.Contains(8005, participant.RuneIds);
    }

    [Fact]
    public void AllowsMultipleRuneIds()
    {
        var participant = CreateParticipant();

        participant.AddRune(8005);
        participant.AddRune(9111);
        participant.AddRune(9104);

        Assert.Equal(3, participant.RuneIds.Count);
        Assert.Contains(8005, participant.RuneIds);
        Assert.Contains(9111, participant.RuneIds);
        Assert.Contains(9104, participant.RuneIds);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThrowsWhenRuneIdIsInvalid(int riotRuneId)
    {
        var participant = CreateParticipant();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            participant.AddRune(riotRuneId));
    }

    [Fact]
    public void ThrowsWhenRuneIsAddedTwice()
    {
        var participant = CreateParticipant();

        participant.AddRune(8005);

        Assert.Throws<InvalidOperationException>(() =>
            participant.AddRune(8005));
    }

    private static MatchParticipant CreateParticipant(
        Guid? matchId = null,
        string playerPuuid = "player-puuid-123",
        int participantId = 1,
        int teamId = 100,
        int championId = 266,
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
