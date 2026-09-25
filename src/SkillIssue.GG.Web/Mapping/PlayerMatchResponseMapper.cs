using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Models.Matches;

namespace SkillIssue.GG.Web.Mapping;

public static class PlayerMatchResponseMapper
{
    public static PlayerMatchResponse Map(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        return new PlayerMatchResponse(
            RiotMatchId: match.RiotMatchId,
            GameVersion: match.GameVersion,
            GameMode: match.GameMode,
            GameType: match.GameType,
            MapId: match.MapId,
            QueueId: match.QueueId,
            PlatformId: match.PlatformId,
            GameCreatedAt: match.GameCreatedAt,
            StartedAt: match.StartedAt,
            EndedAt: match.EndedAt,
            Duration: match.Duration,
            EndOfGameResult: match.EndOfGameResult,
            Participants: [.. match.Participants.Select(MapParticipant)]);
    }

    private static PlayerMatchParticipantResponse MapParticipant(
        MatchParticipant participant)
    {
        return new PlayerMatchParticipantResponse(
            PlayerPuuid: participant.PlayerPuuid,
            ParticipantId: participant.ParticipantId,
            TeamId: participant.TeamId,
            ChampionId: participant.ChampionId,
            TeamPosition: participant.TeamPosition,
            Kills: participant.Kills,
            Deaths: participant.Deaths,
            Assists: participant.Assists,
            GoldEarned: participant.GoldEarned,
            GoldSpent: participant.GoldSpent,
            TotalMinionsKilled: participant.TotalMinionsKilled,
            NeutralMinionsKilled: participant.NeutralMinionsKilled,
            VisionScore: participant.VisionScore,
            WardsPlaced: participant.WardsPlaced,
            WardsKilled: participant.WardsKilled,
            TotalDamageDealtToChampions:
                participant.TotalDamageDealtToChampions,
            TotalDamageTaken: participant.TotalDamageTaken,
            TimePlayed: participant.TimePlayed,
            Won: participant.Won,
            ItemIds: [.. participant.ItemIds],
            RuneIds: [.. participant.RuneIds]);
    }
}
