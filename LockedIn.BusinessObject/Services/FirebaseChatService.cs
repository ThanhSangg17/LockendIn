using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Conversations;
using LockedIn.BusinessObject.Enums;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;

namespace LockedIn.BusinessObject.Services;

public class FirebaseChatService : IFirebaseChatService
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public FirebaseChatService(
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;

        var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];
        var projectId = configuration["Firebase:ProjectId"];

        Console.WriteLine($"Config ProjectId: '{projectId}'");
        Console.WriteLine($"ServiceAccountPath: '{serviceAccountPath}'");
        bool fileExists = !string.IsNullOrEmpty(serviceAccountPath) && System.IO.File.Exists(serviceAccountPath);
        Console.WriteLine($"FileExists: {fileExists}");

        if (fileExists)
        {
            try
            {
                var jsonContent = System.IO.File.ReadAllText(serviceAccountPath!);
                using (var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonContent))
                {
                    if (jsonDoc.RootElement.TryGetProperty("client_email", out var clientEmailProp))
                    {
                        Console.WriteLine($"Json client_email: '{clientEmailProp.GetString()}'");
                    }
                    else
                    {
                        Console.WriteLine("Json client_email: Not found in JSON");
                    }

                    if (jsonDoc.RootElement.TryGetProperty("project_id", out var projectIdProp))
                    {
                        Console.WriteLine($"Json project_id: '{projectIdProp.GetString()}'");
                    }
                    else
                    {
                        Console.WriteLine("Json project_id: Not found in JSON");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing service account JSON: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Firebase service account JSON: File not found or path is empty");
        }

        if (string.IsNullOrEmpty(serviceAccountPath))
        {
            throw new InvalidOperationException("Firebase service account path is not configured (Firebase:ServiceAccountPath).");
        }

        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Firebase project ID is not configured (Firebase:ProjectId).");
        }

        Console.WriteLine($"FirebaseApp.DefaultInstance exists before create: {FirebaseApp.DefaultInstance != null}");

        var credential = GoogleCredential.FromFile(serviceAccountPath!);

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }

        Console.WriteLine($"FirebaseApp.DefaultInstance exists after create: {FirebaseApp.DefaultInstance != null}");

        _firestoreDb = new FirestoreDbBuilder
        {
            ProjectId = projectId,
            Credential = credential
        }.Build();

        Console.WriteLine($"FirestoreDb.ProjectId: '{_firestoreDb.ProjectId}'");
    }

    public async Task<ApiResponse<ChatMessageResponse>> SendMessageAsync(SendMessageRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<ChatMessageResponse>.Fail("User is not authenticated.");
        }

        request.Content = request.Content?.Trim()!;

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return ApiResponse<ChatMessageResponse>.Fail("Message content is required.");
        }

        if (request.Content.Length > 1000)
        {
            return ApiResponse<ChatMessageResponse>.Fail("Message content cannot exceed 1000 characters.");
        }

        var conversation = await _unitOfWork.Conversations.Query()
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

        if (conversation == null)
        {
            return ApiResponse<ChatMessageResponse>.Fail("Conversation not found.");
        }

        var userId = _currentUserService.UserId.Value;
        var (allowed, error) = await CheckParticipantAccessAsync(conversation, userId);
        if (!allowed)
        {
            return ApiResponse<ChatMessageResponse>.Fail(error ?? "Access denied.");
        }

        var senderName = _currentUserService.FullName;
        if (string.IsNullOrWhiteSpace(senderName))
        {
            var dbUser = await _unitOfWork.Users.GetByIdAsync(userId);
            senderName = dbUser?.FullName ?? "Unknown";
        }

        var messageType = string.IsNullOrWhiteSpace(request.MessageType) ? "text" : request.MessageType;
        var messageId = Guid.NewGuid().ToString();

        var docRef = _firestoreDb.Collection("conversations")
            .Document(conversation.FirebaseConversationId)
            .Collection("messages")
            .Document(messageId);

        var createdAtTimestamp = Timestamp.GetCurrentTimestamp();

        var data = new Dictionary<string, object>
        {
            { "id", messageId },
            { "conversationId", conversation.Id.ToString() },
            { "senderUserId", userId.ToString() },
            { "senderName", senderName },
            { "content", request.Content },
            { "messageType", messageType },
            { "isRead", false },
            { "createdAt", createdAtTimestamp }
        };

        Console.WriteLine($"SQL Conversation ID: {conversation.Id}");
        Console.WriteLine($"SQL Conversation FirebaseConversationId: '{conversation.FirebaseConversationId}'");
        Console.WriteLine($"SQL Conversation CustomerId: '{conversation.CustomerId}'");
        Console.WriteLine($"SQL Conversation PtProfileId: '{conversation.PtProfileId}'");

        Console.WriteLine($"Current Authenticated User ID: '{_currentUserService.UserId}'");
        Console.WriteLine($"Current Authenticated User Role: '{_currentUserService.Role}'");
        Console.WriteLine($"Current Authenticated User Email: '{_currentUserService.Email}'");

        Console.WriteLine($"Collection path: conversations/{conversation.FirebaseConversationId}/messages/{messageId}");

        try
        {
            await docRef.SetAsync(data);
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"[FIRESTORE WRITE ERROR] Grpc.Core.RpcException thrown!");
            Console.WriteLine($"RpcException.StatusCode: {ex.StatusCode}");
            Console.WriteLine($"RpcException.Status.Detail: '{ex.Status.Detail}'");
            if (ex.Status.DebugException != null)
            {
                Console.WriteLine($"RpcException.Status.DebugException: '{ex.Status.DebugException}'");
            }
            else
            {
                Console.WriteLine("RpcException.Status.DebugException: Not available");
            }
            Console.WriteLine($"Full RpcException: {ex}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FIRESTORE WRITE ERROR] General Exception thrown!");
            Console.WriteLine($"Full Exception: {ex}");
            throw;
        }

        var response = new ChatMessageResponse
        {
            Id = messageId,
            ConversationId = conversation.Id,
            SenderUserId = userId,
            SenderName = senderName,
            Content = request.Content,
            MessageType = messageType,
            IsRead = false,
            CreatedAt = createdAtTimestamp.ToDateTime()
        };

        return ApiResponse<ChatMessageResponse>.Ok(response, "Message sent successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<ChatMessageResponse>>> GetMessagesAsync(Guid conversationId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<ChatMessageResponse>>.Fail("User is not authenticated.");
        }

        var conversation = await _unitOfWork.Conversations.Query()
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
        {
            return ApiResponse<IReadOnlyList<ChatMessageResponse>>.Fail("Conversation not found.");
        }

        var userId = _currentUserService.UserId.Value;
        var (allowed, error) = await CheckParticipantAccessAsync(conversation, userId);
        if (!allowed)
        {
            return ApiResponse<IReadOnlyList<ChatMessageResponse>>.Fail(error ?? "Access denied.");
        }

        var colRef = _firestoreDb.Collection("conversations")
            .Document(conversation.FirebaseConversationId)
            .Collection("messages");

        var snapshot = await colRef.OrderBy("createdAt").GetSnapshotAsync();

        var messages = new List<ChatMessageResponse>();
        foreach (var doc in snapshot.Documents)
        {
            var docData = doc.ToDictionary();

            Guid.TryParse(docData.GetValueOrDefault("conversationId")?.ToString(), out var parsedConvId);
            Guid.TryParse(docData.GetValueOrDefault("senderUserId")?.ToString(), out var parsedSenderId);

            DateTime createdAtDateTime = DateTime.UtcNow;
            if (docData.TryGetValue("createdAt", out var tsObj) && tsObj is Timestamp ts)
            {
                createdAtDateTime = ts.ToDateTime();
            }

            messages.Add(new ChatMessageResponse
            {
                Id = doc.Id,
                ConversationId = parsedConvId,
                SenderUserId = parsedSenderId,
                SenderName = docData.GetValueOrDefault("senderName")?.ToString() ?? "Unknown",
                Content = docData.GetValueOrDefault("content")?.ToString() ?? "",
                MessageType = docData.GetValueOrDefault("messageType")?.ToString() ?? "text",
                IsRead = docData.TryGetValue("isRead", out var isReadVal) && isReadVal is bool isRead && isRead,
                CreatedAt = createdAtDateTime
            });
        }

        return ApiResponse<IReadOnlyList<ChatMessageResponse>>.Ok(messages, "Messages retrieved successfully.");
    }

    public async Task<ApiResponse<string>> MarkMessagesAsReadAsync(Guid conversationId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<string>.Fail("User is not authenticated.");
        }

        var conversation = await _unitOfWork.Conversations.Query()
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
        {
            return ApiResponse<string>.Fail("Conversation not found.");
        }

        var userId = _currentUserService.UserId.Value;
        var (allowed, error) = await CheckParticipantAccessAsync(conversation, userId);
        if (!allowed)
        {
            return ApiResponse<string>.Fail(error ?? "Access denied.");
        }

        var colRef = _firestoreDb.Collection("conversations")
            .Document(conversation.FirebaseConversationId)
            .Collection("messages");

        var snapshot = await colRef.WhereEqualTo("isRead", false).GetSnapshotAsync();
        var batch = _firestoreDb.StartBatch();
        int updateCount = 0;
        var currentUserIdStr = userId.ToString();

        foreach (var doc in snapshot.Documents)
        {
            var docData = doc.ToDictionary();
            var senderUserIdVal = docData.GetValueOrDefault("senderUserId")?.ToString();
            if (senderUserIdVal != currentUserIdStr)
            {
                batch.Update(doc.Reference, "isRead", true);
                updateCount++;
            }
        }

        if (updateCount > 0)
        {
            await batch.CommitAsync();
        }

        return ApiResponse<string>.Ok($"Successfully marked {updateCount} messages as read.", "Messages marked as read successfully.");
    }

    private async Task<(bool Allowed, string? Error)> CheckParticipantAccessAsync(Conversation conversation, Guid userId)
    {
        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || customerProfile.Id != conversation.CustomerId)
            {
                return (false, "Access denied. You are not a customer participant of this conversation.");
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || ptProfile.Id != conversation.PtProfileId)
            {
                return (false, "Access denied. You are not a personal trainer participant of this conversation.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return (false, "Access denied. Invalid user role.");
        }

        return (true, null);
    }
}
