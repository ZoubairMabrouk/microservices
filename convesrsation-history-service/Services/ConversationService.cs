using ConversationHistoryService.DTOs;
using ConversationHistoryService.Entities;
using ConversationHistoryService.Repositories;

namespace ConversationHistoryService.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _convRepo;
    private readonly IMessageRepository      _msgRepo;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        IConversationRepository convRepo,
        IMessageRepository msgRepo,
        ILogger<ConversationService> logger)
    {
        _convRepo = convRepo;
        _msgRepo  = msgRepo;
        _logger   = logger;
    }

    // ── User operations ──────────────────────────────────────────

    /// <summary>Retourne les conversations de l'utilisateur connecté uniquement.</summary>
    public async Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(Guid userId, CancellationToken ct)
    {
        var convs = await _convRepo.GetByUserIdAsync(userId, ct);
        return convs.Select(c => new ConversationDto(
            c.Id, c.UserId, c.Title, c.CreatedAt, c.UpdatedAt, c.Messages.Count));
    }

    /// <summary>Retourne une conversation en vérifiant l'appartenance.</summary>
    public async Task<ConversationDetailDto> GetConversationAsync(Guid id, Guid requesterId, bool isAdmin, CancellationToken ct)
    {
        var conv = await _convRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Conversation {id} not found.");

        // Sécurité : un user ne peut voir que ses propres conversations
        if (!isAdmin && conv.UserId != requesterId)
            throw new UnauthorizedAccessException("Access denied.");

        var messages = conv.Messages
            .OrderBy(m => m.Timestamp)
            .Select(m => new MessageDto(m.Id, m.SenderType.ToString(), m.Content, m.Timestamp))
            .ToList();

        return new ConversationDetailDto(conv.Id, conv.UserId, conv.Title, conv.CreatedAt, conv.UpdatedAt, messages);
    }

    public async Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationDto dto, CancellationToken ct)
    {
        var conv = new Conversation
        {
            UserId    = userId,
            Title     = dto.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await _convRepo.CreateAsync(conv, ct);
        _logger.LogInformation("Conversation created by {UserId}: {ConvId}", userId, created.Id);
        return new ConversationDto(created.Id, created.UserId, created.Title, created.CreatedAt, created.UpdatedAt, 0);
    }

    public async Task<MessageDto> AddMessageAsync(Guid conversationId, Guid requesterId, bool isAdmin, AddMessageDto dto, CancellationToken ct)
    {
        var conv = await _convRepo.GetByIdAsync(conversationId, ct)
            ?? throw new KeyNotFoundException($"Conversation {conversationId} not found.");

        if (!isAdmin && conv.UserId != requesterId)
            throw new UnauthorizedAccessException("Access denied.");

        if (!Enum.TryParse<SenderType>(dto.SenderType, out var senderType))
            throw new ArgumentException($"Invalid SenderType: {dto.SenderType}");

        var message = new Message
        {
            ConversationId = conversationId,
            SenderType     = senderType,
            Content        = dto.Content,
            Timestamp      = DateTime.UtcNow
        };

        var added = await _msgRepo.AddAsync(message, ct);
        await _convRepo.UpdateTimestampAsync(conversationId, ct);

        return new MessageDto(added.Id, added.SenderType.ToString(), added.Content, added.Timestamp);
    }

    public async Task DeleteConversationAsync(Guid id, Guid requesterId, bool isAdmin, CancellationToken ct)
    {
        var conv = await _convRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Conversation {id} not found.");

        if (!isAdmin && conv.UserId != requesterId)
            throw new UnauthorizedAccessException("Access denied.");

        await _convRepo.DeleteAsync(id, ct);
    }

    // ── Admin operations ─────────────────────────────────────────

    /// <summary>Admin uniquement : toutes les conversations de tous les utilisateurs.</summary>
    public async Task<IEnumerable<ConversationDto>> GetAllConversationsAsync(CancellationToken ct)
    {
        var convs = await _convRepo.GetAllAsync(ct);
        return convs.Select(c => new ConversationDto(
            c.Id, c.UserId, c.Title, c.CreatedAt, c.UpdatedAt, c.Messages.Count));
    }
}
