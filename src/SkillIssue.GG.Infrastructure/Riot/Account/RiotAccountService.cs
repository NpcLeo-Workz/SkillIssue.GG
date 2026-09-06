using System.Text.Json;
using Microsoft.Extensions.Options;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Infrastructure.Riot.Account.Dto;
using SkillIssue.GG.Infrastructure.Riot.Configuration;
using SkillIssue.GG.Infrastructure.Riot.Http;

namespace SkillIssue.GG.Infrastructure.Riot.Account;

public sealed class RiotAccountService(RiotApiClient riotApiClient, IOptions<RiotApiOptions> options) : IRiotAccountService
{
    private readonly RiotApiClient _riotApiClient = riotApiClient;
    private readonly RiotApiOptions _options = options.Value;

    public async Task<RiotAccount> GetByRiotIdAsync(
        string gameName,
        string tagLine,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tagLine);

        var encodedGameName = Uri.EscapeDataString(gameName);
        var encodedTagLine = Uri.EscapeDataString(tagLine);

        var requestUri =
            $"https://{_options.RegionalRoute}.api.riotgames.com" +
            $"/riot/account/v1/accounts/by-riot-id/{encodedGameName}/{encodedTagLine}";

        using var response = await _riotApiClient.GetAsync(
            requestUri,
            cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        var dto = await JsonSerializer.DeserializeAsync<RiotAccountDto>(
            stream,
            cancellationToken: cancellationToken);

        if (dto is null ||
            string.IsNullOrWhiteSpace(dto.Puuid) ||
            string.IsNullOrWhiteSpace(dto.GameName) ||
            string.IsNullOrWhiteSpace(dto.TagLine))
        {
            throw new InvalidOperationException(
                "Riot Account API returned an invalid account response.");
        }

        return new RiotAccount(
            dto.Puuid,
            dto.GameName,
            dto.TagLine);
    }
}
