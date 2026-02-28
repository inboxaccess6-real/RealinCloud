namespace RealEstate.Application.DTOs.Dashboard;

public record DashboardMetrics(
    int TotalUsers,
    int TotalAgents,
    int TotalProperties,
    int TotalBuilders,
    int TotalProjects,
    int PendingAgentVerifications,
    int PendingPropertyApprovals,
    int FlaggedProperties
);
