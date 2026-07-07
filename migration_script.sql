BEGIN TRANSACTION;
GO

ALTER TABLE [addon_products] ADD [duration_days] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260707133800_AddAddonProductDurationDays', N'8.0.8');
GO

COMMIT;
GO

