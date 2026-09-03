namespace SkillIssue.GG.Domain.Entities;

public class Patch
{
    public Guid Id { get; private set; }

    public string Version { get; private set; }

    public string DataDragonVersion { get; private set; }

    private Patch()
    {
        Version = string.Empty;
        DataDragonVersion = string.Empty;
    }

    public Patch(
        string version,
        string dataDragonVersion)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException(
                "Patch version is required.",
                nameof(version));
        }

        if (string.IsNullOrWhiteSpace(dataDragonVersion))
        {
            throw new ArgumentException(
                "Data Dragon version is required.",
                nameof(dataDragonVersion));
        }

        if (!IsValidPatchVersion(version))
        {
            throw new ArgumentException(
                "Patch version must use the format major.minor with positive integers and no leading zeros.",
                nameof(version));
        }

        if (!IsValidDataDragonVersion(dataDragonVersion))
        {
            throw new ArgumentException(
                "Data Dragon version must use the format major.minor.build with positive integers and no leading zeros.",
                nameof(dataDragonVersion));
        }

        Id = Guid.NewGuid();
        Version = version;
        DataDragonVersion = dataDragonVersion;
    }

    private static bool IsValidPatchVersion(string version)
    {
        return IsValidVersion(version, expectedParts: 2);
    }

    private static bool IsValidDataDragonVersion(string version)
    {
        return IsValidVersion(version, expectedParts: 3);
    }

    private static bool IsValidVersion(
        string version,
        int expectedParts)
    {
        var parts = version.Split('.');

        if (parts.Length != expectedParts)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (part.Length == 0)
            {
                return false;
            }

            if (!part.All(char.IsDigit))
            {
                return false;
            }

            if (part.Length > 1 && part.StartsWith('0'))
            {
                return false;
            }

            if (!int.TryParse(part, out var value))
            {
                return false;
            }

            if (value <= 0)
            {
                return false;
            }
        }

        return true;
    }
}
