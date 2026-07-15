using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Domain.Outbox
{
    public sealed class OutboxMessage
    {
        private OutboxMessage()
        {
        }

        public OutboxMessage(
            Guid id,
            string type,
            string content,
            DateTimeOffset occurredOnUtc)
        {
            Id = id;
            Type = type;
            Content = content;
            OccurredOnUtc = occurredOnUtc;
        }

        public Guid Id { get; private set; }
        public string Type { get; private set; } = null!;
        public string Content { get; private set; } = null!;
        public DateTimeOffset OccurredOnUtc { get; private set; }

        public DateTimeOffset? ProcessedOnUtc { get; private set; }
        public string? Error { get; private set; }
        public int Attempts { get; private set; }

        public void MarkAsProcessed(DateTimeOffset processedOnUtc)
        {
            ProcessedOnUtc = processedOnUtc;
            Error = null;
        }

        public void MarkAsFailed(string error)
        {
            Attempts++;
            Error = error;
        }
    }
}
