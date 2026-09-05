namespace SkillIssue.GG.Domain.Entities;

public class Champion
{
    public Guid Id { get; private set; }

    public int RiotChampionId { get; private set; }

    public string Name { get; private set; }

    private Champion()
    {
        Name = string.Empty;
    }

    public Champion(
        int riotChampionId,
        string name)
    {
        if (riotChampionId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(riotChampionId),
                "Riot champion ID must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Champion name is required.",
                nameof(name));
        }

        Id = Guid.NewGuid();
        RiotChampionId = riotChampionId;
        Name = name;
    }
}
