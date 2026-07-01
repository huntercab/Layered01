namespace CartService.API.Contracts
{
    public sealed class CartResponse
    {
        public string CartKey { get; set; } = string.Empty;
        public IReadOnlyCollection<CartItemResponse> Items { get; set; } = [];
    }
}
