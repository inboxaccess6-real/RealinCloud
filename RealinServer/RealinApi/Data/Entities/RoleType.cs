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
