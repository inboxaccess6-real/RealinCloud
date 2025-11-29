using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Features.Admin.Models;

namespace RealinApi.Features.Admin;

public interface IUserService
{
    Task<List<UserResponse>> GetAllUsersAsync(int skip = 0, int take = 50);
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserResponse>> GetAllUsersAsync(int skip = 0, int take = 50)
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return users.Select(MapToUserResponse).ToList();
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToUserResponse(user);
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        if (user == null) return null;

        if (request.Name != null) user.Name = request.Name;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
        
        if (request.RoleId.HasValue)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId.Value);
            if (!roleExists)
                throw new InvalidOperationException($"Role with ID {request.RoleId} not found");
            user.RoleId = request.RoleId.Value;
        }
        
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        // Reload to get updated role
        await _context.Entry(user).Reference(u => u.Role).LoadAsync();
        return MapToUserResponse(user);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    private static UserResponse MapToUserResponse(Data.Entities.User user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.Name,
            user.Provider,
            user.OAuthProviderId,
            user.RoleId,
            user.Role.Name,
            user.Role.RoleType,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
