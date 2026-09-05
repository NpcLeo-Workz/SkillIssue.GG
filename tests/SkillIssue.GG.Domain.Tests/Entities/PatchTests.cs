using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Domain.Tests.Entities;

public class PatchTests
{
    [Fact]
    public void CreatesPatchWithValidInformation()
    {
        var patch = new Patch(
            version: "16.15",
            dataDragonVersion: "16.15.1");

        Assert.NotEqual(Guid.Empty, patch.Id);
        Assert.Equal("16.15", patch.Version);
        Assert.Equal("16.15.1", patch.DataDragonVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ThrowsWhenVersionIsMissing(string version)
    {
        Assert.Throws<ArgumentException>(() =>
            new Patch(
                version,
                "16.15.1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ThrowsWhenDataDragonVersionIsMissing(
        string dataDragonVersion)
    {
        Assert.Throws<ArgumentException>(() =>
            new Patch(
                "16.15",
                dataDragonVersion));
    }

    [Theory]
    [InlineData("16")]
    [InlineData("16.15.1")]
    [InlineData("16.x")]
    [InlineData("16.")]
    [InlineData(".15")]
    [InlineData("16..15")]
    [InlineData("0.15")]
    [InlineData("16.0")]
    [InlineData("016.15")]
    [InlineData("16.015")]
    [InlineData("-16.15")]
    [InlineData("16.-15")]
    [InlineData("+16.15")]
    [InlineData("16.+15")]
    [InlineData(" 16.15")]
    [InlineData("16.15 ")]
    [InlineData("16 .15")]
    [InlineData("16. 15")]
    public void ThrowsWhenVersionFormatIsInvalid(string version)
    {
        Assert.Throws<ArgumentException>(() =>
            new Patch(
                version,
                "16.15.1"));
    }

    [Theory]
    [InlineData("16")]
    [InlineData("16.15")]
    [InlineData("16.15.1.2")]
    [InlineData("16.15.x")]
    [InlineData("16.15.")]
    [InlineData(".15.1")]
    [InlineData("16..1")]
    [InlineData("0.15.1")]
    [InlineData("16.0.1")]
    [InlineData("16.15.0")]
    [InlineData("016.15.1")]
    [InlineData("16.015.1")]
    [InlineData("16.15.01")]
    [InlineData("-16.15.1")]
    [InlineData("16.-15.1")]
    [InlineData("16.15.-1")]
    [InlineData(" 16.15.1")]
    [InlineData("16.15.1 ")]
    [InlineData("16 .15.1")]
    public void ThrowsWhenDataDragonVersionFormatIsInvalid(
        string dataDragonVersion)
    {
        Assert.Throws<ArgumentException>(() =>
            new Patch(
                "16.15",
                dataDragonVersion));
    }
}
