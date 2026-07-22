using CartService.API.Contracts;
using CartService.API.Mappers;
using CartService.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization;

namespace CartService.API.Controllers.V2
{
    [ApiController]
    [Route("api/v2/carts")]
    [Authorize(Policy = StorePolicies.CartAccess)]
    public sealed class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{cartKey}")]
        public async Task<ActionResult<IReadOnlyCollection<CartItemResponse>>> GetCartItems(string cartKey)
        {
            if (!Guid.TryParse(cartKey, out var cartId))
                return BadRequest("Invalid cart key.");

            var items = await _cartService.GetCartItemsAsync(cartId);

            return Ok(items.Select(CartApiMapper.ToResponse).ToList());
        }

        [HttpPost("{cartKey}/items")]
        public async Task<IActionResult> AddItem(string cartKey, CartItemRequest request)
        {
            if (!Guid.TryParse(cartKey, out var cartId))
                return BadRequest("Invalid cart key.");

            try
            {
                var item = CartApiMapper.ToDomain(request);

                await _cartService.AddItemAsync(cartId, item);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{cartKey}/items/{itemId:int}")]
        public async Task<IActionResult> DeleteItem(string cartKey, int itemId)
        {
            if (!Guid.TryParse(cartKey, out var cartId))
                return BadRequest("Invalid cart key.");

            if (itemId <= 0)
                return BadRequest("Invalid item id.");

            await _cartService.RemoveItemAsync(cartId, itemId);

            return Ok();
        }
    }
}
