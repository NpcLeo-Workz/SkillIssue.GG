namespace SkillIssue.GG.Domain.Entities;

public class Item
{
    public Guid Id { get; private set; }

    public int RiotItemId { get; private set; }

    public string Name { get; private set; }

    private Item()
    {
        Name = string.Empty;
    }

    public Item(
        int riotItemId,
        string name)
    {
        if (riotItemId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(riotItemId),
                "Riot item ID must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Item name is required.",
                nameof(name));
        }

        Id = Guid.NewGuid();
        RiotItemId = riotItemId;
        Name = name;
    }
}
