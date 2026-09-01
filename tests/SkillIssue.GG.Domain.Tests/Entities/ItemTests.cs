using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class ItemTests
{
    [Fact]
    public void CreatesItemWithValidInformation()
    {
        var item = new Item(
            3078,
            "Trinity Force");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(3078, item.RiotItemId);
        Assert.Equal("Trinity Force", item.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThrowsWhenRiotItemIdIsInvalid(
        int riotItemId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Item(
                riotItemId,
                "Trinity Force"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ThrowsWhenNameIsMissing(
        string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Item(
                3078,
                name));
    }
}
