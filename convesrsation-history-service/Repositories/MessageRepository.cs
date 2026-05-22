using ConversationHistoryService.Data;
using ConversationHistoryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConversationHistoryService.Repositories;

public class MessageRepository : IMessageRepository
{
private readonly ConversationDbContext _db;

    public MessageRepository(ConversationDbContext db) => _db = db;

    public async Task<Message> AddAsync(Message message, CancellationToken ct = default)
    {
        _db.Messages.Add(message);
        await _db.SaveChangesAsync(ct);
        return message;
    }

    public async Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default)
        => await _db.Messages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == conversationId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync(ct);
}
