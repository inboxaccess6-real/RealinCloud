using MediatR;
using RealEstate.Application.DTOs.AuditLogs;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.AuditLogs.Queries;

public record GetEntityAuditTrailQuery(string EntityType, Guid EntityId) : IRequest<List<AuditLogResponse>>;

public class GetEntityAuditTrailHandler : IRequestHandler<GetEntityAuditTrailQuery, List<AuditLogResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetEntityAuditTrailHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<List<AuditLogResponse>> Handle(GetEntityAuditTrailQuery request, CancellationToken cancellationToken)
    {
        var items = await _auditLogRepository.GetByEntityAsync(request.EntityType, request.EntityId, cancellationToken);

        return items.Select(log => new AuditLogResponse(
            log.Id,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.PerformedBy,
            log.Details,
            log.IpAddress,
            log.CreatedAt
        )).ToList();
    }
}
