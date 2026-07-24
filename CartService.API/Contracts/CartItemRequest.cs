namespace CartService.API.Contracts;

public sealed class CartItemRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ImageRequest? Image { get; set; }
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = "USD";
    public int Quantity { get; set; }
}
