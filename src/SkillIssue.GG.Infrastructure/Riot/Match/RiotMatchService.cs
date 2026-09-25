using System.Text.Json;
using Microsoft.Extensions.Options;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Infrastructure.Riot.Configuration;
using SkillIssue.GG.Infrastructure.Riot.Http;
using SkillIssue.GG.Infrastructure.Riot.Match.Dto;

namespace SkillIssue.GG.Infrastructure.Riot.Match;

public sealed class RiotMatchService(RiotApiClient riotApiClient, IOptions<RiotApiOptions> options) : IRiotMatchService
{
    private readonly RiotApiClient _riotApiClient = riotApiClient;
    private readonly RiotApiOptions _options = options.Value;

    public async Task<RiotMatchDetails> GetMatchAsync(
        string matchId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(matchId);

        var encodedMatchId = Uri.EscapeDataString(matchId);

        var requestUri =
            $"https://{_options.RegionalRoute}.api.riotgames.com" +
            $"/lol/match/v5/matches/{encodedMatchId}";

        using var response = await _riotApiClient.GetAsync(
            requestUri,
            cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        var dto = await JsonSerializer.DeserializeAsync<RiotMatchDto>(
            stream,
            cancellationToken: cancellationToken);

        Validate(dto);

        return Map(dto!);
    }

    private static void Validate(RiotMatchDto? dto)
    {
        if (dto?.Metadata is null)
        {
            throw new InvalidOperationException(
                "Riot Match API returned missing match metadata.");
        }

        if (string.IsNullOrWhiteSpace(dto.Metadata.MatchId))
        {
            throw new InvalidOperationException(
                "Riot Match API returned a missing match ID.");
        }

        if (string.IsNullOrWhiteSpace(dto.Metadata.DataVersion))
        {
            throw new InvalidOperationException(
                "Riot Match API returned a missing data version.");
        }

        if (dto.Info is null)
        {
            throw new InvalidOperationException(
                "Riot Match API returned missing match info.");
        }

        if (dto.Info.Participants is null ||
            dto.Info.Participants.Count == 0)
        {
            throw new InvalidOperationException(
                "Riot Match API returned no participants.");
        }

        foreach (var participant in dto.Info.Participants)
        {
            if (string.IsNullOrWhiteSpace(participant.Puuid))
            {
                throw new InvalidOperationException(
                    "Riot Match API returned a participant without a PUUID.");
            }

            if (participant.ParticipantId <= 0)
            {
                throw new InvalidOperationException(
                    "Riot Match API returned an invalid participant ID.");
            }

            if (participant.ChampionId <= 0)
            {
                throw new InvalidOperationException(
                    "Riot Match API returned an invalid champion ID.");
            }
        }
    }

    private static RiotMatchDetails Map(RiotMatchDto dto)
    {
        var metadata = dto.Metadata!;
        var info = dto.Info!;

        var participants = info.Participants!
            .Select(MapParticipant)
            .ToArray();

        return new RiotMatchDetails(
            metadata.DataVersion,
            metadata.MatchId,
            info.GameId,
            info.GameVersion,
            info.GameMode,
            info.GameType,
            info.MapId,
            info.QueueId,
            info.PlatformId,
            DateTimeOffset.FromUnixTimeMilliseconds(info.GameCreation),
            DateTimeOffset.FromUnixTimeMilliseconds(info.GameStartTimestamp),
            info.GameEndTimestamp.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(
                    info.GameEndTimestamp.Value)
                : null,
            TimeSpan.FromSeconds(info.GameDuration),
            info.EndOfGameResult,
            participants);
    }

    private static RiotMatchParticipant MapParticipant(
        RiotMatchParticipantDto dto)
    {
        var itemIds = new[]
        {
            dto.Item0,
            dto.Item1,
            dto.Item2,
            dto.Item3,
            dto.Item4,
            dto.Item5,
            dto.Item6
        }
        .Where(itemId => itemId > 0)
        .ToArray();

        var runeIds = dto.Perks?.Styles?
            .Where(style => style.Selections is not null)
            .SelectMany(style => style.Selections!)
            .Select(selection => selection.Perk)
            .Where(runeId => runeId > 0)
            .Distinct()
            .ToArray()
            ?? [];

        return new RiotMatchParticipant(
            dto.Puuid,
            dto.ParticipantId,
            dto.TeamId,
            dto.ChampionId,
            dto.ChampionName,
            dto.TeamPosition,
            dto.Kills,
            dto.Deaths,
            dto.Assists,
            dto.GoldEarned,
            dto.GoldSpent,
            dto.TotalMinionsKilled,
            dto.NeutralMinionsKilled,
            dto.VisionScore,
            dto.WardsPlaced,
            dto.WardsKilled,
            dto.TotalDamageDealt,
            dto.TotalDamageDealtToChampions,
            dto.TotalDamageTaken,
            TimeSpan.FromSeconds(dto.TimePlayed),
            dto.Win,
            itemIds,
            runeIds);
    }
}
