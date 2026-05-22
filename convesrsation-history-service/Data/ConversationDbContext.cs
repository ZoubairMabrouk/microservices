using ConversationHistoryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConversationHistoryService.Data;

public class ConversationDbContext : DbContext
{
    public ConversationDbContext(DbContextOptions<ConversationDbContext> options)
        : base(options) { }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Conversation
        modelBuilder.Entity<Conversation>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Title).HasMaxLength(200).IsRequired();
            e.Property(c => c.UserId).IsRequired();
            e.HasIndex(c => c.UserId);   // Performance
        });

        // Message
        modelBuilder.Entity<Message>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Content).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(m => m.SenderType)
             .HasConversion<string>()   // Stocké comme "User" | "AI"
             .HasMaxLength(10);

            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}