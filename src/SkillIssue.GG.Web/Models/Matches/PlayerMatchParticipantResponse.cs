namespace SkillIssue.GG.Web.Models.Matches;

public sealed record PlayerMatchParticipantResponse(
    string PlayerPuuid,
    int ParticipantId,
    int TeamId,
    int ChampionId,
    string TeamPosition,
    int Kills,
    int Deaths,
    int Assists,
    int GoldEarned,
    int GoldSpent,
    int TotalMinionsKilled,
    int NeutralMinionsKilled,
    int VisionScore,
    int WardsPlaced,
    int WardsKilled,
    int TotalDamageDealtToChampions,
    int TotalDamageTaken,
    TimeSpan TimePlayed,
    bool Won,
    IReadOnlyList<int> ItemIds,
    IReadOnlyList<int> RuneIds);
