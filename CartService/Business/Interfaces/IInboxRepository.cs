namespace CartService.Business.Interfaces;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);
}
