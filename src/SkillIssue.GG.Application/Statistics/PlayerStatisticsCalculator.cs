using SkillIssue.GG.Application.Statistics.Models;

namespace SkillIssue.GG.Application.Statistics;

public static class PlayerStatisticsCalculator
{
    public static PlayerStatistics Calculate(
        IReadOnlyCollection<PlayerMatchStatistics> matches)
    {
        ArgumentNullException.ThrowIfNull(matches);

        if (matches.Count == 0)
        {
            throw new ArgumentException(
                "At least one player match statistic is required.",
                nameof(matches));
        }

        var playerPuuid = matches.First().PlayerPuuid;

        if (matches.Any(x =>
                !string.Equals(
                    x.PlayerPuuid,
                    playerPuuid,
                    StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "All match statistics must belong to the same player.",
                nameof(matches));
        }

        if (matches
            .GroupBy(x => x.MatchId)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Duplicate matches cannot be aggregated.",
                nameof(matches));
        }

        var gamesPlayed = matches.Count;
        var wins = matches.Count(x => x.Won);
        var losses = gamesPlayed - wins;

        var kills = matches.Sum(x => x.Kills);
        var deaths = matches.Sum(x => x.Deaths);
        var assists = matches.Sum(x => x.Assists);

        var totalCs = matches.Sum(x => x.Cs);

        var totalGoldEarned =
            matches.Sum(x => (long)x.GoldEarned);

        var totalParticipantMinutes =
            matches.Sum(x => x.TimePlayed.TotalMinutes);

        var totalGameDuration = TimeSpan.FromTicks(
            matches.Sum(x => x.GameDuration.Ticks));

        var winRate =
            wins / (double)gamesPlayed;

        var kda = deaths == 0
            ? kills + assists
            : (kills + assists) / (double)deaths;

        var averageCs =
            totalCs / (double)gamesPlayed;

        var averageCsPerMinute =
            totalCs / totalParticipantMinutes;

        var averageGoldEarned =
            totalGoldEarned / (double)gamesPlayed;

        var averageGoldPerMinute =
            totalGoldEarned / totalParticipantMinutes;

        var averageGameDuration =
            TimeSpan.FromTicks(
                totalGameDuration.Ticks / gamesPlayed);

        return new PlayerStatistics(
            playerPuuid,
            gamesPlayed,
            wins,
            losses,
            winRate,
            kills,
            deaths,
            assists,
            kda,
            totalCs,
            averageCs,
            averageCsPerMinute,
            totalGoldEarned,
            averageGoldEarned,
            averageGoldPerMinute,
            totalGameDuration,
            averageGameDuration);
    }
}
