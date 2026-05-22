using ConversationHistoryService.Data;
using ConversationHistoryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConversationHistoryService.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly ConversationDbContext _db;

    public ConversationRepository(ConversationDbContext db) => _db = db;

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Conversations
                    .AsNoTracking()
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync(ct);

    public async Task<IEnumerable<Conversation>> GetAllAsync(CancellationToken ct = default)
        => await _db.Conversations
                    .AsNoTracking()
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync(ct);

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Conversation> CreateAsync(Conversation conv, CancellationToken ct = default)
    {
        _db.Conversations.Add(conv);
        await _db.SaveChangesAsync(ct);
        return conv;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var conv = await _db.Conversations.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Conversation {id} not found.");
        _db.Conversations.Remove(conv);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Conversation> UpdateTimestampAsync(Guid id, CancellationToken ct = default)
    {
        var conv = await _db.Conversations.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Conversation {id} not found.");
        conv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return conv;
    }
}
