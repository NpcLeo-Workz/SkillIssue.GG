using SkillIssue.GG.Application.Statistics;
using SkillIssue.GG.Application.Statistics.Models;

namespace SkillIssue.GG.Application.Tests.Statistics;

public sealed class PlayerStatisticsCalculatorTests
{
    [Fact]
    public void Calculate_CalculatesAggregatePlayerStatistics()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: true,
                kills: 10,
                deaths: 2,
                assists: 8,
                cs: 100,
                goldEarned: 6000,
                timePlayed: TimeSpan.FromMinutes(10),
                gameDuration: TimeSpan.FromMinutes(12)),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: false,
                kills: 5,
                deaths: 3,
                assists: 7,
                cs: 200,
                goldEarned: 12000,
                timePlayed: TimeSpan.FromMinutes(30),
                gameDuration: TimeSpan.FromMinutes(32))
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal("test-puuid", result.PlayerPuuid);

        Assert.Equal(2, result.GamesPlayed);
        Assert.Equal(1, result.Wins);
        Assert.Equal(1, result.Losses);
        Assert.Equal(0.5, result.WinRate);

        Assert.Equal(15, result.Kills);
        Assert.Equal(5, result.Deaths);
        Assert.Equal(15, result.Assists);
        Assert.Equal(6.0, result.Kda);

        Assert.Equal(300, result.TotalCs);
        Assert.Equal(150.0, result.AverageCs);

        Assert.Equal(
            7.5,
            result.AverageCsPerMinute);

        Assert.Equal(18000L, result.TotalGoldEarned);
        Assert.Equal(9000.0, result.AverageGoldEarned);

        Assert.Equal(
            450.0,
            result.AverageGoldPerMinute);

        Assert.Equal(
            TimeSpan.FromMinutes(44),
            result.TotalGameDuration);

        Assert.Equal(
            TimeSpan.FromMinutes(22),
            result.AverageGameDuration);
    }

    [Fact]
    public void Calculate_CalculatesWinsLossesAndWinRate()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: true),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: true),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: false),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: false)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(4, result.GamesPlayed);
        Assert.Equal(2, result.Wins);
        Assert.Equal(2, result.Losses);
        Assert.Equal(0.5, result.WinRate);
    }

    [Fact]
    public void Calculate_WhenAllMatchesAreWins_ReturnsWinRateOfOne()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: true),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: true)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(2, result.Wins);
        Assert.Equal(0, result.Losses);
        Assert.Equal(1.0, result.WinRate);
    }

    [Fact]
    public void Calculate_WhenAllMatchesAreLosses_ReturnsWinRateOfZero()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: false),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: false)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(0, result.Wins);
        Assert.Equal(2, result.Losses);
        Assert.Equal(0.0, result.WinRate);
    }

    [Fact]
    public void Calculate_UsesAggregateTotalsForKda()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                kills: 10,
                deaths: 1,
                assists: 0),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                kills: 0,
                deaths: 9,
                assists: 10)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(10, result.Kills);
        Assert.Equal(10, result.Deaths);
        Assert.Equal(10, result.Assists);

        // (10 kills + 10 assists) / 10 deaths
        Assert.Equal(2.0, result.Kda);
    }

    [Fact]
    public void Calculate_WhenTotalDeathsAreZero_UsesKillsPlusAssistsForKda()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                kills: 10,
                deaths: 0,
                assists: 5),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                kills: 4,
                deaths: 0,
                assists: 6)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(14, result.Kills);
        Assert.Equal(0, result.Deaths);
        Assert.Equal(11, result.Assists);
        Assert.Equal(25.0, result.Kda);
    }

    [Fact]
    public void Calculate_CalculatesTotalAndAverageCs()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                cs: 100),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                cs: 200),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                cs: 300)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(600, result.TotalCs);
        Assert.Equal(200.0, result.AverageCs);
    }

    [Fact]
    public void Calculate_UsesWeightedCsPerMinute()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                cs: 100,
                timePlayed: TimeSpan.FromMinutes(10)),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                cs: 200,
                timePlayed: TimeSpan.FromMinutes(30))
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        // 300 total CS / 40 total participant minutes
        Assert.Equal(7.5, result.AverageCsPerMinute);

        // A simple average of the individual rates would be:
        // (10 + 6.666...) / 2 = 8.333...
        Assert.NotEqual(
            (matches[0].CsPerMinute +
             matches[1].CsPerMinute) / 2.0,
            result.AverageCsPerMinute);
    }

    [Fact]
    public void Calculate_CalculatesTotalAndAverageGoldEarned()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                goldEarned: 6000),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                goldEarned: 12000)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(18000L, result.TotalGoldEarned);
        Assert.Equal(9000.0, result.AverageGoldEarned);
    }

    [Fact]
    public void Calculate_UsesWeightedGoldPerMinute()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                goldEarned: 6000,
                timePlayed: TimeSpan.FromMinutes(10)),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                goldEarned: 12000,
                timePlayed: TimeSpan.FromMinutes(30))
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        // 18,000 total gold / 40 total participant minutes
        Assert.Equal(450.0, result.AverageGoldPerMinute);

        // A simple average would incorrectly produce:
        // (600 + 400) / 2 = 500
        Assert.NotEqual(
            (matches[0].GoldPerMinute +
             matches[1].GoldPerMinute) / 2.0,
            result.AverageGoldPerMinute);
    }

    [Fact]
    public void Calculate_CalculatesTotalAndAverageGameDuration()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                gameDuration: TimeSpan.FromMinutes(20)),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                gameDuration: TimeSpan.FromMinutes(40))
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(
            TimeSpan.FromMinutes(60),
            result.TotalGameDuration);

        Assert.Equal(
            TimeSpan.FromMinutes(30),
            result.AverageGameDuration);
    }

    [Fact]
    public void Calculate_AcceptsMatchesForSamePlayer()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                playerPuuid: "player-one"),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                playerPuuid: "player-one")
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal("player-one", result.PlayerPuuid);
        Assert.Equal(2, result.GamesPlayed);
    }

    [Fact]
    public void Calculate_ThrowsWhenPlayersAreDifferent()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                playerPuuid: "player-one"),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                playerPuuid: "player-two")
        };

        var exception = Assert.Throws<ArgumentException>(
            () => PlayerStatisticsCalculator.Calculate(matches));

        Assert.Equal(
            "matches",
            exception.ParamName);
    }

    [Fact]
    public void Calculate_ThrowsWhenMatchIdsAreDuplicated()
    {
        var matchId = Guid.NewGuid();

        var matches = new[]
        {
            CreateStatistics(
                matchId: matchId),

            CreateStatistics(
                matchId: matchId)
        };

        var exception = Assert.Throws<ArgumentException>(
            () => PlayerStatisticsCalculator.Calculate(matches));

        Assert.Equal(
            "matches",
            exception.ParamName);
    }

    [Fact]
    public void Calculate_ThrowsWhenCollectionIsEmpty()
    {
        PlayerMatchStatistics[] matches = [];

        var exception = Assert.Throws<ArgumentException>(
            () => PlayerStatisticsCalculator.Calculate(matches));

        Assert.Equal(
            "matches",
            exception.ParamName);
    }

    [Fact]
    public void Calculate_ThrowsWhenMatchesIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => PlayerStatisticsCalculator.Calculate(null!));
    }

    [Fact]
    public void Calculate_DoesNotUseIntegerDivision()
    {
        var matches = new[]
        {
            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: true,
                kills: 1,
                deaths: 2,
                assists: 0,
                cs: 100,
                goldEarned: 1000),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: false,
                kills: 0,
                deaths: 1,
                assists: 0,
                cs: 101,
                goldEarned: 1001),

            CreateStatistics(
                matchId: Guid.NewGuid(),
                won: false,
                kills: 0,
                deaths: 0,
                assists: 0,
                cs: 100,
                goldEarned: 1000)
        };

        var result =
            PlayerStatisticsCalculator.Calculate(matches);

        Assert.Equal(
            1.0 / 3.0,
            result.WinRate);

        Assert.Equal(
            1.0 / 3.0,
            result.Kda);

        Assert.Equal(
            301.0 / 3.0,
            result.AverageCs);

        Assert.Equal(
            3001.0 / 3.0,
            result.AverageGoldEarned);
    }

    private static PlayerMatchStatistics CreateStatistics(
        Guid matchId,
        string playerPuuid = "test-puuid",
        bool won = true,
        int kills = 10,
        int deaths = 2,
        int assists = 8,
        int cs = 200,
        int goldEarned = 12000,
        TimeSpan? timePlayed = null,
        TimeSpan? gameDuration = null)
    {
        var participantTime =
            timePlayed ?? TimeSpan.FromMinutes(30);

        var duration =
            gameDuration ?? TimeSpan.FromMinutes(30);

        var kda = deaths == 0
            ? kills + assists
            : (kills + assists) / (double)deaths;

        var csPerMinute =
            cs / participantTime.TotalMinutes;

        var goldPerMinute =
            goldEarned / participantTime.TotalMinutes;

        return new PlayerMatchStatistics(
            MatchId: matchId,
            RiotMatchId: $"EUW1_{matchId:N}",
            PlayerPuuid: playerPuuid,
            ChampionId: 266,
            Won: won,
            Kills: kills,
            Deaths: deaths,
            Assists: assists,
            Kda: kda,
            Cs: cs,
            CsPerMinute: csPerMinute,
            GoldEarned: goldEarned,
            GoldPerMinute: goldPerMinute,
            TimePlayed: participantTime,
            GameDuration: duration);
    }
}
