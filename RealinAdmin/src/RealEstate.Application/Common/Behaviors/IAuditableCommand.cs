namespace RealEstate.Application.Common.Behaviors;

public interface IAuditableCommand
{
    string AuditAction { get; }
    string EntityType { get; }
    Guid EntityId { get; }
}
