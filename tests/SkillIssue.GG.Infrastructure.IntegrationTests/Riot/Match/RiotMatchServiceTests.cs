using System.Net;
using Microsoft.Extensions.Options;
using SkillIssue.GG.Infrastructure.Riot.Configuration;
using SkillIssue.GG.Infrastructure.Riot.Http;
using SkillIssue.GG.Infrastructure.Riot.Match;

namespace SkillIssue.GG.Infrastructure.IntegrationTests.Riot.Match;

public sealed class RiotMatchServiceTests
{
    [Fact]
    public async Task GetMatchAsync_ReturnsMappedMatch_WhenRequestSucceeds()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchService(
            riotApiClient,
            options);

        var match = await service.GetMatchAsync(
            "EUW1_1234567890");

        Assert.Equal("2", match.DataVersion);
        Assert.Equal("EUW1_1234567890", match.RiotMatchId);
        Assert.Equal(1234567890L, match.RiotGameId);
        Assert.Equal("16.15.123.4567", match.GameVersion);
        Assert.Equal("CLASSIC", match.GameMode);
        Assert.Equal("MATCHED_GAME", match.GameType);
        Assert.Equal(11, match.MapId);
        Assert.Equal(420, match.QueueId);
        Assert.Equal("EUW1", match.PlatformId);

        Assert.Single(match.Participants);
    }

    [Fact]
    public async Task GetMatchAsync_UsesRegionalRouteAndCorrectEndpoint()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "na1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchService(
            riotApiClient,
            options);

        await service.GetMatchAsync(
            "EUW1_1234567890");

        Assert.NotNull(handler.RequestUri);

        Assert.Equal(
            "https://europe.api.riotgames.com/lol/match/v5/matches/EUW1_1234567890",
            handler.RequestUri.AbsoluteUri);
    }

    [Fact]
    public async Task GetMatchAsync_EncodesMatchId()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchService(
            riotApiClient,
            options);

        await service.GetMatchAsync(
            "EUW1 test/match");

        Assert.NotNull(handler.RequestUri);

        Assert.Equal(
            "/lol/match/v5/matches/EUW1%20test%2Fmatch",
            handler.RequestUri.AbsolutePath);
    }

    [Fact]
    public async Task GetMatchAsync_MapsTimestampsAndDuration()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchService(
            riotApiClient,
            options);

        var match = await service.GetMatchAsync(
            "EUW1_1234567890");

        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(1722500000000),
            match.GameCreatedAt);

        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(1722500010000),
            match.StartedAt);

        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(1722501810000),
            match.EndedAt);

        Assert.Equal(
            TimeSpan.FromSeconds(1800),
            match.Duration);

        Assert.Equal(
            "GameComplete",
            match.EndOfGameResult);
    }

    [Fact]
    public async Task GetMatchAsync_MapsParticipantData()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchService(
            riotApiClient,
            options);

        var match = await service.GetMatchAsync(
            "EUW1_1234567890");

        var participant = Assert.Single(
            match.Participants);

        Assert.Equal("test-puuid", participant.Puuid);
        Assert.Equal(1, participant.ParticipantId);
        Assert.Equal(100, participant.TeamId);
        Assert.Equal(266, participant.ChampionId);
        Assert.Equal("Aatrox", participant.ChampionName);
        Assert.Equal("TOP", participant.TeamPosition);

        Assert.Equal(10, participant.Kills);
        Assert.Equal(2, participant.Deaths);
        Assert.Equal(8, participant.Assists);

        Assert.Equal(12500, participant.GoldEarned);
        Assert.Equal(11800, participant.GoldSpent);

        Assert.Equal(210, participant.TotalMinionsKilled);
        Assert.Equal(12, participant.NeutralMinionsKilled);

        Assert.Equal(24, participant.VisionScore);
        Assert.Equal(9, participant.WardsPlaced);
        Assert.Equal(2, participant.WardsKilled);

        Assert.Equal(25000, participant.TotalDamageDealt);
        Assert.Equal(18000, participant.TotalDamageDealtToChampions);
        Assert.Equal(22000, participant.TotalDamageTaken);

        Assert.Equal(
            TimeSpan.FromSeconds(1800),
            participant.TimePlayed);

        Assert.True(participant.Won);
    }

    [Fact]
    public async Task GetMatchAsync_MapsNonZeroItemIds()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchService(
            riotApiClient,
            options);

        var match = await service.GetMatchAsync(
            "EUW1_1234567890");

        var participant = Assert.Single(
            match.Participants);

        Assert.Equal(
            [3071, 3047, 6333, 3065, 3053, 3364],
            participant.ItemIds);
    }

    [Fact]
    public async Task GetMatchAsync_MapsRuneIds()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var riotApiClient = new RiotApiClient(httpClient);

        var options = Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });

        var service = new RiotMatchService(
            riotApiClient,
            options);

        var match = await service.GetMatchAsync(
            "EUW1_1234567890");

        var participant = Assert.Single(
            match.Participants);

        Assert.Equal(
            [8005, 9111, 9104, 8014, 8451, 8444],
            participant.RuneIds);
    }

    [Fact]
    public async Task GetMatchAsync_AllowsNullableEndFields()
    {
        var json = ValidMatchJson
            .Replace(
                "\"gameEndTimestamp\": 1722501810000,",
                "\"gameEndTimestamp\": null,")
            .Replace(
                "\"endOfGameResult\": \"GameComplete\",",
                "\"endOfGameResult\": null,");

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        var match = await service.GetMatchAsync(
            "EUW1_1234567890");

        Assert.Null(match.EndedAt);
        Assert.Null(match.EndOfGameResult);
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsArgumentException_WhenMatchIdIsEmpty()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            ValidMatchJson);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetMatchAsync(""));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenMetadataIsMissing()
    {
        const string json =
            """
        {
          "info": {
            "participants": []
          }
        }
        """;

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));

        Assert.Equal(
            "Riot Match API returned missing match metadata.",
            exception.Message);
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenMatchIdIsMissing()
    {
        const string json =
            """
        {
          "metadata": {
            "dataVersion": "2",
            "matchId": ""
          },
          "info": {
            "participants": [
              {
                "puuid": "test-puuid",
                "participantId": 1,
                "championId": 266
              }
            ]
          }
        }
        """;

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenDataVersionIsMissing()
    {
        const string json =
            """
        {
          "metadata": {
            "dataVersion": "",
            "matchId": "EUW1_1234567890"
          },
          "info": {
            "participants": [
              {
                "puuid": "test-puuid",
                "participantId": 1,
                "championId": 266
              }
            ]
          }
        }
        """;

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenInfoIsMissing()
    {
        const string json =
            """
        {
          "metadata": {
            "dataVersion": "2",
            "matchId": "EUW1_1234567890"
          }
        }
        """;

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenParticipantsAreMissing()
    {
        const string json =
            """
        {
          "metadata": {
            "dataVersion": "2",
            "matchId": "EUW1_1234567890"
          },
          "info": {
          }
        }
        """;

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenParticipantsAreEmpty()
    {
        const string json =
            """
        {
          "metadata": {
            "dataVersion": "2",
            "matchId": "EUW1_1234567890"
          },
          "info": {
            "participants": []
          }
        }
        """;

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenParticipantPuuidIsMissing()
    {
        var json = ValidMatchJson.Replace(
            "\"puuid\": \"test-puuid\"",
            "\"puuid\": \"\"");

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenParticipantIdIsInvalid()
    {
        var json = ValidMatchJson.Replace(
            "\"participantId\": 1",
            "\"participantId\": 0");

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenChampionIdIsInvalid()
    {
        var json = ValidMatchJson.Replace(
            "\"championId\": 266",
            "\"championId\": 0");

        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            json);

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsJsonException_WhenResponseContainsMalformedJson()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "{ invalid-json");

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsInvalidOperationException_WhenResponseIsNull()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            "null");

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetMatchAsync("EUW1_1234567890"));
    }

    [Fact]
    public async Task GetMatchAsync_ThrowsRiotApiException_WhenRequestFails()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.NotFound,
            "{}");

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        var exception = await Assert.ThrowsAsync<RiotApiException>(
            () => service.GetMatchAsync("EUW1_missing"));

        Assert.Equal(
            HttpStatusCode.NotFound,
            exception.StatusCode);
    }

    [Fact]
    public async Task GetMatchAsync_PropagatesCancellation()
    {
        var handler = new CancellationHttpMessageHandler();

        using var httpClient = new HttpClient(handler);

        var service = new RiotMatchService(
            new RiotApiClient(httpClient),
            CreateOptions());

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetMatchAsync(
                "EUW1_1234567890",
                cancellationTokenSource.Token));
    }

    private static IOptions<RiotApiOptions> CreateOptions()
    {
        return Options.Create(new RiotApiOptions
        {
            ApiKey = "test-api-key",
            PlatformRoute = "euw1",
            RegionalRoute = "europe"
        });
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

    private const string ValidMatchJson =
        """
        {
          "metadata": {
            "dataVersion": "2",
            "matchId": "EUW1_1234567890"
          },
          "info": {
            "gameId": 1234567890,
            "gameVersion": "16.15.123.4567",
            "gameMode": "CLASSIC",
            "gameType": "MATCHED_GAME",
            "mapId": 11,
            "queueId": 420,
            "platformId": "EUW1",
            "gameCreation": 1722500000000,
            "gameStartTimestamp": 1722500010000,
            "gameEndTimestamp": 1722501810000,
            "gameDuration": 1800,
            "endOfGameResult": "GameComplete",
            "participants": [
              {
                "puuid": "test-puuid",
                "participantId": 1,
                "teamId": 100,
                "championId": 266,
                "championName": "Aatrox",
                "teamPosition": "TOP",
                "kills": 10,
                "deaths": 2,
                "assists": 8,
                "goldEarned": 12500,
                "goldSpent": 11800,
                "totalMinionsKilled": 210,
                "neutralMinionsKilled": 12,
                "visionScore": 24,
                "wardsPlaced": 9,
                "wardsKilled": 2,
                "totalDamageDealt": 25000,
                "totalDamageDealtToChampions": 18000,
                "totalDamageTaken": 22000,
                "timePlayed": 1800,
                "win": true,
                "item0": 3071,
                "item1": 3047,
                "item2": 6333,
                "item3": 3065,
                "item4": 3053,
                "item5": 0,
                "item6": 3364,
                "perks": {
                  "styles": [
                    {
                      "style": 8000,
                      "selections": [
                        { "perk": 8005 },
                        { "perk": 9111 },
                        { "perk": 9104 },
                        { "perk": 8014 }
                      ]
                    },
                    {
                      "style": 8400,
                      "selections": [
                        { "perk": 8451 },
                        { "perk": 8444 }
                      ]
                    }
                  ]
                }
              }
            ]
          }
        }
        """;
}
