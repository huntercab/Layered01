using CartService.Domain.ValueObjects;

namespace CartService.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; }
        public string Name { get; }
        public ImageInfo? Image { get; }
        public Money Price { get; }
        public int Quantity { get; private set; }

        public CartItem(int id, string name, Money price, int quantity, ImageInfo? image = null)
        {
            if(id <= 0)
                throw new ArgumentException("Item id is required", nameof(id));

            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Item Name is required", nameof(name));

            if(quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            ArgumentNullException.ThrowIfNull(price);

            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
            Image = image;
        }

        public void IncreaseQuantity(int quantity)
        {
            if(quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            Quantity += quantity;
        }
    }
}
