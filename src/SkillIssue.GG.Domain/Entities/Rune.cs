namespace SkillIssue.GG.Domain.Entities;

public class Rune
{
    public Guid Id { get; private set; }

    public int RiotRuneId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string IconPath { get; private set; }

    public int RuneTreeId { get; private set; }

    public string RuneTreeName { get; private set; }

    private Rune()
    {
        Name = string.Empty;
        Description = string.Empty;
        IconPath = string.Empty;
        RuneTreeName = string.Empty;
    }

    public Rune(
        int riotRuneId,
        string name,
        string description,
        string iconPath,
        int runeTreeId,
        string runeTreeName)
    {
        if (riotRuneId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(riotRuneId),
                "Riot rune ID must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Rune name is required.",
                nameof(name));
        }

        if (runeTreeId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(runeTreeId),
                "Rune tree ID must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(runeTreeName))
        {
            throw new ArgumentException(
                "Rune tree name is required.",
                nameof(runeTreeName));
        }

        Id = Guid.NewGuid();
        RiotRuneId = riotRuneId;
        Name = name;
        Description = description ?? string.Empty;
        IconPath = iconPath ?? string.Empty;
        RuneTreeId = runeTreeId;
        RuneTreeName = runeTreeName;
    }
}
