using Xunit;
using Moq;
using ConversationHistoryService.DTOs;
using ConversationHistoryService.Entities;
using ConversationHistoryService.Repositories;
using ConversationHistoryService.Services;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace ConversationHistoryService.Tests;

public class ConversationServiceTests
{
    private readonly Mock<IConversationRepository> _mockConvRepo;
    private readonly Mock<IMessageRepository> _mockMsgRepo;
    private readonly Mock<ILogger<ConversationService>> _mockLogger;
    private readonly ConversationService _service;

    public ConversationServiceTests()
    {
        _mockConvRepo = new Mock<IConversationRepository>();
        _mockMsgRepo = new Mock<IMessageRepository>();
        _mockLogger = new Mock<ILogger<ConversationService>>();
        _service = new ConversationService(_mockConvRepo.Object, _mockMsgRepo.Object, _mockLogger.Object);
    }

    #region GetMyConversationsAsync Tests

    [Fact]
    public async Task GetMyConversationsAsync_WithValidUserId_ReturnsUserConversations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversations = new List<Conversation>
        {
            new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Conv 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<Message> { new Message { Id = Guid.NewGuid() } }
            },
            new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Conv 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<Message>()
            }
        };

        _mockConvRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);

        // Act
        var result = await _service.GetMyConversationsAsync(userId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Test Conv 1");
        result.Last().Title.Should().Be("Test Conv 2");
        result.First().MessageCount.Should().Be(1);
        result.Last().MessageCount.Should().Be(0);
        _mockConvRepo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyConversationsAsync_WithNoConversations_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockConvRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation>());

        // Act
        var result = await _service.GetMyConversationsAsync(userId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetConversationAsync Tests

    [Fact]
    public async Task GetConversationAsync_WithValidIdAndOwner_ReturnsConversationDetail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = userId,
            Title = "Test Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<Message>
            {
                new Message
                {
                    Id = messageId,
                    ConversationId = conversationId,
                    SenderType = SenderType.User,
                    Content = "Hello",
                    Timestamp = DateTime.UtcNow
                }
            }
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _service.GetConversationAsync(conversationId, userId, false, CancellationToken.None);

        // Assert
        result.Id.Should().Be(conversationId);
        result.Title.Should().Be("Test Conversation");
        result.Messages.Should().HaveCount(1);
        result.Messages.First().Content.Should().Be("Hello");
        result.Messages.First().SenderType.Should().Be("User");
    }

    [Fact]
    public async Task GetConversationAsync_WithNonOwnerUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = ownerId,
            Title = "Test Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<Message>()
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.GetConversationAsync(conversationId, userId, false, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetConversationAsync_WithAdminUser_ReturnsConversationEvenIfNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = ownerId,
            Title = "Test Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<Message>()
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _service.GetConversationAsync(conversationId, userId, true, CancellationToken.None);

        // Assert
        result.Id.Should().Be(conversationId);
        result.UserId.Should().Be(ownerId);
    }

    [Fact]
    public async Task GetConversationAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetConversationAsync(conversationId, userId, false, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetConversationAsync_MessagesSortedByTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = userId,
            Title = "Test Conversation",
            CreatedAt = now,
            UpdatedAt = now,
            Messages = new List<Message>
            {
                new Message { Id = Guid.NewGuid(), SenderType = SenderType.AI, Content = "Third", Timestamp = now.AddSeconds(3) },
                new Message { Id = Guid.NewGuid(), SenderType = SenderType.User, Content = "First", Timestamp = now.AddSeconds(1) },
                new Message { Id = Guid.NewGuid(), SenderType = SenderType.AI, Content = "Second", Timestamp = now.AddSeconds(2) }
            }
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act
        var result = await _service.GetConversationAsync(conversationId, userId, false, CancellationToken.None);

        // Assert
        result.Messages.Should().HaveCount(3);
        result.Messages[0].Content.Should().Be("First");
        result.Messages[1].Content.Should().Be("Second");
        result.Messages[2].Content.Should().Be("Third");
    }

    #endregion

    #region CreateConversationAsync Tests

    [Fact]
    public async Task CreateConversationAsync_WithValidData_CreatesConversation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateConversationDto { Title = "New Conversation" };
        var createdConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "New Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockConvRepo.Setup(r => r.CreateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdConversation);

        // Act
        var result = await _service.CreateConversationAsync(userId, createDto, CancellationToken.None);

        // Assert
        result.Id.Should().Be(createdConversation.Id);
        result.UserId.Should().Be(userId);
        result.Title.Should().Be("New Conversation");
        result.MessageCount.Should().Be(0);
        _mockConvRepo.Verify(r => r.CreateAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region AddMessageAsync Tests

    [Fact]
    public async Task AddMessageAsync_WithValidData_AddsMessageAndUpdatesTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = userId,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var addMessageDto = new AddMessageDto { SenderType = "User", Content = "Hello" };
        var createdMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderType = SenderType.User,
            Content = "Hello",
            Timestamp = DateTime.UtcNow
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _mockMsgRepo.Setup(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);
        _mockConvRepo.Setup(r => r.UpdateTimestampAsync(conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddMessageAsync(conversationId, userId, false, addMessageDto, CancellationToken.None);

        // Assert
        result.Id.Should().Be(createdMessage.Id);
        result.Content.Should().Be("Hello");
        result.SenderType.Should().Be("User");
        _mockMsgRepo.Verify(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockConvRepo.Verify(r => r.UpdateTimestampAsync(conversationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMessageAsync_WithNonOwnerUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = ownerId,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var addMessageDto = new AddMessageDto { SenderType = "User", Content = "Hello" };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.AddMessageAsync(conversationId, userId, false, addMessageDto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task AddMessageAsync_WithAdminUser_AllowsMessageAddition()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = ownerId,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var addMessageDto = new AddMessageDto { SenderType = "AI", Content = "Response" };
        var createdMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderType = SenderType.AI,
            Content = "Response",
            Timestamp = DateTime.UtcNow
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _mockMsgRepo.Setup(r => r.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);
        _mockConvRepo.Setup(r => r.UpdateTimestampAsync(conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddMessageAsync(conversationId, userId, true, addMessageDto, CancellationToken.None);

        // Assert
        result.Content.Should().Be("Response");
        result.SenderType.Should().Be("AI");
    }

    [Fact]
    public async Task AddMessageAsync_WithInvalidSenderType_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = userId,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var addMessageDto = new AddMessageDto { SenderType = "InvalidType", Content = "Hello" };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AddMessageAsync(conversationId, userId, false, addMessageDto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task AddMessageAsync_WithNonExistentConversation_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var addMessageDto = new AddMessageDto { SenderType = "User", Content = "Hello" };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddMessageAsync(conversationId, userId, false, addMessageDto, CancellationToken.None)
        );
    }

    #endregion

    #region DeleteConversationAsync Tests

    [Fact]
    public async Task DeleteConversationAsync_WithOwner_DeletesConversation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = userId,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _mockConvRepo.Setup(r => r.DeleteAsync(conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteConversationAsync(conversationId, userId, false, CancellationToken.None);

        // Assert
        _mockConvRepo.Verify(r => r.DeleteAsync(conversationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteConversationAsync_WithNonOwnerUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = ownerId,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.DeleteConversationAsync(conversationId, userId, false, CancellationToken.None)
        );

        _mockConvRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteConversationAsync_WithAdminUser_DeletesConversation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = ownerId,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _mockConvRepo.Setup(r => r.DeleteAsync(conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteConversationAsync(conversationId, userId, true, CancellationToken.None);

        // Assert
        _mockConvRepo.Verify(r => r.DeleteAsync(conversationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteConversationAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _mockConvRepo.Setup(r => r.GetByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteConversationAsync(conversationId, userId, false, CancellationToken.None)
        );
    }

    #endregion

    #region GetAllConversationsAsync Tests

    [Fact]
    public async Task GetAllConversationsAsync_ReturnsAllConversations()
    {
        // Arrange
        var conversations = new List<Conversation>
        {
            new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Conv 1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<Message> { new Message { Id = Guid.NewGuid() } }
            },
            new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = "Conv 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<Message>()
            }
        };

        _mockConvRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);

        // Act
        var result = await _service.GetAllConversationsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Conv 1");
        result.Last().Title.Should().Be("Conv 2");
        result.First().MessageCount.Should().Be(1);
        result.Last().MessageCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllConversationsAsync_WithNoConversations_ReturnsEmptyList()
    {
        // Arrange
        _mockConvRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation>());

        // Act
        var result = await _service.GetAllConversationsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
