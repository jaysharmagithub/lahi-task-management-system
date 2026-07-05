/*
Lahi Task Management System - Database Schema Script
Target: SQL Server 2022+
*/

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

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    [Department] nvarchar(100) NULL,
    [Designation] nvarchar(100) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Tasks] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [Priority] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [DueDate] datetime2 NOT NULL,
    [AttachmentPath] nvarchar(500) NULL,
    [AttachmentFileName] nvarchar(255) NULL,
    [IsDueSoonNotificationSent] bit NOT NULL,
    [AssignedToId] uniqueidentifier NOT NULL,
    [CreatedById] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Tasks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tasks_Users_AssignedToId] FOREIGN KEY ([AssignedToId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Tasks_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [Token] nvarchar(450) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [RevokedReason] nvarchar(max) NULL,
    [ReplacedByToken] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Notifications] (
    [Id] uniqueidentifier NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TaskId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Tasks_TaskId] FOREIGN KEY ([TaskId]) REFERENCES [Tasks] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO
CREATE INDEX [IX_Tasks_AssignedToId] ON [Tasks] ([AssignedToId]);
GO
CREATE INDEX [IX_Tasks_CreatedById] ON [Tasks] ([CreatedById]);
GO
CREATE INDEX [IX_Tasks_DueDate] ON [Tasks] ([DueDate]);
GO
CREATE INDEX [IX_Tasks_Status] ON [Tasks] ([Status]);
GO
CREATE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
GO
CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
GO
CREATE INDEX [IX_Notifications_TaskId] ON [Notifications] ([TaskId]);
GO
CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

COMMIT;
GO
