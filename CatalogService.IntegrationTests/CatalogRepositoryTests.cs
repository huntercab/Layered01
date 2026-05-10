using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.IntegrationTests
{
    public class CatalogRepositoryTests : IDisposable
    {
        private readonly CatalogDbContext _catalogDbContext;
        private readonly string _databaseFilePath;

        public CatalogRepositoryTests()
        {
            var databaseName = $"CatalogServiceDb_{Guid.NewGuid():N}";

            var databaseFolder = Path.Combine(Path.GetTempPath(), "CatalogService.Tests");

            Directory.CreateDirectory(databaseFolder);

            _databaseFilePath = Path.Combine(databaseFolder, $"{databaseName}.mdf");

            var connectionString =
                $"Server=(LocalDB)\\MSSQLLocalDB;" +
                $"Integrated Security=true;" +
                $"Database={databaseName};" +
                $"AttachDbFilename={_databaseFilePath};" +
                $"Connect Timeout=30";

            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            _catalogDbContext = new CatalogDbContext(options);
            _catalogDbContext.Database.EnsureDeleted();
            _catalogDbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task HasSubCategoriesAsync_ShouldReturnTrue_WhenCategoryHasChildCategory()
        {
            var repository = new CategoryRepository(_catalogDbContext);
            var parent = new Category("parentCat1");
            await repository.AddAsync(parent);

            var child = new Category(name: "cat1", parentCategoryId: parent.Id);
            await repository.AddAsync(child);

            var result = await repository.HasSubCategoriesAsync(parent.Id);

            Assert.True(result);
        }

        public void Dispose()
        {
            _catalogDbContext.Database.EnsureDeleted();
            _catalogDbContext.Dispose();

            if (File.Exists(_databaseFilePath))
                File.Delete(_databaseFilePath);

            var logFile = Path.ChangeExtension(_databaseFilePath,".ldf");

            if (File.Exists(logFile))
                File.Delete(logFile);
        }
    }
}
