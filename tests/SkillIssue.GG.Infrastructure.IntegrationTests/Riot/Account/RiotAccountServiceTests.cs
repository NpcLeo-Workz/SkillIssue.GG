using System.Net;
using Microsoft.Extensions.Options;
using SkillIssue.GG.Infrastructure.Riot.Account;
using SkillIssue.GG.Infrastructure.Riot.Configuration;
using SkillIssue.GG.Infrastructure.Riot.Http;

namespace SkillIssue.GG.Infrastructure.IntegrationTests.Riot.Account;

public sealed class RiotAccountServiceTests
{
    [Fact]
    public async Task GetByRiotIdAsync_ReturnsAccount_WhenRequestSucceeds()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """
            {
              "puuid": "test-puuid",
              "gameName": "Test Player",
              "tagLine": "EUW"
            }
            """);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotAccountService(
            riotApiClient,
            options);

        var account = await service.GetByRiotIdAsync(
            "Test Player",
            "EUW");

        Assert.Equal("test-puuid", account.Puuid);
        Assert.Equal("Test Player", account.GameName);
        Assert.Equal("EUW", account.TagLine);
    }

    [Fact]
    public async Task GetByRiotIdAsync_UsesRegionalRouteAndCorrectEndpoint()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """
            {
              "puuid": "test-puuid",
              "gameName": "TestPlayer",
              "tagLine": "EUW"
            }
            """);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotAccountService(
            riotApiClient,
            options);

        await service.GetByRiotIdAsync(
            "TestPlayer",
            "EUW");

        Assert.NotNull(handler.RequestUri);

        Assert.Equal(
            "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/TestPlayer/EUW",
            handler.RequestUri.AbsoluteUri);
    }

    [Fact]
    public async Task GetByRiotIdAsync_EncodesRiotIdPathSegments()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """
            {
              "puuid": "test-puuid",
              "gameName": "Test Player",
              "tagLine": "EU W"
            }
            """);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotAccountService(
            riotApiClient,
            options);

        await service.GetByRiotIdAsync(
            "Test Player",
            "EU W");

        Assert.NotNull(handler.RequestUri);

        Assert.Equal(
            "/riot/account/v1/accounts/by-riot-id/Test%20Player/EU%20W",
            handler.RequestUri.AbsolutePath);
    }
    [Fact]
    public async Task GetByRiotIdAsync_ThrowsRiotApiException_WhenRequestFails()
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

        var service = new RiotAccountService(
            riotApiClient,
            options);

        var exception = await Assert.ThrowsAsync<RiotApiException>(
            () => service.GetByRiotIdAsync(
                "MissingPlayer",
                "EUW"));

        Assert.Equal(
            HttpStatusCode.NotFound,
            exception.StatusCode);
    }

    [Fact]
    public async Task GetByRiotIdAsync_ThrowsInvalidOperationException_WhenResponseIsInvalid()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """
        {
          "puuid": "",
          "gameName": "TestPlayer",
          "tagLine": "EUW"
        }
        """);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotAccountService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetByRiotIdAsync(
                "TestPlayer",
                "EUW"));
    }

    [Fact]
    public async Task GetByRiotIdAsync_ThrowsJsonException_WhenResponseContainsMalformedJson()
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

        var service = new RiotAccountService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => service.GetByRiotIdAsync(
                "TestPlayer",
                "EUW"));
    }

    [Fact]
    public async Task GetByRiotIdAsync_ThrowsArgumentException_WhenGameNameIsEmpty()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "{}");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotAccountService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetByRiotIdAsync(
                "",
                "EUW"));
    }

    [Fact]
    public async Task GetByRiotIdAsync_ThrowsArgumentException_WhenTagLineIsEmpty()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "{}");

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotAccountService(
            riotApiClient,
            options);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetByRiotIdAsync(
                "TestPlayer",
                ""));
    }

    [Fact]
    public async Task GetByRiotIdAsync_PropagatesCancellation()
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

        var service = new RiotAccountService(
            riotApiClient,
            options);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetByRiotIdAsync(
                "TestPlayer",
                "EUW",
                cancellationTokenSource.Token));
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
}
