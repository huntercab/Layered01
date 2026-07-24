using CartService.DataAccess.Documents;
using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;

namespace CartService.DataAccess.Mappers;

internal static class CartMapper
{
    public static Cart ToDomain(CartDocument document)
    {
        var cart = new Cart(document.Id);

        foreach (var itemDocument in document.Items)
        {
            var image = string.IsNullOrWhiteSpace(itemDocument.ImageUrl) ? null
                : ImageInfo.Create(itemDocument.ImageUrl, itemDocument.ImageAltText ?? string.Empty);

            var item = new CartItem(
                    id: itemDocument.Id,
                    name: itemDocument.Name,
                    price: Money.Create(itemDocument.PriceAmount, itemDocument.PriceCurrency),
                    quantity: itemDocument.Quantity,
                    image: image
                );

            cart.AddItem(item);
        }

        return cart;
    }

    public static CartDocument ToDocument(Cart cart)
    {
        return new CartDocument
        {
            Id = cart.Id,
            Items = cart.Items.Select(item => new CartItemDocument
            {
                Id = item.Id,
                Name = item.Name,
                ImageUrl = item.Image?.Url ?? string.Empty,
                ImageAltText = item.Image?.AltText ?? string.Empty,

                PriceAmount = item.Price.Amount,
                PriceCurrency = item.Price.Currency,

                Quantity = item.Quantity
            }).ToList()
        };
    }
}
