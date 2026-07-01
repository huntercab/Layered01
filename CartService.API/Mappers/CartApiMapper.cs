using CartService.API.Contracts;
using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;

namespace CartService.API.Mappers
{
    public static class CartApiMapper
    {
        public static CartItem ToDomain(CartItemRequest request)
        {
            var image = request.Image is null
                ? null
                : ImageInfo.Create(request.Image.Url, request.Image.AltText);

            return new CartItem(
                id: request.Id,
                name: request.Name,
                price: Money.Create(request.PriceAmount, request.PriceCurrency),
                quantity: request.Quantity,
                image: image
            );
        }

        public static CartItemResponse ToResponse(CartItem item)
        {
            return new CartItemResponse
            {
                Id = item.Id,
                Name = item.Name,
                ImageUrl = item.Image?.Url,
                ImageAltText = item.Image?.AltText,
                PriceAmount = item.Price.Amount,
                PriceCurrency = item.Price.Currency,
                Quantity = item.Quantity
            };
        }
    }
}
