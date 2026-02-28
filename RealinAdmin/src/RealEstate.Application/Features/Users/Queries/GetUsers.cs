using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Users;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Users.Queries;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    bool? IsActive = null
) : IRequest<PagedResult<UserResponse>>;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedResult<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _userRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.IsActive,
            cancellationToken);

        var users = items.Select(user => new UserResponse(
            user.Id,
            user.Email,
            user.PhoneNumber,
            user.Name,
            user.Provider,
            user.Role?.Name ?? "",
            user.IsActive,
            user.IsBlocked,
            user.IsBlacklisted,
            user.BlacklistReason,
            user.IsDeleted,
            user.CreatedAt,
            user.UpdatedAt)).ToList();

        return new PagedResult<UserResponse>(users, totalCount, request.Page, request.PageSize);
    }
}
