using ConversationHistoryService.Entities;

namespace ConversationHistoryService.Repositories;

public interface IMessageRepository
{
    Task<Message> AddAsync(Message message, CancellationToken ct = default);
    Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
}
