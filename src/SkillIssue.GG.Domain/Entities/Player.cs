namespace SkillIssue.GG.Domain.Entities;

public class Player
{
    public Guid Id { get; private set; }
    public string GamePlayerId { get; private set; }
    public string Name { get; private set; }
    public string Region { get; private set; }
    private Player() { }
    public Player(
        string gamePlayerId,
        string name,
        string region)
    {
        if (string.IsNullOrWhiteSpace(gamePlayerId))
        {
            throw new ArgumentException(
                "Game player ID is required.",
                nameof(gamePlayerId));
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
        GamePlayerId = gamePlayerId;
        Name = name;
        Region = region;
    }
}
