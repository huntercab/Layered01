using CartService.Business.Interfaces;
using CartService.DataAccess.Interfaces;
using CartService.Domain.Entities;

namespace CartService.Business.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository
                    ?? throw new ArgumentNullException(nameof(cartRepository));
        }

        public async Task AddItemAsync(Guid cartId, CartItem item)
        {
            ValidateCartId(cartId);
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            var cart = await _cartRepository.GetByIdAsync(cartId) ?? new Cart(cartId);

            cart.AddItem(item);

            await _cartRepository.SaveAsync(cart);
        }

        public async Task<IReadOnlyCollection<CartItem>> GetCartItemsAsync(Guid cartId)
        {
            ValidateCartId(cartId);

            var cart = await _cartRepository.GetByIdAsync(cartId);

            return cart?.Items ?? Array.Empty<CartItem>();
        }

        public async Task RemoveItemAsync(Guid cartId, int ItemId)
        {
            ValidateCartId(cartId);

            var cart = await _cartRepository.GetByIdAsync(cartId);

            if (cart is null)
                return;

            cart.RemoveItem(ItemId);

            await _cartRepository.SaveAsync(cart);
        }

        private static void ValidateCartId(Guid cartId) 
        { 
            if(cartId == Guid.Empty)
                    throw new ArgumentException("Cart id is required",nameof(cartId));
        }
    }
}
