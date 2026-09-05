using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class PlayerTests
{
    [Fact]
    public void CreatesPlayerWithValidInformation()
    {
        // Arrange
        var puuid = "12345";
        var name = "John Doe";
        var region = "EUW";
        // Act
        var player = new Player(puuid, name, region);
        // Assert
        Assert.NotEqual(Guid.Empty, player.Id);
        Assert.Equal(puuid, player.Puuid);
        Assert.Equal(name, player.Name);
        Assert.Equal(region, player.Region);
    }

    [Fact]
    public void ThrowsWhenPuuidIsNullOrWhitespace()
    {
        // Arrange
        var name = "John Doe";
        var region = "EUW";
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Player("", name, region));
        Assert.Throws<ArgumentException>(() => new Player("   ", name, region));
    }

    [Fact]
    public void ThrowsWhenNameIsNullOrWhitespace()
    {
        // Arrange
        var puuid = "12345";
        var region = "EUW";
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Player(puuid, "", region));
        Assert.Throws<ArgumentException>(() => new Player(puuid, "   ", region));
    }

    [Fact]
    public void ThrowsWhenRegionIsNullOrWhitespace()
    {
        // Arrange
        var puuid = "12345";
        var name = "John Doe";
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Player(puuid, name, ""));
        Assert.Throws<ArgumentException>(() => new Player(puuid, name, "   "));
    }

    [Fact]
    public void MatchesPuuidWhenPuuidMatches()
    {
        var player = new Player(
            "test-puuid",
            "TestPlayer",
            "EUW");

        var matches = player.MatchesPuuid("test-puuid");

        Assert.True(matches);
    }

    [Fact]
    public void DoesNotMatchDifferentPuuid()
    {
        var player = new Player(
            "test-puuid",
            "TestPlayer",
            "EUW");

        var matches = player.MatchesPuuid("different-puuid");

        Assert.False(matches);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void DoesNotMatchMissingPuuid(string playerPuuid)
    {
        var player = new Player(
            "test-puuid",
            "TestPlayer",
            "EUW");

        var matches = player.MatchesPuuid(playerPuuid);

        Assert.False(matches);
    }
}
