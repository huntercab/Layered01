
namespace Shared.Messaging.Contracts;

public sealed record ProductUpdatedIntegrationEvent(
Guid MessageId,
int ProductId,
string Name,
decimal PriceAmount,
string Currency,
string? Image,
DateTimeOffset OccurredOnUtc);
