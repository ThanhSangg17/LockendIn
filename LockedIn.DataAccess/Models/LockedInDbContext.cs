using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LockedIn.DataAccess.Models;

public partial class LockedInDbContext : DbContext
{
    public LockedInDbContext()
    {
    }

    public LockedInDbContext(DbContextOptions<LockedInDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiUsageLog> AiUsageLogs { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<CustomerProfile> CustomerProfiles { get; set; }

    public virtual DbSet<Dispute> Disputes { get; set; }

    public virtual DbSet<DisputeEvidence> DisputeEvidences { get; set; }

    public virtual DbSet<MealPlan> MealPlans { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; }

    public virtual DbSet<PtDocument> PtDocuments { get; set; }

    public virtual DbSet<PtProfile> PtProfiles { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workspace> Workspaces { get; set; }

    public virtual DbSet<WorkspaceSession> WorkspaceSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiUsageLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ai_usage__3213E83F94E74880");

            entity.ToTable("ai_usage_logs");

            entity.HasIndex(e => e.CreatedAt, "ix_ai_usage_logs_created_at");

            entity.HasIndex(e => e.PtProfileId, "ix_ai_usage_logs_pt_profile_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Feature)
                .HasMaxLength(100)
                .HasColumnName("feature");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.TokenUsed).HasColumnName("token_used");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.AiUsageLogs)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ai_usage_logs_pt_profile_id");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audit_lo__3213E83F4EF56003");

            entity.ToTable("audit_logs");

            entity.HasIndex(e => e.ActorUserId, "ix_audit_logs_actor_user_id");

            entity.HasIndex(e => e.CreatedAt, "ix_audit_logs_created_at");

            entity.HasIndex(e => new { e.EntityName, e.EntityId }, "ix_audit_logs_entity");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(150)
                .HasColumnName("action");
            entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityName)
                .HasMaxLength(150)
                .HasColumnName("entity_name");
            entity.Property(e => e.MetadataJson).HasColumnName("metadata_json");

            entity.HasOne(d => d.ActorUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActorUserId)
                .HasConstraintName("fk_audit_logs_actor_user_id");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bookings__3213E83FD21291C7");

            entity.ToTable("bookings");

            entity.HasIndex(e => e.CreatedAt, "ix_bookings_created_at");

            entity.HasIndex(e => e.CustomerId, "ix_bookings_customer_id");

            entity.HasIndex(e => e.PackageId, "ix_bookings_package_id");

            entity.HasIndex(e => e.PtProfileId, "ix_bookings_pt_profile_id");

            entity.HasIndex(e => e.Status, "ix_bookings_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.SessionCount).HasColumnName("session_count");
            entity.Property(e => e.SettlementDueAt).HasColumnName("settlement_due_at");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_customer_id");

            entity.HasOne(d => d.Package).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_package_id");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookings_pt_profile_id");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__conversa__3213E83F4A447B13");

            entity.ToTable("conversations");

            entity.HasIndex(e => e.BookingId, "UQ__conversa__5DE3A5B065808E54").IsUnique();

            entity.HasIndex(e => e.FirebaseConversationId, "UQ__conversa__C704B367F57EC45D").IsUnique();

            entity.HasIndex(e => e.BookingId, "ux_conversations_booking_id").IsUnique();

            entity.HasIndex(e => e.FirebaseConversationId, "ux_conversations_firebase_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.FirebaseConversationId)
                .HasMaxLength(255)
                .HasColumnName("firebase_conversation_id");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");

            entity.HasOne(d => d.Booking).WithOne(p => p.Conversation)
                .HasForeignKey<Conversation>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversations_booking_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversations_customer_id");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversations_pt_profile_id");
        });

        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__customer__3213E83F7EEE07E2");

            entity.ToTable("customer_profiles");

            entity.HasIndex(e => e.UserId, "UQ__customer__B9BE370E5DAC7E13").IsUnique();

            entity.HasIndex(e => e.IsDeleted, "ix_customer_profiles_is_deleted");

            entity.HasIndex(e => e.UserId, "ux_customer_profiles_user_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.FitnessGoal)
                .HasMaxLength(255)
                .HasColumnName("fitness_goal");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.HealthNote).HasColumnName("health_note");
            entity.Property(e => e.HeightCm)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("height_cm");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WeightKg)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weight_kg");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.CustomerProfileDeletedByNavigations)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("fk_customer_profiles_deleted_by");

            entity.HasOne(d => d.User).WithOne(p => p.CustomerProfileUser)
                .HasForeignKey<CustomerProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customer_profiles_user_id");
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__disputes__3213E83F9BA14776");

            entity.ToTable("disputes");

            entity.HasIndex(e => e.BookingId, "ix_disputes_booking_id");

            entity.HasIndex(e => e.CreatedAt, "ix_disputes_created_at");

            entity.HasIndex(e => e.CustomerId, "ix_disputes_customer_id");

            entity.HasIndex(e => e.PtProfileId, "ix_disputes_pt_profile_id");

            entity.HasIndex(e => e.Status, "ix_disputes_status");

            entity.HasIndex(e => e.BookingId, "ux_disputes_one_open_per_booking")
                .IsUnique()
                .HasFilter("([status] IN ((1), (2), (3)))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.ResolutionNote).HasColumnName("resolution_note");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedByAdminId).HasColumnName("resolved_by_admin_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Booking).WithOne(p => p.Dispute)
                .HasForeignKey<Dispute>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disputes_booking_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disputes_customer_id");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_disputes_pt_profile_id");

            entity.HasOne(d => d.ResolvedByAdmin).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.ResolvedByAdminId)
                .HasConstraintName("fk_disputes_resolved_by_admin_id");
        });

        modelBuilder.Entity<DisputeEvidence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__dispute___3213E83F1F7FC76C");

            entity.ToTable("dispute_evidences");

            entity.HasIndex(e => e.DisputeId, "ix_dispute_evidences_dispute_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .HasColumnName("file_type");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Dispute).WithMany(p => p.DisputeEvidences)
                .HasForeignKey(d => d.DisputeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dispute_evidences_dispute_id");
        });

        modelBuilder.Entity<MealPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__meal_pla__3213E83FE8434E9A");

            entity.ToTable("meal_plans");

            entity.HasIndex(e => e.CreatedByPtId, "ix_meal_plans_created_by_pt_id");

            entity.HasIndex(e => e.IsActive, "ix_meal_plans_is_active");

            entity.HasIndex(e => e.IsDeleted, "ix_meal_plans_is_deleted");

            entity.HasIndex(e => e.WorkspaceId, "ix_meal_plans_workspace_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ContentJson).HasColumnName("content_json");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByPtId).HasColumnName("created_by_pt_id");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id");

            entity.HasOne(d => d.CreatedByPt).WithMany(p => p.MealPlans)
                .HasForeignKey(d => d.CreatedByPtId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_meal_plans_created_by_pt_id");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.MealPlans)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("fk_meal_plans_deleted_by");

            entity.HasOne(d => d.Workspace).WithMany(p => p.MealPlans)
                .HasForeignKey(d => d.WorkspaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_meal_plans_workspace_id");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FD17F67A2");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.CreatedAt, "ix_notifications_created_at");

            entity.HasIndex(e => e.IsDeleted, "ix_notifications_is_deleted");

            entity.HasIndex(e => e.IsRead, "ix_notifications_is_read");

            entity.HasIndex(e => e.UserId, "ix_notifications_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.NotificationDeletedByNavigations)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("fk_notifications_deleted_by");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notifications_user_id");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__packages__3213E83F330FC9D4");

            entity.ToTable("packages");

            entity.HasIndex(e => e.IsActive, "ix_packages_is_active");

            entity.HasIndex(e => e.IsDeleted, "ix_packages_is_deleted");

            entity.HasIndex(e => e.Price, "ix_packages_price");

            entity.HasIndex(e => e.PtProfileId, "ix_packages_pt_profile_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("price");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.SessionCount).HasColumnName("session_count");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.Packages)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("fk_packages_deleted_by");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.Packages)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_packages_pt_profile_id");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83FEDB87C2A");

            entity.ToTable("payments");

            entity.HasIndex(e => e.OrderCode, "UQ__payments__99D12D3FD8197709").IsUnique();

            entity.HasIndex(e => e.BookingId, "ix_payments_booking_id");

            entity.HasIndex(e => e.Status, "ix_payments_status");

            entity.HasIndex(e => e.BookingId, "ux_payments_one_success_per_booking")
                .IsUnique()
                .HasFilter("([status]=(2))");

            entity.HasIndex(e => e.OrderCode, "ux_payments_order_code").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CheckoutUrl).HasColumnName("checkout_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.OrderCode)
                .HasMaxLength(100)
                .HasColumnName("order_code");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .HasDefaultValue("PayOS")
                .HasColumnName("provider");
            entity.Property(e => e.ProviderTransactionId)
                .HasMaxLength(255)
                .HasColumnName("provider_transaction_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payments_booking_id");
        });

        modelBuilder.Entity<PaymentWebhookLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payment___3213E83F77B7BFB3");

            entity.ToTable("payment_webhook_logs");

            entity.HasIndex(e => e.EventId, "ix_payment_webhook_logs_event_id");

            entity.HasIndex(e => e.PaymentId, "ix_payment_webhook_logs_payment_id");

            entity.HasIndex(e => e.ReceivedAt, "ix_payment_webhook_logs_received_at");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EventId)
                .HasMaxLength(255)
                .HasColumnName("event_id");
            entity.Property(e => e.EventType)
                .HasMaxLength(100)
                .HasColumnName("event_type");
            entity.Property(e => e.IsValidSignature).HasColumnName("is_valid_signature");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .HasDefaultValue("PayOS")
                .HasColumnName("provider");
            entity.Property(e => e.RawPayload).HasColumnName("raw_payload");
            entity.Property(e => e.ReceivedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("received_at");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentWebhookLogs)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("fk_payment_webhook_logs_payment_id");
        });

        modelBuilder.Entity<PtDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pt_docum__3213E83F008BB1B4");

            entity.ToTable("pt_documents");

            entity.HasIndex(e => e.DocumentType, "ix_pt_documents_document_type");

            entity.HasIndex(e => e.PtProfileId, "ix_pt_documents_pt_profile_id");

            entity.HasIndex(e => e.Status, "ix_pt_documents_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.PtDocuments)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pt_documents_pt_profile_id");
        });

        modelBuilder.Entity<PtProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pt_profi__3213E83F912EDD79");

            entity.ToTable("pt_profiles");

            entity.HasIndex(e => e.UserId, "UQ__pt_profi__B9BE370E276F861C").IsUnique();

            entity.HasIndex(e => e.AverageRating, "ix_pt_profiles_average_rating");

            entity.HasIndex(e => e.IsDeleted, "ix_pt_profiles_is_deleted");

            entity.HasIndex(e => e.VerificationStatus, "ix_pt_profiles_verification_status");

            entity.HasIndex(e => e.UserId, "ux_pt_profiles_user_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.AverageRating)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("average_rating");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Specialization)
                .HasMaxLength(255)
                .HasColumnName("specialization");
            entity.Property(e => e.TotalReviews).HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerificationStatus)
                .HasDefaultValue(1)
                .HasColumnName("verification_status");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.PtProfileDeletedByNavigations)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("fk_pt_profiles_deleted_by");

            entity.HasOne(d => d.User).WithOne(p => p.PtProfileUser)
                .HasForeignKey<PtProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pt_profiles_user_id");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__refresh___3213E83FB90CC2E0");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.ExpiresAt, "ix_refresh_tokens_expires_at");

            entity.HasIndex(e => e.UserId, "ix_refresh_tokens_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_refresh_tokens_user_id");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reviews__3213E83FC246B2B0");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.BookingId, "UQ__reviews__5DE3A5B06B553922").IsUnique();

            entity.HasIndex(e => e.CustomerId, "ix_reviews_customer_id");

            entity.HasIndex(e => e.IsHidden, "ix_reviews_is_hidden");

            entity.HasIndex(e => e.PtProfileId, "ix_reviews_pt_profile_id");

            entity.HasIndex(e => e.BookingId, "ux_reviews_booking_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.IsHidden).HasColumnName("is_hidden");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Booking).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_booking_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_customer_id");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reviews_pt_profile_id");
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__settleme__3213E83FC82FEC5F");

            entity.ToTable("settlements");

            entity.HasIndex(e => e.BookingId, "UQ__settleme__5DE3A5B0FE34E5F5").IsUnique();

            entity.HasIndex(e => e.PtProfileId, "ix_settlements_pt_profile_id");

            entity.HasIndex(e => e.Status, "ix_settlements_status");

            entity.HasIndex(e => e.BookingId, "ux_settlements_booking_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GrossAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("gross_amount");
            entity.Property(e => e.NetAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("net_amount");
            entity.Property(e => e.PlatformFee)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("platform_fee");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.SettledAt).HasColumnName("settled_at");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Booking).WithOne(p => p.Settlement)
                .HasForeignKey<Settlement>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlements_booking_id");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_settlements_pt_profile_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FD1C72651");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164EEF4CB86").IsUnique();

            entity.HasIndex(e => e.Email, "ix_users_email");

            entity.HasIndex(e => e.IsDeleted, "ix_users_is_deleted");

            entity.HasIndex(e => e.Role, "ix_users_role");

            entity.HasIndex(e => e.Status, "ix_users_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.InverseDeletedByNavigation)
                .HasForeignKey(d => d.DeletedBy)
                .HasConstraintName("fk_users_deleted_by");
        });

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__workspac__3213E83FDD6DCD60");

            entity.ToTable("workspaces");

            entity.HasIndex(e => e.BookingId, "UQ__workspac__5DE3A5B0181B9644").IsUnique();

            entity.HasIndex(e => e.CustomerId, "ix_workspaces_customer_id");

            entity.HasIndex(e => e.PtProfileId, "ix_workspaces_pt_profile_id");

            entity.HasIndex(e => e.Status, "ix_workspaces_status");

            entity.HasIndex(e => e.BookingId, "ux_workspaces_booking_id").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CourseNote).HasColumnName("course_note");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.PtProfileId).HasColumnName("pt_profile_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Booking).WithOne(p => p.Workspace)
                .HasForeignKey<Workspace>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workspaces_booking_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Workspaces)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workspaces_customer_id");

            entity.HasOne(d => d.PtProfile).WithMany(p => p.Workspaces)
                .HasForeignKey(d => d.PtProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workspaces_pt_profile_id");
        });

        modelBuilder.Entity<WorkspaceSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__workspace_sessions__3213E83FD1C72652");

            entity.ToTable("workspace_sessions");

            entity.HasIndex(e => new { e.WorkspaceId, e.SessionNumber }, "uq_workspace_sessions_workspace_id_session_number").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id");
            entity.Property(e => e.SessionNumber).HasColumnName("session_number");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Workspace).WithMany(p => p.WorkspaceSessions)
                .HasForeignKey(d => d.WorkspaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workspace_sessions_workspace_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
