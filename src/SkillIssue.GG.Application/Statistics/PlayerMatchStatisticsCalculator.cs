using SkillIssue.GG.Application.Statistics.Models;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Statistics;

public static class PlayerMatchStatisticsCalculator
{
    public static PlayerMatchStatistics Calculate(
        Match match,
        MatchParticipant participant)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(participant);

        if (participant.MatchId != match.Id)
        {
            throw new ArgumentException(
                "Participant belongs to a different match.",
                nameof(participant));
        }

        var cs =
            participant.TotalMinionsKilled +
            participant.NeutralMinionsKilled;

        var minutesPlayed =
            participant.TimePlayed.TotalMinutes;

        var kda = participant.Deaths == 0
            ? participant.Kills + participant.Assists
            : (participant.Kills + participant.Assists) /
              (double)participant.Deaths;

        var csPerMinute =
            cs / minutesPlayed;

        var goldPerMinute =
            participant.GoldEarned / minutesPlayed;

        return new PlayerMatchStatistics(
            match.Id,
            match.RiotMatchId,
            participant.PlayerPuuid,
            participant.ChampionId,
            participant.Won,
            participant.Kills,
            participant.Deaths,
            participant.Assists,
            kda,
            cs,
            csPerMinute,
            participant.GoldEarned,
            goldPerMinute,
            participant.TimePlayed,
            match.Duration);
    }
}
