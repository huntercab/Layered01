using CartService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CartService.Business.Interfaces
{
    public interface ICartService
    {
        Task<IReadOnlyCollection<CartItem>> GetCartItemsAsync(Guid cartId);
        Task AddItemAsync(Guid cartId, CartItem item);
        Task RemoveItemAsync(Guid cartId, int ItemId);
    }
}
