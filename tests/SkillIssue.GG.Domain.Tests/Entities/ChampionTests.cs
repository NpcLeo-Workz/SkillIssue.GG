using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class ChampionTests
{
    [Fact]
    public void CreatesChampionWithValidInformation()
    {
        var champion = new Champion(
            266,
            "Aatrox");

        Assert.NotEqual(Guid.Empty, champion.Id);
        Assert.Equal(266, champion.RiotChampionId);
        Assert.Equal("Aatrox", champion.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThrowsWhenRiotChampionIdIsInvalid(
        int riotChampionId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Champion(
                riotChampionId,
                "Aatrox"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ThrowsWhenNameIsMissing(
        string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Champion(
                266,
                name));
    }
}
