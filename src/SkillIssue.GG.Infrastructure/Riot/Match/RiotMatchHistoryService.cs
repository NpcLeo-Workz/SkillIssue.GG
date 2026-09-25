using System.Text.Json;
using Microsoft.Extensions.Options;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Infrastructure.Riot.Configuration;
using SkillIssue.GG.Infrastructure.Riot.Http;

namespace SkillIssue.GG.Infrastructure.Riot.Match;

public sealed class RiotMatchHistoryService(RiotApiClient riotApiClient,
        IOptions<RiotApiOptions> options) : IRiotMatchHistoryService
{
    private const int MaxCount = 100;

    private readonly RiotApiClient _riotApiClient = riotApiClient;
    private readonly RiotApiOptions _options = options.Value;

    public async Task<IReadOnlyList<string>> GetMatchIdsAsync(
        string puuid,
        int start = 0,
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(puuid);

        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(start),
                "Start must not be negative.");
        }

        if (count <= 0 || count > MaxCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                $"Count must be between 1 and {MaxCount}.");
        }

        var encodedPuuid = Uri.EscapeDataString(puuid);

        var requestUri =
            $"https://{_options.RegionalRoute}.api.riotgames.com" +
            $"/lol/match/v5/matches/by-puuid/{encodedPuuid}/ids" +
            $"?start={start}&count={count}";

        using var response = await _riotApiClient.GetAsync(
            requestUri,
            cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        var matchIds = await JsonSerializer.DeserializeAsync<string[]>(
            stream,
            cancellationToken: cancellationToken);

        if (matchIds is null)
        {
            throw new InvalidOperationException(
                "Riot Match API returned an invalid match history response.");
        }

        if (matchIds.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "Riot Match API returned an invalid match ID.");
        }

        return matchIds;
    }
}
