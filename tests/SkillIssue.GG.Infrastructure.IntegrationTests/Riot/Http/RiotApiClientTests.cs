using System.Net;
using SkillIssue.GG.Infrastructure.Riot.Http;

namespace SkillIssue.GG.Infrastructure.IntegrationTests.Riot.Http;

public sealed class RiotApiClientTests
{
    [Fact]
    public async Task GetAsync_ReturnsResponse_WhenRequestSucceeds()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """{"puuid":"test-puuid"}""");

        using var httpClient = new HttpClient(handler);
        var client = new RiotApiClient(httpClient);

        using var response = await client.GetAsync(
            "https://europe.api.riotgames.com/test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal("""{"puuid":"test-puuid"}""", content);
    }

    [Fact]
    public async Task GetAsync_ThrowsRiotApiException_WhenRequestFails()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.NotFound);

        using var httpClient = new HttpClient(handler);
        var client = new RiotApiClient(httpClient);

        var exception = await Assert.ThrowsAsync<RiotApiException>(
            () => client.GetAsync(
                "https://europe.api.riotgames.com/test"));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task GetAsync_PropagatesCancellation()
    {
        var handler = new CancellationHttpMessageHandler();

        using var httpClient = new HttpClient(handler);
        var client = new RiotApiClient(httpClient);

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetAsync(
                "https://europe.api.riotgames.com/test",
                cancellationTokenSource.Token));
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string? content = null) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode);

            if (content is not null)
            {
                response.Content = new StringContent(content);
            }

            return Task.FromResult(response);
        }
    }

    private sealed class CancellationHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
