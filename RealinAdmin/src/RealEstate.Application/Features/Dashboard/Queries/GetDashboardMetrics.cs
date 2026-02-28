using MediatR;
using RealEstate.Application.DTOs.Dashboard;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Dashboard.Queries;

public record GetDashboardMetricsQuery() : IRequest<DashboardMetrics>;

public class GetDashboardMetricsHandler : IRequestHandler<GetDashboardMetricsQuery, DashboardMetrics>
{
    private readonly IUserRepository _userRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IBuilderRepository _builderRepository;
    private readonly IProjectRepository _projectRepository;

    public GetDashboardMetricsHandler(
        IUserRepository userRepository,
        IAgentRepository agentRepository,
        IPropertyRepository propertyRepository,
        IBuilderRepository builderRepository,
        IProjectRepository projectRepository)
    {
        _userRepository = userRepository;
        _agentRepository = agentRepository;
        _propertyRepository = propertyRepository;
        _builderRepository = builderRepository;
        _projectRepository = projectRepository;
    }

    public async Task<DashboardMetrics> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        // Sequential execution - DbContext is not thread-safe
        var totalUsers = await _userRepository.CountAsync(cancellationToken);
        var totalAgents = await _agentRepository.CountAsync(cancellationToken);
        var totalProperties = await _propertyRepository.CountAsync(cancellationToken);
        var totalBuilders = await _builderRepository.CountAsync(cancellationToken);
        var totalProjects = await _projectRepository.CountAsync(cancellationToken);
        var pendingAgents = await _agentRepository.CountByStatusAsync("pending", cancellationToken);
        var pendingProperties = await _propertyRepository.CountByApprovalStatusAsync("submitted", cancellationToken);
        var flaggedProperties = await _propertyRepository.CountFlaggedAsync(cancellationToken);

        return new DashboardMetrics(
            TotalUsers: totalUsers,
            TotalAgents: totalAgents,
            TotalProperties: totalProperties,
            TotalBuilders: totalBuilders,
            TotalProjects: totalProjects,
            PendingAgentVerifications: pendingAgents,
            PendingPropertyApprovals: pendingProperties,
            FlaggedProperties: flaggedProperties
        );
    }
}
