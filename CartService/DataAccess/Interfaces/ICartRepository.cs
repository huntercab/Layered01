using CartService.Domain.Entities;

namespace CartService.DataAccess.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(Guid cartId);
        Task SaveAsync(Cart cart);
    }
}
