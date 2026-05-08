
namespace CatalogService.Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Image { get; private set; }
        public int? ParentCategoryId { get; private set; }
        public Category? ParentCategory { get; private set; }

        public Category()
        {
            //for entity Core
        }

        public Category(string name, string? image = null, int? parentCategoryId = null)
        {
            SetName(name);
            SetImage(image);
            SetParentCategory(parentCategoryId);
        }

        public void Update(string name, string? image = null, int? parentCategoryId = null)
        {
            SetName(name);
            SetImage(image);
            SetParentCategory(parentCategoryId);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category Name is required", nameof(name));
            name = name.Trim();

            if (name.Length > 50)
                throw new ArgumentException("Category Name max length is 50 characters", nameof(name));

            if (name.Contains('<') || name.Contains('>'))
                throw new ArgumentException("Category Name must be plain text", nameof(name));

            Name = name;
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

        private void SetParentCategory(int? parentCategoryId)
        {

            if (parentCategoryId.HasValue && parentCategoryId.Value <= 0)
                throw new ArgumentException("Parent category id must be greater than zero", nameof(parentCategoryId));

            ParentCategoryId = parentCategoryId;
        }

    }
}
