namespace SkillIssue.GG.Application.Statistics.Models;

public sealed record PlayerMatchStatistics(
    Guid MatchId,
    string RiotMatchId,
    string PlayerPuuid,
    int ChampionId,
    bool Won,
    int Kills,
    int Deaths,
    int Assists,
    double Kda,
    int Cs,
    double CsPerMinute,
    int GoldEarned,
    double GoldPerMinute,
    TimeSpan TimePlayed,
    TimeSpan GameDuration);
