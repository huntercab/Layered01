using CartService.Business.Interfaces;
using CartService.Domain;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CartService.DataAccess.Repositories
{
    public sealed class LiteDbInboxRepository : IInboxRepository
    {
        private readonly ILiteDatabase _database;

        public LiteDbInboxRepository(ILiteDatabase database)
        {
            _database = database;
        }

        public Task<bool> ExistsAsync(
            Guid messageId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ILiteCollection<InboxMessage> collection =
                _database.GetCollection<InboxMessage>(
                    "inbox_messages");

            return Task.FromResult(
                collection.Exists(x => x.MessageId == messageId));
        }

        public Task AddAsync(
            Guid messageId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ILiteCollection<InboxMessage> collection =
                _database.GetCollection<InboxMessage>(
                    "inbox_messages");

            collection.EnsureIndex(
                x => x.MessageId,
                unique: true);

            collection.Insert(
                new InboxMessage(
                    messageId,
                    DateTimeOffset.UtcNow));

            return Task.CompletedTask;
        }
    }
}
