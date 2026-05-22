using ConversationHistoryService.Entities;

namespace ConversationHistoryService.Repositories;

public interface IConversationRepository
{
    Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Conversation>> GetAllAsync(CancellationToken ct = default);    // Admin
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Conversation> CreateAsync(Conversation conv, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Conversation> UpdateTimestampAsync(Guid id, CancellationToken ct = default);

}
