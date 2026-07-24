
namespace CartService.Domain;

public sealed class InboxMessage
{
    private InboxMessage()
    {
    }

    public InboxMessage(
        Guid messageId,
        DateTimeOffset processedOnUtc)
    {
        MessageId = messageId;
        ProcessedOnUtc = processedOnUtc;
    }

    public Guid MessageId { get; private set; }
    public DateTimeOffset ProcessedOnUtc { get; private set; }
}
