namespace RealinApi.Data.Entities;

/// <summary>
/// Defines the hierarchical role types with explicit integer values
/// Higher number = higher privilege level
/// </summary>
public enum RoleType
{
    Guest = 0,        // Unverified or limited access users
    User = 1,         // Standard verified users
    Agent = 5,        // Real estate agents with property management access
    Admin = 10,       // System administrators
    SuperAdmin = 100  // Full system access
}

/// <summary>
/// Extension methods for RoleType enum
/// </summary>
public static class RoleTypeExtensions
{
    /// <summary>
    /// Maps a string role name to its corresponding RoleType enum value
    /// </summary>
    /// <param name="roleName">The role name as a string (case-insensitive)</param>
    /// <returns>The corresponding RoleType enum value</returns>
    /// <exception cref="ArgumentException">Thrown when the role name doesn't match any RoleType</exception>
    public static RoleType ToRoleType(this string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));
        }

        // Normalize the input: trim and convert to lowercase for case-insensitive comparison
        var normalizedName = roleName.Trim();

        // Try to parse the enum using case-insensitive matching
        if (Enum.TryParse<RoleType>(normalizedName, ignoreCase: true, out var roleType))
        {
            return roleType;
        }

        // If parsing fails, throw a descriptive exception
        throw new ArgumentException(
            $"Invalid role name '{roleName}'. Valid values are: {string.Join(", ", Enum.GetNames<RoleType>())}",
            nameof(roleName));
    }

    /// <summary>
    /// Tries to map a string role name to its corresponding RoleType enum value
    /// </summary>
    /// <param name="roleName">The role name as a string (case-insensitive)</param>
    /// <param name="roleType">The resulting RoleType if successful</param>
    /// <returns>True if the mapping was successful, false otherwise</returns>
    public static bool TryParseRoleType(this string roleName, out RoleType roleType)
    {
        roleType = default;
        
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        var normalizedName = roleName.Trim();
        return Enum.TryParse(normalizedName, ignoreCase: true, out roleType);
    }
}
