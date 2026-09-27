using System.Net;
using Microsoft.Extensions.Options;
using SkillIssue.GG.Infrastructure.Riot.Configuration;
using SkillIssue.GG.Infrastructure.Riot.Http;
using SkillIssue.GG.Infrastructure.Riot.Match;

namespace SkillIssue.GG.Infrastructure.IntegrationTests.Riot.Match;

public sealed class RiotMatchHistoryServiceTests
{
    [Fact]
    public async Task GetMatchIdsAsync_ReturnsMatchIds_WhenRequestSucceeds()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """
            [
              "EUW1_1234567890",
              "EUW1_1234567891"
            ]
            """);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        var matchIds = await service.GetMatchIdsAsync(
            "test-puuid");

        Assert.Equal(2, matchIds.Count);
        Assert.Equal("EUW1_1234567890", matchIds[0]);
        Assert.Equal("EUW1_1234567891", matchIds[1]);
    }

    [Fact]
    public async Task GetMatchIdsAsync_ReturnsEmptyCollection_WhenHistoryIsEmpty()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        var matchIds = await service.GetMatchIdsAsync(
            "test-puuid");

        Assert.Empty(matchIds);
    }

    [Fact]
    public async Task GetMatchIdsAsync_UsesRegionalRouteAndCorrectEndpoint()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await service.GetMatchIdsAsync(
            "test-puuid");

        Assert.NotNull(handler.RequestUri);

        Assert.Equal(
            "https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/test-puuid/ids?start=0&count=20",
            handler.RequestUri.AbsoluteUri);
    }

    [Fact]
    public async Task GetMatchIdsAsync_EncodesPuuid()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await service.GetMatchIdsAsync(
            "test puuid/value");

        Assert.NotNull(handler.RequestUri);

        Assert.Equal(
            "/lol/match/v5/matches/by-puuid/test%20puuid%2Fvalue/ids",
            handler.RequestUri.AbsolutePath);
    }

    [Fact]
    public async Task GetMatchIdsAsync_UsesCustomPagination()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await service.GetMatchIdsAsync(
            "test-puuid",
            start: 40,
            count: 50);

        Assert.NotNull(handler.RequestUri);

        Assert.Equal(
            "?start=40&count=50",
            handler.RequestUri.Query);
    }
    [Fact]
    public async Task GetMatchIdsAsync_ThrowsArgumentException_WhenPuuidIsEmpty()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetMatchIdsAsync(""));
    }

    [Fact]
    public async Task GetMatchIdsAsync_ThrowsArgumentOutOfRangeException_WhenStartIsNegative()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.GetMatchIdsAsync(
                "test-puuid",
                start: -1));
    }

    [Fact]
    public async Task GetMatchIdsAsync_ThrowsArgumentOutOfRangeException_WhenCountIsZero()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.GetMatchIdsAsync(
                "test-puuid",
                count: 0));
    }

    [Fact]
    public async Task GetMatchIdsAsync_ThrowsArgumentOutOfRangeException_WhenCountExceedsMaximum()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "[]");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.GetMatchIdsAsync(
                "test-puuid",
                count: 101));
    }

    [Fact]
    public async Task GetMatchIdsAsync_ThrowsRiotApiException_WhenRequestFails()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.NotFound,
            "{}");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        var exception = await Assert.ThrowsAsync<RiotApiException>(
            () => service.GetMatchIdsAsync(
                "missing-puuid"));

        Assert.Equal(
            HttpStatusCode.NotFound,
            exception.StatusCode);
    }

    [Fact]
    public async Task GetMatchIdsAsync_ThrowsJsonException_WhenResponseContainsMalformedJson()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "{ invalid-json");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => service.GetMatchIdsAsync(
                "test-puuid"));
    }

    [Fact]
    public async Task GetMatchIdsAsync_ThrowsInvalidOperationException_WhenResponseContainsInvalidMatchId()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """
        [
          "EUW1_1234567890",
          ""
        ]
        """);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchIdsAsync(
                "test-puuid"));
    }

    [Fact]
    public async Task GetMatchIdsAsync_ThrowsInvalidOperationException_WhenResponseIsNull()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "null");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchIdsAsync(
                "test-puuid"));
    }

    [Fact]
    public async Task GetMatchIdsAsync_PropagatesCancellation()
    {
        var handler = new CancellationHttpMessageHandler();

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchHistoryService(
            riotApiClient,
            options);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetMatchIdsAsync(
                "test-puuid",
                cancellationToken: cancellationTokenSource.Token));
    }

    private sealed class CancellationHttpMessageHandler
    : HttpMessageHandler
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

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public Uri? RequestUri { get; private set; }

        public StubHttpMessageHandler(
            HttpStatusCode statusCode,
            string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(
                new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_content)
                });
        }
    }
}
