IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'LockedInDB')
BEGIN
    CREATE DATABASE [LockedInDB];
END;
GO

USE [LockedInDB];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [users] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [email] nvarchar(255) NOT NULL,
        [password_hash] nvarchar(max) NOT NULL,
        [full_name] nvarchar(150) NOT NULL,
        [phone] nvarchar(20) NULL,
        [avatar_url] nvarchar(max) NULL,
        [role] int NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [email_verified] bit NOT NULL,
        [is_deleted] bit NOT NULL,
        [deleted_at] datetime2 NULL,
        [deleted_by] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__users__3213E83FD1C72651] PRIMARY KEY ([id]),
        CONSTRAINT [fk_users_deleted_by] FOREIGN KEY ([deleted_by]) REFERENCES [users] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [audit_logs] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [actor_user_id] uniqueidentifier NULL,
        [action] nvarchar(150) NOT NULL,
        [entity_name] nvarchar(150) NULL,
        [entity_id] uniqueidentifier NULL,
        [metadata_json] nvarchar(max) NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__audit_lo__3213E83F4EF56003] PRIMARY KEY ([id]),
        CONSTRAINT [fk_audit_logs_actor_user_id] FOREIGN KEY ([actor_user_id]) REFERENCES [users] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [customer_profiles] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [user_id] uniqueidentifier NOT NULL,
        [date_of_birth] date NULL,
        [gender] nvarchar(20) NULL,
        [height_cm] decimal(5,2) NULL,
        [weight_kg] decimal(5,2) NULL,
        [fitness_goal] nvarchar(255) NULL,
        [health_note] nvarchar(max) NULL,
        [is_deleted] bit NOT NULL,
        [deleted_at] datetime2 NULL,
        [deleted_by] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__customer__3213E83F7EEE07E2] PRIMARY KEY ([id]),
        CONSTRAINT [fk_customer_profiles_deleted_by] FOREIGN KEY ([deleted_by]) REFERENCES [users] ([id]),
        CONSTRAINT [fk_customer_profiles_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [notifications] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [user_id] uniqueidentifier NOT NULL,
        [title] nvarchar(255) NOT NULL,
        [content] nvarchar(max) NOT NULL,
        [type] int NOT NULL,
        [is_read] bit NOT NULL,
        [is_deleted] bit NOT NULL,
        [deleted_at] datetime2 NULL,
        [deleted_by] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__notifica__3213E83FD17F67A2] PRIMARY KEY ([id]),
        CONSTRAINT [fk_notifications_deleted_by] FOREIGN KEY ([deleted_by]) REFERENCES [users] ([id]),
        CONSTRAINT [fk_notifications_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [pt_profiles] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [user_id] uniqueidentifier NOT NULL,
        [bio] nvarchar(max) NULL,
        [specialization] nvarchar(255) NULL,
        [experience_years] int NOT NULL,
        [verification_status] int NOT NULL DEFAULT 1,
        [average_rating] decimal(3,2) NOT NULL,
        [total_reviews] int NOT NULL,
        [approved_at] datetime2 NULL,
        [is_deleted] bit NOT NULL,
        [deleted_at] datetime2 NULL,
        [deleted_by] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__pt_profi__3213E83F912EDD79] PRIMARY KEY ([id]),
        CONSTRAINT [fk_pt_profiles_deleted_by] FOREIGN KEY ([deleted_by]) REFERENCES [users] ([id]),
        CONSTRAINT [fk_pt_profiles_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [refresh_tokens] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [user_id] uniqueidentifier NOT NULL,
        [token_hash] nvarchar(max) NOT NULL,
        [expires_at] datetime2 NOT NULL,
        [revoked_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__refresh___3213E83FB90CC2E0] PRIMARY KEY ([id]),
        CONSTRAINT [fk_refresh_tokens_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [ai_usage_logs] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [feature] nvarchar(100) NOT NULL,
        [token_used] int NOT NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__ai_usage__3213E83F94E74880] PRIMARY KEY ([id]),
        CONSTRAINT [fk_ai_usage_logs_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [packages] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [name] nvarchar(150) NOT NULL,
        [description] nvarchar(max) NULL,
        [session_count] int NOT NULL,
        [price] decimal(12,2) NOT NULL,
        [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
        [is_deleted] bit NOT NULL,
        [deleted_at] datetime2 NULL,
        [deleted_by] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__packages__3213E83F330FC9D4] PRIMARY KEY ([id]),
        CONSTRAINT [fk_packages_deleted_by] FOREIGN KEY ([deleted_by]) REFERENCES [users] ([id]),
        CONSTRAINT [fk_packages_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [pt_documents] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [document_type] int NOT NULL,
        [file_url] nvarchar(max) NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [uploaded_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__pt_docum__3213E83F008BB1B4] PRIMARY KEY ([id]),
        CONSTRAINT [fk_pt_documents_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [bookings] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [customer_id] uniqueidentifier NOT NULL,
        [pt_profile_id] uniqueidentifier NOT NULL,
        [package_id] uniqueidentifier NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [total_amount] decimal(12,2) NOT NULL,
        [session_count] int NOT NULL,
        [paid_at] datetime2 NULL,
        [started_at] datetime2 NULL,
        [completed_at] datetime2 NULL,
        [settlement_due_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__bookings__3213E83FD21291C7] PRIMARY KEY ([id]),
        CONSTRAINT [fk_bookings_customer_id] FOREIGN KEY ([customer_id]) REFERENCES [customer_profiles] ([id]),
        CONSTRAINT [fk_bookings_package_id] FOREIGN KEY ([package_id]) REFERENCES [packages] ([id]),
        CONSTRAINT [fk_bookings_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [conversations] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [booking_id] uniqueidentifier NOT NULL,
        [customer_id] uniqueidentifier NOT NULL,
        [pt_profile_id] uniqueidentifier NOT NULL,
        [firebase_conversation_id] nvarchar(255) NOT NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__conversa__3213E83F4A447B13] PRIMARY KEY ([id]),
        CONSTRAINT [fk_conversations_booking_id] FOREIGN KEY ([booking_id]) REFERENCES [bookings] ([id]),
        CONSTRAINT [fk_conversations_customer_id] FOREIGN KEY ([customer_id]) REFERENCES [customer_profiles] ([id]),
        CONSTRAINT [fk_conversations_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [disputes] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [booking_id] uniqueidentifier NOT NULL,
        [customer_id] uniqueidentifier NOT NULL,
        [pt_profile_id] uniqueidentifier NOT NULL,
        [reason] nvarchar(255) NOT NULL,
        [description] nvarchar(max) NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [resolution_note] nvarchar(max) NULL,
        [resolved_by_admin_id] uniqueidentifier NULL,
        [resolved_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__disputes__3213E83F9BA14776] PRIMARY KEY ([id]),
        CONSTRAINT [fk_disputes_booking_id] FOREIGN KEY ([booking_id]) REFERENCES [bookings] ([id]),
        CONSTRAINT [fk_disputes_customer_id] FOREIGN KEY ([customer_id]) REFERENCES [customer_profiles] ([id]),
        CONSTRAINT [fk_disputes_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id]),
        CONSTRAINT [fk_disputes_resolved_by_admin_id] FOREIGN KEY ([resolved_by_admin_id]) REFERENCES [users] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [payments] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [booking_id] uniqueidentifier NOT NULL,
        [provider] nvarchar(50) NOT NULL DEFAULT N'PayOS',
        [order_code] nvarchar(100) NOT NULL,
        [amount] decimal(12,2) NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [checkout_url] nvarchar(max) NULL,
        [provider_transaction_id] nvarchar(255) NULL,
        [paid_at] datetime2 NULL,
        [expired_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__payments__3213E83FEDB87C2A] PRIMARY KEY ([id]),
        CONSTRAINT [fk_payments_booking_id] FOREIGN KEY ([booking_id]) REFERENCES [bookings] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [reviews] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [booking_id] uniqueidentifier NOT NULL,
        [customer_id] uniqueidentifier NOT NULL,
        [pt_profile_id] uniqueidentifier NOT NULL,
        [rating] int NOT NULL,
        [comment] nvarchar(max) NULL,
        [is_hidden] bit NOT NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__reviews__3213E83FC246B2B0] PRIMARY KEY ([id]),
        CONSTRAINT [fk_reviews_booking_id] FOREIGN KEY ([booking_id]) REFERENCES [bookings] ([id]),
        CONSTRAINT [fk_reviews_customer_id] FOREIGN KEY ([customer_id]) REFERENCES [customer_profiles] ([id]),
        CONSTRAINT [fk_reviews_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [settlements] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [booking_id] uniqueidentifier NOT NULL,
        [pt_profile_id] uniqueidentifier NOT NULL,
        [gross_amount] decimal(12,2) NOT NULL,
        [platform_fee] decimal(12,2) NOT NULL,
        [net_amount] decimal(12,2) NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [settled_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__settleme__3213E83FC82FEC5F] PRIMARY KEY ([id]),
        CONSTRAINT [fk_settlements_booking_id] FOREIGN KEY ([booking_id]) REFERENCES [bookings] ([id]),
        CONSTRAINT [fk_settlements_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [workspaces] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [booking_id] uniqueidentifier NOT NULL,
        [customer_id] uniqueidentifier NOT NULL,
        [pt_profile_id] uniqueidentifier NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [course_note] nvarchar(max) NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__workspac__3213E83FDD6DCD60] PRIMARY KEY ([id]),
        CONSTRAINT [fk_workspaces_booking_id] FOREIGN KEY ([booking_id]) REFERENCES [bookings] ([id]),
        CONSTRAINT [fk_workspaces_customer_id] FOREIGN KEY ([customer_id]) REFERENCES [customer_profiles] ([id]),
        CONSTRAINT [fk_workspaces_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [dispute_evidences] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [dispute_id] uniqueidentifier NOT NULL,
        [file_url] nvarchar(max) NOT NULL,
        [file_type] nvarchar(50) NULL,
        [uploaded_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__dispute___3213E83F1F7FC76C] PRIMARY KEY ([id]),
        CONSTRAINT [fk_dispute_evidences_dispute_id] FOREIGN KEY ([dispute_id]) REFERENCES [disputes] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [payment_webhook_logs] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [payment_id] uniqueidentifier NULL,
        [provider] nvarchar(50) NOT NULL DEFAULT N'PayOS',
        [event_type] nvarchar(100) NULL,
        [event_id] nvarchar(255) NULL,
        [raw_payload] nvarchar(max) NOT NULL,
        [is_valid_signature] bit NOT NULL,
        [processed_at] datetime2 NULL,
        [received_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__payment___3213E83F77B7BFB3] PRIMARY KEY ([id]),
        CONSTRAINT [fk_payment_webhook_logs_payment_id] FOREIGN KEY ([payment_id]) REFERENCES [payments] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE TABLE [meal_plans] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [workspace_id] uniqueidentifier NOT NULL,
        [created_by_pt_id] uniqueidentifier NOT NULL,
        [title] nvarchar(150) NOT NULL,
        [content_json] nvarchar(max) NOT NULL,
        [source] int NOT NULL,
        [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
        [is_deleted] bit NOT NULL,
        [deleted_at] datetime2 NULL,
        [deleted_by] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [PK__meal_pla__3213E83FE8434E9A] PRIMARY KEY ([id]),
        CONSTRAINT [fk_meal_plans_created_by_pt_id] FOREIGN KEY ([created_by_pt_id]) REFERENCES [pt_profiles] ([id]),
        CONSTRAINT [fk_meal_plans_deleted_by] FOREIGN KEY ([deleted_by]) REFERENCES [users] ([id]),
        CONSTRAINT [fk_meal_plans_workspace_id] FOREIGN KEY ([workspace_id]) REFERENCES [workspaces] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_ai_usage_logs_created_at] ON [ai_usage_logs] ([created_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_ai_usage_logs_pt_profile_id] ON [ai_usage_logs] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_audit_logs_actor_user_id] ON [audit_logs] ([actor_user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_audit_logs_created_at] ON [audit_logs] ([created_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_audit_logs_entity] ON [audit_logs] ([entity_name], [entity_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_bookings_created_at] ON [bookings] ([created_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_bookings_customer_id] ON [bookings] ([customer_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_bookings_package_id] ON [bookings] ([package_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_bookings_pt_profile_id] ON [bookings] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_bookings_status] ON [bookings] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_conversations_customer_id] ON [conversations] ([customer_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_conversations_pt_profile_id] ON [conversations] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__conversa__5DE3A5B065808E54] ON [conversations] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__conversa__C704B367F57EC45D] ON [conversations] ([firebase_conversation_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_conversations_booking_id] ON [conversations] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_conversations_firebase_id] ON [conversations] ([firebase_conversation_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_customer_profiles_deleted_by] ON [customer_profiles] ([deleted_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_customer_profiles_is_deleted] ON [customer_profiles] ([is_deleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__customer__B9BE370E5DAC7E13] ON [customer_profiles] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_customer_profiles_user_id] ON [customer_profiles] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_dispute_evidences_dispute_id] ON [dispute_evidences] ([dispute_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_disputes_booking_id] ON [disputes] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_disputes_created_at] ON [disputes] ([created_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_disputes_customer_id] ON [disputes] ([customer_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_disputes_pt_profile_id] ON [disputes] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_disputes_resolved_by_admin_id] ON [disputes] ([resolved_by_admin_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_disputes_status] ON [disputes] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [ux_disputes_one_open_per_booking] ON [disputes] ([booking_id]) WHERE ([status] IN ((1), (2), (3)))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_meal_plans_created_by_pt_id] ON [meal_plans] ([created_by_pt_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_meal_plans_deleted_by] ON [meal_plans] ([deleted_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_meal_plans_is_active] ON [meal_plans] ([is_active]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_meal_plans_is_deleted] ON [meal_plans] ([is_deleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_meal_plans_workspace_id] ON [meal_plans] ([workspace_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_notifications_created_at] ON [notifications] ([created_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_notifications_deleted_by] ON [notifications] ([deleted_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_notifications_is_deleted] ON [notifications] ([is_deleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_notifications_is_read] ON [notifications] ([is_read]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_notifications_user_id] ON [notifications] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_packages_deleted_by] ON [packages] ([deleted_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_packages_is_active] ON [packages] ([is_active]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_packages_is_deleted] ON [packages] ([is_deleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_packages_price] ON [packages] ([price]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_packages_pt_profile_id] ON [packages] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_payment_webhook_logs_event_id] ON [payment_webhook_logs] ([event_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_payment_webhook_logs_payment_id] ON [payment_webhook_logs] ([payment_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_payment_webhook_logs_received_at] ON [payment_webhook_logs] ([received_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_payments_booking_id] ON [payments] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_payments_status] ON [payments] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__payments__99D12D3FD8197709] ON [payments] ([order_code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [ux_payments_one_success_per_booking] ON [payments] ([booking_id]) WHERE ([status]=(2))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_payments_order_code] ON [payments] ([order_code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_pt_documents_document_type] ON [pt_documents] ([document_type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_pt_documents_pt_profile_id] ON [pt_documents] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_pt_documents_status] ON [pt_documents] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_pt_profiles_average_rating] ON [pt_profiles] ([average_rating]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_pt_profiles_deleted_by] ON [pt_profiles] ([deleted_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_pt_profiles_is_deleted] ON [pt_profiles] ([is_deleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_pt_profiles_verification_status] ON [pt_profiles] ([verification_status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__pt_profi__B9BE370E276F861C] ON [pt_profiles] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_pt_profiles_user_id] ON [pt_profiles] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_refresh_tokens_expires_at] ON [refresh_tokens] ([expires_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_refresh_tokens_user_id] ON [refresh_tokens] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_reviews_customer_id] ON [reviews] ([customer_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_reviews_is_hidden] ON [reviews] ([is_hidden]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_reviews_pt_profile_id] ON [reviews] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__reviews__5DE3A5B06B553922] ON [reviews] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_reviews_booking_id] ON [reviews] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_settlements_pt_profile_id] ON [settlements] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_settlements_status] ON [settlements] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__settleme__5DE3A5B0FE34E5F5] ON [settlements] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_settlements_booking_id] ON [settlements] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [IX_users_deleted_by] ON [users] ([deleted_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_users_email] ON [users] ([email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_users_is_deleted] ON [users] ([is_deleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_users_role] ON [users] ([role]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_users_status] ON [users] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__users__AB6E6164EEF4CB86] ON [users] ([email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_workspaces_customer_id] ON [workspaces] ([customer_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_workspaces_pt_profile_id] ON [workspaces] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE INDEX [ix_workspaces_status] ON [workspaces] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__workspac__5DE3A5B0181B9644] ON [workspaces] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    CREATE UNIQUE INDEX [ux_workspaces_booking_id] ON [workspaces] ([booking_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604140618_InitialAzureDb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260604140618_InitialAzureDb', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260702182100_AddWorkspaceSessions'
)
BEGIN
    CREATE TABLE [workspace_sessions] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [workspace_id] uniqueidentifier NOT NULL,
        [session_number] int NOT NULL,
        [description] nvarchar(max) NULL,
        [completed_at] datetime2 NOT NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [PK__workspace_sessions__3213E83FD1C72652] PRIMARY KEY ([id]),
        CONSTRAINT [fk_workspace_sessions_workspace_id] FOREIGN KEY ([workspace_id]) REFERENCES [workspaces] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260702182100_AddWorkspaceSessions'
)
BEGIN
    CREATE UNIQUE INDEX [uq_workspace_sessions_workspace_id_session_number] ON [workspace_sessions] ([workspace_id], [session_number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260702182100_AddWorkspaceSessions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260702182100_AddWorkspaceSessions', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703152631_AddConversationMetadata'
)
BEGIN
    ALTER TABLE [conversations] ADD [last_activity_at] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703152631_AddConversationMetadata'
)
BEGIN
    ALTER TABLE [conversations] ADD [last_message_preview] nvarchar(255) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703152631_AddConversationMetadata'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703152631_AddConversationMetadata', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704091334_AddPtProfileEditRequests'
)
BEGIN
    CREATE TABLE [pt_profile_edit_requests] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [current_bio] nvarchar(max) NULL,
        [current_specialization] nvarchar(max) NULL,
        [current_experience_years] int NOT NULL,
        [requested_bio] nvarchar(max) NULL,
        [requested_specialization] nvarchar(max) NULL,
        [requested_experience_years] int NOT NULL,
        [status] int NOT NULL DEFAULT 1,
        [rejection_reason] nvarchar(max) NULL,
        [requested_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [reviewed_at] datetime2 NULL,
        [reviewed_by_admin_id] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [pk_pt_profile_edit_requests] PRIMARY KEY ([id]),
        CONSTRAINT [fk_pt_profile_edit_requests_admin_id] FOREIGN KEY ([reviewed_by_admin_id]) REFERENCES [users] ([id]),
        CONSTRAINT [fk_pt_profile_edit_requests_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704091334_AddPtProfileEditRequests'
)
BEGIN
    CREATE INDEX [ix_pt_profile_edit_requests_pt_profile_id] ON [pt_profile_edit_requests] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704091334_AddPtProfileEditRequests'
)
BEGIN
    CREATE INDEX [IX_pt_profile_edit_requests_reviewed_by_admin_id] ON [pt_profile_edit_requests] ([reviewed_by_admin_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704091334_AddPtProfileEditRequests'
)
BEGIN
    CREATE INDEX [ix_pt_profile_edit_requests_status] ON [pt_profile_edit_requests] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704091334_AddPtProfileEditRequests'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [ux_pt_profile_edit_requests_one_pending] ON [pt_profile_edit_requests] ([pt_profile_id]) WHERE ([status]=(1))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704091334_AddPtProfileEditRequests'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704091334_AddPtProfileEditRequests', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706115458_AddDisputeHardening'
)
BEGIN
    ALTER TABLE [disputes] ADD [original_booking_status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706115458_AddDisputeHardening'
)
BEGIN
    ALTER TABLE [disputes] ADD [original_settlement_status] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706115458_AddDisputeHardening'
)
BEGIN
    ALTER TABLE [disputes] ADD [withdrawn_at] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706115458_AddDisputeHardening'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [ux_disputes_one_active_dispute_per_booking] ON [disputes] ([booking_id]) WHERE ([status] IN ((1), (2)))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706115458_AddDisputeHardening'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706115458_AddDisputeHardening', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_orders] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [status] int NOT NULL,
        [total_amount] decimal(12,2) NOT NULL,
        [currency] nvarchar(10) NOT NULL,
        [paid_at] datetime2 NULL,
        [cancelled_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [pk_addon_orders] PRIMARY KEY ([id]),
        CONSTRAINT [fk_addon_orders_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_products] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [code] nvarchar(50) NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [description] nvarchar(max) NULL,
        [product_type] int NOT NULL,
        [grant_quantity] int NULL,
        [is_active] bit NOT NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [pk_addon_products] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [meal_plan_quota_counters] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [quota_date] date NOT NULL,
        [consumed_count] int NOT NULL DEFAULT 0,
        [reserved_count] int NOT NULL DEFAULT 0,
        [daily_limit] int NOT NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [pk_meal_plan_quota_counters] PRIMARY KEY ([id]),
        CONSTRAINT [ck_meal_plan_quota_counters_counts] CHECK (consumed_count >= 0 AND reserved_count >= 0 AND daily_limit > 0 AND consumed_count + reserved_count <= daily_limit),
        CONSTRAINT [fk_meal_plan_quota_counters_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_payment_attempts] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [order_id] uniqueidentifier NOT NULL,
        [provider] nvarchar(50) NOT NULL,
        [order_code] nvarchar(100) NOT NULL,
        [amount] decimal(12,2) NOT NULL,
        [currency] nvarchar(10) NOT NULL,
        [status] int NOT NULL,
        [checkout_url] nvarchar(max) NULL,
        [provider_transaction_id] nvarchar(255) NULL,
        [paid_at] datetime2 NULL,
        [expired_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [pk_addon_payment_attempts] PRIMARY KEY ([id]),
        CONSTRAINT [fk_addon_payment_attempts_order_id] FOREIGN KEY ([order_id]) REFERENCES [addon_orders] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_product_prices] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [product_id] uniqueidentifier NOT NULL,
        [unit_amount] decimal(12,2) NOT NULL,
        [currency] nvarchar(10) NOT NULL,
        [is_active] bit NOT NULL,
        [created_by_admin_id] uniqueidentifier NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [deactivated_at] datetime2 NULL,
        CONSTRAINT [pk_addon_product_prices] PRIMARY KEY ([id]),
        CONSTRAINT [fk_addon_product_prices_created_by_admin_id] FOREIGN KEY ([created_by_admin_id]) REFERENCES [users] ([id]),
        CONSTRAINT [fk_addon_product_prices_product_id] FOREIGN KEY ([product_id]) REFERENCES [addon_products] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_webhook_logs] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [attempt_id] uniqueidentifier NULL,
        [provider] nvarchar(50) NOT NULL,
        [event_type] nvarchar(100) NULL,
        [event_id] nvarchar(255) NULL,
        [raw_payload] nvarchar(max) NOT NULL,
        [is_valid_signature] bit NOT NULL,
        [processed_at] datetime2 NULL,
        [received_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [pk_addon_webhook_logs] PRIMARY KEY ([id]),
        CONSTRAINT [fk_addon_webhook_logs_attempt_id] FOREIGN KEY ([attempt_id]) REFERENCES [addon_payment_attempts] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_order_items] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [order_id] uniqueidentifier NOT NULL,
        [product_id] uniqueidentifier NOT NULL,
        [price_id] uniqueidentifier NOT NULL,
        [product_code] nvarchar(50) NOT NULL,
        [product_name] nvarchar(200) NOT NULL,
        [unit_amount] decimal(12,2) NOT NULL,
        [currency] nvarchar(10) NOT NULL,
        [quantity] int NOT NULL,
        [total_amount] decimal(12,2) NOT NULL,
        [fulfillment_type_snapshot] nvarchar(50) NOT NULL,
        [grant_quantity_snapshot] int NULL,
        [duration_days_snapshot] int NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        CONSTRAINT [pk_addon_order_items] PRIMARY KEY ([id]),
        CONSTRAINT [fk_addon_order_items_order_id] FOREIGN KEY ([order_id]) REFERENCES [addon_orders] ([id]) ON DELETE CASCADE,
        CONSTRAINT [fk_addon_order_items_price_id] FOREIGN KEY ([price_id]) REFERENCES [addon_product_prices] ([id]),
        CONSTRAINT [fk_addon_order_items_product_id] FOREIGN KEY ([product_id]) REFERENCES [addon_products] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_entitlements] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [order_id] uniqueidentifier NULL,
        [order_item_id] uniqueidentifier NULL,
        [product_code] nvarchar(50) NOT NULL,
        [fulfillment_type] nvarchar(50) NOT NULL,
        [quantity_granted] int NOT NULL,
        [quantity_remaining] int NOT NULL,
        [status] int NOT NULL,
        [activated_at] datetime2 NULL,
        [expires_at] datetime2 NULL,
        [created_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [updated_at] datetime2 NULL,
        CONSTRAINT [pk_addon_entitlements] PRIMARY KEY ([id]),
        CONSTRAINT [ck_addon_entitlements_quantity] CHECK (quantity_remaining >= 0),
        CONSTRAINT [fk_addon_entitlements_order_id] FOREIGN KEY ([order_id]) REFERENCES [addon_orders] ([id]),
        CONSTRAINT [fk_addon_entitlements_order_item_id] FOREIGN KEY ([order_item_id]) REFERENCES [addon_order_items] ([id]),
        CONSTRAINT [fk_addon_entitlements_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE TABLE [addon_quota_reservations] (
        [id] uniqueidentifier NOT NULL DEFAULT ((newid())),
        [pt_profile_id] uniqueidentifier NOT NULL,
        [reservation_type] nvarchar(50) NOT NULL,
        [entitlement_id] uniqueidentifier NULL,
        [quota_date] date NULL,
        [generation_request_id] uniqueidentifier NOT NULL,
        [status] int NOT NULL,
        [reserved_at] datetime2 NOT NULL DEFAULT ((sysutcdatetime())),
        [expires_at] datetime2 NOT NULL,
        [finalized_at] datetime2 NULL,
        [released_at] datetime2 NULL,
        CONSTRAINT [pk_addon_quota_reservations] PRIMARY KEY ([id]),
        CONSTRAINT [fk_addon_quota_reservations_entitlement_id] FOREIGN KEY ([entitlement_id]) REFERENCES [addon_entitlements] ([id]),
        CONSTRAINT [fk_addon_quota_reservations_pt_profile_id] FOREIGN KEY ([pt_profile_id]) REFERENCES [pt_profiles] ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_entitlements_consumption] ON [addon_entitlements] ([pt_profile_id], [product_code], [status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [IX_addon_entitlements_order_id] ON [addon_entitlements] ([order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [IX_addon_entitlements_order_item_id] ON [addon_entitlements] ([order_item_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_order_items_order_id] ON [addon_order_items] ([order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [IX_addon_order_items_price_id] ON [addon_order_items] ([price_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_order_items_product_id] ON [addon_order_items] ([product_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_orders_pt_profile_id] ON [addon_orders] ([pt_profile_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_orders_status] ON [addon_orders] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_payment_attempts_order_id] ON [addon_payment_attempts] ([order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_payment_attempts_status] ON [addon_payment_attempts] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [ux_addon_payment_attempts_one_success_per_order] ON [addon_payment_attempts] ([order_id]) WHERE ([status]=(2))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [ux_addon_payment_attempts_order_code] ON [addon_payment_attempts] ([order_code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [IX_addon_product_prices_created_by_admin_id] ON [addon_product_prices] ([created_by_admin_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_product_prices_product_id] ON [addon_product_prices] ([product_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [ux_addon_product_prices_one_active_per_product] ON [addon_product_prices] ([product_id]) WHERE ([is_active]=(1))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_products_is_active] ON [addon_products] ([is_active]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [ux_addon_products_code] ON [addon_products] ([code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_quota_reservations_cleanup] ON [addon_quota_reservations] ([pt_profile_id], [status], [expires_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [IX_addon_quota_reservations_entitlement_id] ON [addon_quota_reservations] ([entitlement_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_quota_reservations_generation_request_id] ON [addon_quota_reservations] ([generation_request_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_webhook_logs_attempt_id] ON [addon_webhook_logs] ([attempt_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_webhook_logs_event_id] ON [addon_webhook_logs] ([event_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE INDEX [ix_addon_webhook_logs_received_at] ON [addon_webhook_logs] ([received_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [ux_meal_plan_quota_counters_pt_date] ON [meal_plan_quota_counters] ([pt_profile_id], [quota_date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706180651_AddAddonPaymentFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706180651_AddAddonPaymentFoundation', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707133800_AddAddonProductDurationDays'
)
BEGIN
    ALTER TABLE [addon_products] ADD [duration_days] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707133800_AddAddonProductDurationDays'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707133800_AddAddonProductDurationDays', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707171245_MakeGenerationRequestIdUnique'
)
BEGIN
    DROP INDEX [ix_addon_quota_reservations_generation_request_id] ON [addon_quota_reservations];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707171245_MakeGenerationRequestIdUnique'
)
BEGIN
    CREATE UNIQUE INDEX [ix_addon_quota_reservations_generation_request_id] ON [addon_quota_reservations] ([generation_request_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707171245_MakeGenerationRequestIdUnique'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707171245_MakeGenerationRequestIdUnique', N'8.0.8');
END;
GO

COMMIT;
GO

