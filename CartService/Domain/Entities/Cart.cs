
namespace CartService.Domain.Entities;

public class Cart
{
    private readonly List<CartItem> _items = new();
    public Guid Id { get; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public Cart(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentNullException("Cart id is required", nameof(id));
        }

        Id = id;
    }

    public void AddItem(CartItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var exisitngItem = _items.FirstOrDefault(x => x.Id == item.Id);

        if (exisitngItem is null)
        {
            _items.Add(item);
            return;
        }

        exisitngItem.IncreaseQuantity(item.Quantity);
    }

    public void RemoveItem(int ItemId)
    {
        if (ItemId <= 0)
        {
            throw new ArgumentException("Item id is required", nameof(ItemId));
        }

        _items.RemoveAll(x => x.Id == ItemId);
    }
}
