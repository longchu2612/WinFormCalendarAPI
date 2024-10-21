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

CREATE TABLE [Schedules] (
    [Id] int NOT NULL IDENTITY,
    [date] datetime2 NOT NULL,
    [fromX] int NOT NULL,
    [fromY] int NOT NULL,
    [toX] int NOT NULL,
    [toY] int NOT NULL,
    CONSTRAINT [PK_Schedules] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241011102422_addNewSchedule', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241011102715_AddNewDatabase', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Schedules] ADD [reason] nvarchar(max) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241011103606_addNewMigartionSchedule', N'8.0.10');
GO

COMMIT;
GO

