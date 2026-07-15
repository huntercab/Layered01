using System;
using System.Collections.Generic;
using System.Text;

namespace CartService.Business.Interfaces
{
    public interface IInboxRepository
    {
        Task<bool> ExistsAsync(
            Guid messageId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Guid messageId,
            CancellationToken cancellationToken = default);
    }
}
