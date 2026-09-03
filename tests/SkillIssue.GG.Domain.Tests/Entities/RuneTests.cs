using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class RuneTests
{
    [Fact]
    public void CreatesRuneWithValidInformation()
    {
        var rune = new Rune(
            riotRuneId: 8005,
            name: "Press the Attack",
            description: "Hitting an enemy champion 3 consecutive times deals bonus damage.",
            iconPath: "perk-images/Styles/Precision/PressTheAttack/PressTheAttack.png",
            runeTreeId: 8000,
            runeTreeName: "Precision");

        Assert.NotEqual(Guid.Empty, rune.Id);
        Assert.Equal(8005, rune.RiotRuneId);
        Assert.Equal("Press the Attack", rune.Name);
        Assert.Equal(
            "Hitting an enemy champion 3 consecutive times deals bonus damage.",
            rune.Description);
        Assert.Equal(
            "perk-images/Styles/Precision/PressTheAttack/PressTheAttack.png",
            rune.IconPath);
        Assert.Equal(8000, rune.RuneTreeId);
        Assert.Equal("Precision", rune.RuneTreeName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThrowsWhenRiotRuneIdIsInvalid(int riotRuneId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Rune(
                riotRuneId,
                "Press the Attack",
                "Description",
                "icon.png",
                8000,
                "Precision"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ThrowsWhenNameIsMissing(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Rune(
                8005,
                name,
                "Description",
                "icon.png",
                8000,
                "Precision"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThrowsWhenRuneTreeIdIsInvalid(int runeTreeId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Rune(
                8005,
                "Press the Attack",
                "Description",
                "icon.png",
                runeTreeId,
                "Precision"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ThrowsWhenRuneTreeNameIsMissing(string runeTreeName)
    {
        Assert.Throws<ArgumentException>(() =>
            new Rune(
                8005,
                "Press the Attack",
                "Description",
                "icon.png",
                8000,
                runeTreeName));
    }

    [Fact]
    public void UsesEmptyDescriptionWhenDescriptionIsNull()
    {
        var rune = new Rune(
            8005,
            "Press the Attack",
            null!,
            "icon.png",
            8000,
            "Precision");

        Assert.Equal(string.Empty, rune.Description);
    }

    [Fact]
    public void UsesEmptyIconPathWhenIconPathIsNull()
    {
        var rune = new Rune(
            8005,
            "Press the Attack",
            "Description",
            null!,
            8000,
            "Precision");

        Assert.Equal(string.Empty, rune.IconPath);
    }
}
