namespace RealEstate.Domain.Entities;

public enum RoleType
{
    Guest = 0,
    User = 1,
    Agent = 5,
    Admin = 10,
    SuperAdmin = 100
}

public static class RoleTypeExtensions
{
    public static RoleType ToRoleType(this string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

        var normalizedName = roleName.Trim();

        if (Enum.TryParse<RoleType>(normalizedName, ignoreCase: true, out var roleType))
            return roleType;

        throw new ArgumentException(
            $"Invalid role name '{roleName}'. Valid values are: {string.Join(", ", Enum.GetNames<RoleType>())}",
            nameof(roleName));
    }

    public static bool TryParseRoleType(this string roleName, out RoleType roleType)
    {
        roleType = default;

        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        var normalizedName = roleName.Trim();
        return Enum.TryParse(normalizedName, ignoreCase: true, out roleType);
    }
}
