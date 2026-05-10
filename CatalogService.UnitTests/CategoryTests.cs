using CatalogService.Domain.Entities;

namespace CatalogService.UnitTests
{
    public class CategoryTests
    {
        [Fact]
        public void Constructor_ShouldCreateCategory_WhenValuesAreValid()
        {
            var category = new Category("cat1","https://url.com/cat1.png");

            Assert.NotNull(category);
            Assert.Equal("cat1",category.Name);
            Assert.Equal("https://url.com/cat1.png", category.Image);
            Assert.Null(category.ParentCategoryId);
        }

        //WhenNameIsEmpty, WhenNameContainsHTMLTags, WhenImageIsInvalidUrl
    }
}
