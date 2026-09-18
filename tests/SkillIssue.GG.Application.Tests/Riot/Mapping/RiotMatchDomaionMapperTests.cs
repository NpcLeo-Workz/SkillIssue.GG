using SkillIssue.GG.Application.Riot.Mapping;
using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Tests.Riot.Mapping;

public sealed class RiotMatchDomainMapperTests
{
    [Fact]
    public void Map_MapsMatchFields()
    {
        var source = CreateMatchDetails();

        var match = RiotMatchDomainMapper.Map(source);

        Assert.NotEqual(Guid.Empty, match.Id);

        Assert.Equal(source.RiotMatchId, match.RiotMatchId);
        Assert.Equal(source.RiotGameId, match.RiotGameId);
        Assert.Equal(source.DataVersion, match.DataVersion);
        Assert.Equal(source.GameVersion, match.GameVersion);
        Assert.Equal(source.GameMode, match.GameMode);
        Assert.Equal(source.GameType, match.GameType);
        Assert.Equal(source.MapId, match.MapId);
        Assert.Equal(source.QueueId, match.QueueId);
        Assert.Equal(source.PlatformId, match.PlatformId);
        Assert.Equal(source.GameCreatedAt, match.GameCreatedAt);
        Assert.Equal(source.StartedAt, match.StartedAt);
        Assert.Equal(source.EndedAt, match.EndedAt);
        Assert.Equal(source.Duration, match.Duration);
        Assert.Equal(source.EndOfGameResult, match.EndOfGameResult);
    }

    [Fact]
    public void Map_MapsParticipantFields()
    {
        var source = CreateMatchDetails();

        var match = RiotMatchDomainMapper.Map(source);

        var participant = Assert.Single(match.Participants);

        Assert.NotEqual(Guid.Empty, participant.Id);
        Assert.Equal(match.Id, participant.MatchId);

        Assert.Equal("test-puuid", participant.PlayerPuuid);
        Assert.Equal(1, participant.ParticipantId);
        Assert.Equal(100, participant.TeamId);
        Assert.Equal(266, participant.ChampionId);
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

        Assert.Equal(
            18000,
            participant.TotalDamageDealtToChampions);

        Assert.Equal(
            22000,
            participant.TotalDamageTaken);

        Assert.Equal(
            TimeSpan.FromSeconds(1800),
            participant.TimePlayed);

        Assert.True(participant.Won);
    }

    [Fact]
    public void Map_AddsItemIdsThroughParticipant()
    {
        var source = CreateMatchDetails();

        var match = RiotMatchDomainMapper.Map(source);

        var participant = Assert.Single(match.Participants);

        Assert.Equal(
            [3071, 3047, 6333, 3364],
            participant.ItemIds);
    }

    [Fact]
    public void Map_AddsRuneIdsThroughParticipant()
    {
        var source = CreateMatchDetails();

        var match = RiotMatchDomainMapper.Map(source);

        var participant = Assert.Single(match.Participants);

        Assert.Equal(
            [8005, 9111, 9104, 8014],
            participant.RuneIds);
    }

    [Fact]
    public void Map_PreservesNullableMatchCompletionFields()
    {
        var normal = CreateMatchDetails();

        var source = normal with
        {
            EndedAt = null,
            EndOfGameResult = null
        };

        var match = RiotMatchDomainMapper.Map(source);

        Assert.Null(match.EndedAt);
        Assert.Null(match.EndOfGameResult);
    }

    [Fact]
    public void Map_MapsMultipleParticipants()
    {
        var first = CreateParticipant(
            puuid: "puuid-one",
            participantId: 1,
            championId: 266);

        var second = CreateParticipant(
            puuid: "puuid-two",
            participantId: 2,
            championId: 103);

        var source = CreateMatchDetails(
            participants: [first, second]);

        var match = RiotMatchDomainMapper.Map(source);

        Assert.Equal(2, match.Participants.Count);

        Assert.All(
            match.Participants,
            participant =>
            {
                Assert.NotEqual(Guid.Empty, participant.Id);
                Assert.Equal(match.Id, participant.MatchId);
            });

        Assert.Contains(
            match.Participants,
            participant =>
                participant.PlayerPuuid == "puuid-one" &&
                participant.ChampionId == 266);

        Assert.Contains(
            match.Participants,
            participant =>
                participant.PlayerPuuid == "puuid-two" &&
                participant.ChampionId == 103);
    }

    [Fact]
    public void Map_AllowsEmptyItemAndRuneCollections()
    {
        var participant = CreateParticipant(
            itemIds: [],
            runeIds: []);

        var source = CreateMatchDetails(
            participants: [participant]);

        var match = RiotMatchDomainMapper.Map(source);

        var mappedParticipant = Assert.Single(
            match.Participants);

        Assert.Empty(mappedParticipant.ItemIds);
        Assert.Empty(mappedParticipant.RuneIds);
    }

    [Fact]
    public void Map_ThrowsArgumentNullException_WhenSourceIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => RiotMatchDomainMapper.Map(null!));
    }

    [Fact]
    public void Map_ThrowsWhenParticipantContainsInvalidDomainData()
    {
        var participant = CreateParticipant(
            puuid: "");

        var source = CreateMatchDetails(
            participants: [participant]);

        Assert.Throws<ArgumentException>(
            () => RiotMatchDomainMapper.Map(source));
    }

    private static RiotMatchDetails CreateMatchDetails(
        IReadOnlyList<RiotMatchParticipant>? participants = null)
    {
        var gameCreatedAt =
            DateTimeOffset.FromUnixTimeMilliseconds(
                1722500000000);

        var startedAt =
            DateTimeOffset.FromUnixTimeMilliseconds(
                1722500010000);

        participants ??=
        [
            CreateParticipant()
        ];

        return new RiotMatchDetails(
            DataVersion: "2",
            RiotMatchId: "EUW1_1234567890",
            RiotGameId: 1234567890L,
            GameVersion: "16.15.123.4567",
            GameMode: "CLASSIC",
            GameType: "MATCHED_GAME",
            MapId: 11,
            QueueId: 420,
            PlatformId: "EUW1",
            GameCreatedAt: gameCreatedAt,
            StartedAt: startedAt,
            EndedAt: DateTimeOffset.FromUnixTimeMilliseconds(1722501810000),
            Duration: TimeSpan.FromSeconds(1800),
            EndOfGameResult: "GameComplete",
            Participants: participants);
    }

    private static RiotMatchParticipant CreateParticipant(
        string puuid = "test-puuid",
        int participantId = 1,
        int championId = 266,
        IReadOnlyList<int>? itemIds = null,
        IReadOnlyList<int>? runeIds = null)
    {
        itemIds ??= [3071, 3047, 6333, 3364];
        runeIds ??= [8005, 9111, 9104, 8014];

        return new RiotMatchParticipant(
            Puuid: puuid,
            ParticipantId: participantId,
            TeamId: 100,
            ChampionId: championId,
            ChampionName: "Aatrox",
            TeamPosition: "TOP",
            Kills: 10,
            Deaths: 2,
            Assists: 8,
            GoldEarned: 12500,
            GoldSpent: 11800,
            TotalMinionsKilled: 210,
            NeutralMinionsKilled: 12,
            VisionScore: 24,
            WardsPlaced: 9,
            WardsKilled: 2,
            TotalDamageDealt: 25000,
            TotalDamageDealtToChampions: 18000,
            TotalDamageTaken: 22000,
            TimePlayed: TimeSpan.FromSeconds(1800),
            Won: true,
            ItemIds: itemIds,
            RuneIds: runeIds);
    }
}
