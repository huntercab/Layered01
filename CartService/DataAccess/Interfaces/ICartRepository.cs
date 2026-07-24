using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;

namespace CartService.DataAccess.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid cartId);
    Task SaveAsync(Cart cart);

    Task UpdateProductInAllCartsAsync(int productId, string name, Money price, ImageInfo? image);
}
