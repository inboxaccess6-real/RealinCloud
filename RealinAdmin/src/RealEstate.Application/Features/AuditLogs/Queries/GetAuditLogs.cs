using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.AuditLogs;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.AuditLogs.Queries;

public record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 50,
    string? EntityType = null,
    Guid? PerformedBy = null
) : IRequest<PagedResult<AuditLogResponse>>;

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<AuditLogResponse>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _auditLogRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.EntityType,
            request.PerformedBy,
            cancellationToken);

        var responses = items.Select(log => new AuditLogResponse(
            log.Id,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.PerformedBy,
            log.Details,
            log.IpAddress,
            log.CreatedAt,
            log.PerformedByUser is not null
                ? new UserSummary(log.PerformedByUser.Id, log.PerformedByUser.Name, log.PerformedByUser.Email)
                : null
        )).ToList();

        return new PagedResult<AuditLogResponse>(responses, totalCount, request.Page, request.PageSize);
    }
}
