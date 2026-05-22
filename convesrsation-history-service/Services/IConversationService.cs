using ConversationHistoryService.DTOs;

namespace ConversationHistoryService.Services;

public interface IConversationService
{

    Task<MessageDto> AddMessageAsync(Guid conversationId, Guid requesterId, bool isAdmin, AddMessageDto dto, CancellationToken ct);
    Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationDto dto, CancellationToken ct);
    Task<ConversationDetailDto> GetConversationAsync(Guid id, Guid requesterId, bool isAdmin, CancellationToken ct);
    Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(Guid userId, CancellationToken ct);
    Task DeleteConversationAsync(Guid id, Guid requesterId, bool isAdmin, CancellationToken ct);
    Task<IEnumerable<ConversationDto>> GetAllConversationsAsync(CancellationToken ct);

}
