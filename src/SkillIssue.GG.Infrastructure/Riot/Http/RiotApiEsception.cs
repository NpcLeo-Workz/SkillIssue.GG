using System.Net;

namespace SkillIssue.GG.Infrastructure.Riot.Http;

public sealed class RiotApiException(HttpStatusCode statusCode)
    : Exception($"Riot API request failed with status code {(int)statusCode} ({statusCode}).")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
