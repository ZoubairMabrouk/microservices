namespace ConversationHistoryService.DTOs;

public record ConversationDto(
    Guid     Id,
    Guid     UserId,
    string   Title,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int      MessageCount
);
public record ConversationDetailDto(
    Guid              Id,
    Guid              UserId,
    string            Title,
    DateTime          CreatedAt,
    DateTime          UpdatedAt,
    List<MessageDto>  Messages
);
