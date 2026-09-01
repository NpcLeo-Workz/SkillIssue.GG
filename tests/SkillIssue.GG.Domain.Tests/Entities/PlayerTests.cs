using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class PlayerTests
{
    [Fact]
    public void CreatesPlayerWithValidInformation()
    {
        // Arrange
        var gamePlayerId = "12345";
        var name = "John Doe";
        var region = "EUW";
        // Act
        var player = new Player(gamePlayerId, name, region);
        // Assert
        Assert.NotEqual(Guid.Empty, player.Id);
        Assert.Equal(gamePlayerId, player.GamePlayerId);
        Assert.Equal(name, player.Name);
        Assert.Equal(region, player.Region);
    }

    [Fact]
    public void ThrowsWhenGamePlayerIdIsNullOrWhitespace()
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
        var gamePlayerId = "12345";
        var region = "EUW";
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Player(gamePlayerId, "", region));
        Assert.Throws<ArgumentException>(() => new Player(gamePlayerId, "   ", region));
    }

    [Fact]
    public void ThrowsWhenRegionIsNullOrWhitespace()
    {
        // Arrange
        var gamePlayerId = "12345";
        var name = "John Doe";
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Player(gamePlayerId, name, ""));
        Assert.Throws<ArgumentException>(() => new Player(gamePlayerId, name, "   "));
    }
}
