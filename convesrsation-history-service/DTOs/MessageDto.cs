namespace ConversationHistoryService.DTOs;

public record MessageDto(
    Guid       Id,
    string     SenderType,  // "User" | "AI"
    string     Content,
    DateTime   Timestamp
);
