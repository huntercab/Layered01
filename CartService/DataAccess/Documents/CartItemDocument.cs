
namespace CartService.DataAccess.Documents;

public class CartItemDocument
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }

    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = "USD";

    public int Quantity { get; set; }
}
