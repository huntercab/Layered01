using System;
using System.Collections.Generic;
using System.Text;
using CartService.DataAccess.Documents;
using CartService.DataAccess.Interfaces;
using CartService.DataAccess.Mappers;
using CartService.Domain.Entities;
using LiteDB;

namespace CartService.DataAccess.Repositories
{
    public class LiteDbCartRepository: ICartRepository
    {
        private const string CollectionName = "cart";
        private readonly string _connectionString;

        public LiteDbCartRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));

            _connectionString = connectionString;
        }

        public Task<Cart?> GetByIdAsync(Guid cartId)
        {
            using var database = new LiteDatabase(_connectionString);
            var collection = database.GetCollection<CartDocument>(CollectionName);

            var document = collection.FindById(cartId);

            return Task.FromResult(document is null ? null : CartMapper.ToDomain(document));
        }

        public Task SaveAsync(Cart cart) 
        { 
            ArgumentNullException.ThrowIfNull(cart);

            using var database = new LiteDatabase(_connectionString);
            var collection = database.GetCollection<CartDocument>(CollectionName);

            var document = CartMapper.ToDocument(cart);

            collection.Upsert(document);

            return Task.CompletedTask;
        }
    }
}
