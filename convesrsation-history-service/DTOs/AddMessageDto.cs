namespace ConversationHistoryService.DTOs;

public record AddMessageDto(
    string SenderType,   // "User" | "AI"
    string Content
);