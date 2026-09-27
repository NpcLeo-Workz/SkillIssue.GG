using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Riot.Mapping;

public static class RiotMatchDomainMapper
{
    public static Match Map(RiotMatchDetails source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var match = new Match(
            source.RiotMatchId,
            source.RiotGameId,
            source.DataVersion,
            source.GameVersion,
            source.GameMode,
            source.GameType,
            source.MapId,
            source.QueueId,
            source.PlatformId,
            source.GameCreatedAt,
            source.StartedAt,
            source.EndedAt,
            source.Duration,
            source.EndOfGameResult);

        foreach (var sourceParticipant in source.Participants)
        {
            var participant = MapParticipant(
                match.Id,
                sourceParticipant);

            foreach (var itemId in sourceParticipant.ItemIds)
            {
                participant.AddItem(itemId);
            }

            foreach (var runeId in sourceParticipant.RuneIds)
            {
                participant.AddRune(runeId);
            }

            match.AddParticipant(participant);
        }

        return match;
    }

    private static MatchParticipant MapParticipant(
        Guid matchId,
        RiotMatchParticipant source)
    {
        return new MatchParticipant(
            matchId,
            source.Puuid,
            source.ParticipantId,
            source.TeamId,
            source.ChampionId,
            source.TeamPosition,
            source.Kills,
            source.Deaths,
            source.Assists,
            source.GoldEarned,
            source.GoldSpent,
            source.TotalMinionsKilled,
            source.NeutralMinionsKilled,
            source.VisionScore,
            source.WardsPlaced,
            source.WardsKilled,
            source.TotalDamageDealtToChampions,
            source.TotalDamageTaken,
            source.TimePlayed,
            source.Won);
    }
}
