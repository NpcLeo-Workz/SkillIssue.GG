using SkillIssue.GG.Domain.Entities;
using SkillIssue.GG.Web.Models.Players;

namespace SkillIssue.GG.Web.Mapping;

public static class PlayerResponseMapper
{
    public static PlayerResponse Map(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new PlayerResponse(
            Id: player.Id,
            Puuid: player.Puuid,
            Name: player.Name,
            Region: player.Region);
    }
}
