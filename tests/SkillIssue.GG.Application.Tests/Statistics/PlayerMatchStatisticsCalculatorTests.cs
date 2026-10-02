using SkillIssue.GG.Application.Statistics;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Tests.Statistics;

public sealed class PlayerMatchStatisticsCalculatorTests
{
    [Fact]
    public void Calculate_CalculatesPlayerMatchStatistics()
    {
        var match = CreateMatch();

        var participant = CreateParticipant(
            match.Id,
            kills: 10,
            deaths: 2,
            assists: 8,
            totalMinionsKilled: 180,
            neutralMinionsKilled: 20,
            goldEarned: 12000,
            timePlayed: TimeSpan.FromMinutes(30),
            won: true);

        var result =
            PlayerMatchStatisticsCalculator.Calculate(
                match,
                participant);

        Assert.Equal(match.Id, result.MatchId);
        Assert.Equal(match.RiotMatchId, result.RiotMatchId);
        Assert.Equal("test-puuid", result.PlayerPuuid);
        Assert.Equal(266, result.ChampionId);

        Assert.True(result.Won);

        Assert.Equal(10, result.Kills);
        Assert.Equal(2, result.Deaths);
        Assert.Equal(8, result.Assists);

        Assert.Equal(9.0, result.Kda);

        Assert.Equal(200, result.Cs);
        Assert.Equal(200.0 / 30.0, result.CsPerMinute);

        Assert.Equal(12000, result.GoldEarned);
        Assert.Equal(400.0, result.GoldPerMinute);
        Assert.Equal(participant.TimePlayed, result.TimePlayed);
        Assert.Equal(match.Duration, result.GameDuration);
    }

    [Fact]
    public void Calculate_WhenDeathsAreZero_UsesKillsPlusAssistsForKda()
    {
        var match = CreateMatch();

        var participant = CreateParticipant(
            match.Id,
            kills: 10,
            deaths: 0,
            assists: 5);

        var result =
            PlayerMatchStatisticsCalculator.Calculate(
                match,
                participant);

        Assert.Equal(15.0, result.Kda);
    }

    [Fact]
    public void Calculate_UsesParticipantTimePlayedForPerMinuteStatistics()
    {
        var match = CreateMatch(
            duration: TimeSpan.FromMinutes(40));

        var participant = CreateParticipant(
            match.Id,
            totalMinionsKilled: 150,
            neutralMinionsKilled: 30,
            goldEarned: 9000,
            timePlayed: TimeSpan.FromMinutes(30));

        var result =
            PlayerMatchStatisticsCalculator.Calculate(
                match,
                participant);

        Assert.Equal(6.0, result.CsPerMinute);
        Assert.Equal(300.0, result.GoldPerMinute);

        Assert.Equal(
            TimeSpan.FromMinutes(40),
            result.GameDuration);
    }

    [Fact]
    public void Calculate_ThrowsWhenParticipantBelongsToDifferentMatch()
    {
        var match = CreateMatch();
        var otherMatch = CreateMatch();

        var participant =
            CreateParticipant(otherMatch.Id);

        Assert.Throws<ArgumentException>(
            () => PlayerMatchStatisticsCalculator.Calculate(
                match,
                participant));
    }

    [Fact]
    public void Calculate_ThrowsWhenMatchIsNull()
    {
        var match = CreateMatch();
        var participant = CreateParticipant(match.Id);

        Assert.Throws<ArgumentNullException>(
            () => PlayerMatchStatisticsCalculator.Calculate(
                null!,
                participant));
    }

    [Fact]
    public void Calculate_ThrowsWhenParticipantIsNull()
    {
        var match = CreateMatch();

        Assert.Throws<ArgumentNullException>(
            () => PlayerMatchStatisticsCalculator.Calculate(
                match,
                null!));
    }

    private static Match CreateMatch(
    TimeSpan? duration = null)
    {
        var startedAt =
            DateTimeOffset.FromUnixTimeMilliseconds(
                1722500010000);

        return new Match(
            riotMatchId: $"EUW1_{Guid.NewGuid():N}",
            riotGameId: Random.Shared.NextInt64(1, long.MaxValue),
            dataVersion: "2",
            gameVersion: "16.15.123.4567",
            gameMode: "CLASSIC",
            gameType: "MATCHED_GAME",
            mapId: 11,
            queueId: 420,
            platformId: "EUW1",
            gameCreatedAt: startedAt.AddSeconds(-10),
            startedAt: startedAt,
            endedAt: startedAt.Add(duration ?? TimeSpan.FromMinutes(30)),
            duration: duration ?? TimeSpan.FromMinutes(30),
            endOfGameResult: "GameComplete");
    }

    private static MatchParticipant CreateParticipant(
    Guid matchId,
    int kills = 10,
    int deaths = 2,
    int assists = 8,
    int totalMinionsKilled = 180,
    int neutralMinionsKilled = 20,
    int goldEarned = 12000,
    TimeSpan? timePlayed = null,
    bool won = true)
    {
        return new MatchParticipant(
            matchId: matchId,
            playerPuuid: "test-puuid",
            participantId: 1,
            teamId: 100,
            championId: 266,
            teamPosition: "TOP",
            kills: kills,
            deaths: deaths,
            assists: assists,
            goldEarned: goldEarned,
            goldSpent: 11000,
            totalMinionsKilled: totalMinionsKilled,
            neutralMinionsKilled: neutralMinionsKilled,
            visionScore: 20,
            wardsPlaced: 8,
            wardsKilled: 2,
            totalDamageDealtToChampions: 18000,
            totalDamageTaken: 22000,
            timePlayed: timePlayed ?? TimeSpan.FromMinutes(30),
            won: won);
    }
}
