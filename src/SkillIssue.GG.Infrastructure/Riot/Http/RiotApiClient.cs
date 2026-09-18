namespace SkillIssue.GG.Infrastructure.Riot.Http;

public sealed class RiotApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HttpResponseMessage> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            requestUri,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;

            response.Dispose();

            throw new RiotApiException(statusCode);
        }

        return response;
    }
}
