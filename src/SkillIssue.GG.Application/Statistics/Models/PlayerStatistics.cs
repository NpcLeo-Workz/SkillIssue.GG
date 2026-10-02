namespace SkillIssue.GG.Application.Statistics.Models;

public sealed record PlayerStatistics(
    string PlayerPuuid,
    int GamesPlayed,
    int Wins,
    int Losses,
    double WinRate,
    int Kills,
    int Deaths,
    int Assists,
    double Kda,
    int TotalCs,
    double AverageCs,
    double AverageCsPerMinute,
    long TotalGoldEarned,
    double AverageGoldEarned,
    double AverageGoldPerMinute,
    TimeSpan TotalGameDuration,
    TimeSpan AverageGameDuration);
