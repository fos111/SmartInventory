namespace SmartInventory.Domain.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
