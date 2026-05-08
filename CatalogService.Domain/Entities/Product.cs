using CatalogService.Domain.ValueObject;
using System.Xml.Linq;

namespace CatalogService.Domain.Entities
{
    public class Product
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = null;
        public string? Description { get; private set; }
        public string? Image { get; private set; }
        public int CategoryId { get; private set; }
        public Category Category { get; private set; } = null!;
        public Money Price { get; private set; } = null!;
        public int Amount { get; private set; }

        private Product()
        {
            //EF Core
        }

        public Product(string name, int categoryId, Money price, int amount, string? description = null, string? image = null)
        {
            SetName(name);
            SetCategory(categoryId);
            SetPrice(price);
            SetAmount(amount);
            SetDescription(description);
            SetImage(image);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product Name is required", nameof(name));
            name = name.Trim();

            if (name.Length > 50)
                throw new ArgumentException("Product Name max length is 50 characters", nameof(name));

            if (name.Contains('<') || name.Contains('>'))
                throw new ArgumentException("Product Name must be plain text", nameof(name));

            Name = name;
        }

        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        private void SetImage(string? image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                Image = null;
                return;
            }

            image = image.Trim();

            if (!Uri.TryCreate(image, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid Image Url", nameof(image));

            Image = image;
        }

        private void SetCategory(int categoryId)
        {

            if (categoryId <= 0)
                throw new ArgumentException("Parent category id must be greater than zero", nameof(categoryId));

            CategoryId = categoryId;
        }

        private void SetPrice(Money price)
        {
            Price = price ?? throw new ArgumentException("Price is required", nameof(price));
        }

        private void SetAmount(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Product amount must be a positive integer");

            Amount = amount;
        }
    }
}
