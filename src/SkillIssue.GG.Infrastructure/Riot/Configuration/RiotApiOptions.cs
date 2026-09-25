namespace SkillIssue.GG.Infrastructure.Riot.Configuration;

public sealed class RiotApiOptions
{
    public const string SectionName = "RiotApi";

    public string ApiKey { get; init; } = string.Empty;

    public string PlatformRoute { get; init; } = string.Empty;

    public string RegionalRoute { get; init; } = string.Empty;
}
