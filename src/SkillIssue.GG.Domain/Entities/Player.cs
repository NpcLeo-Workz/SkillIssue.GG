namespace SkillIssue.GG.Domain.Entities;

public class Player
{
    public Guid Id { get; private set; }
    public string Puuid { get; private set; }
    public string Name { get; private set; }
    public string Region { get; private set; }
    private Player()
    {
        Puuid = string.Empty;
        Name = string.Empty;
        Region = string.Empty;
    }
    public Player(
        string puuid,
        string name,
        string region)
    {
        if (string.IsNullOrWhiteSpace(puuid))
        {
            throw new ArgumentException(
                "PUUID is required.",
                nameof(puuid));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Player name is required.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "Player region is required.",
                nameof(region));
        }

        Id = Guid.NewGuid();
        Puuid = puuid;
        Name = name;
        Region = region;
    }

    public bool MatchesPuuid(string playerPuuid)
    {
        if (string.IsNullOrWhiteSpace(playerPuuid))
        {
            return false;
        }

        return string.Equals(
            Puuid,
            playerPuuid,
            StringComparison.Ordinal);
    }
}
