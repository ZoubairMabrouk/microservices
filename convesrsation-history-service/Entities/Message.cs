namespace ConversationHistoryService.Entities;


public enum SenderType { User, AI }
public class Message
{
 public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public SenderType SenderType { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Conversation Conversation { get; set; } = null!;

}
