-- RaceDay SQL Server schema reconstructed for Part 2, 17 October 2026.
-- Generated from the included EF Core Code First migration.
-- Run in SSMS. Demo accounts use RaceDay!Demo2026, stored only as salted hashes.
IF DB_ID(N'RaceDay') IS NULL
BEGIN
    CREATE DATABASE [RaceDay];
END;
GO
USE [RaceDay];
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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [PhoneNumber] nvarchar(30) NOT NULL,
        [ProfilePictureUrl] nvarchar(2048) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
        CONSTRAINT [CK_Users_Role] CHECK ([Role] IN ('Organiser', 'Participant'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [LogId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Action] nvarchar(500) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([LogId]),
        CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE TABLE [Events] (
        [EventId] int NOT NULL IDENTITY,
        [OrganiserId] int NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [EventDate] datetime2 NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [DistanceKm] decimal(8,2) NOT NULL,
        [EventType] nvarchar(10) NOT NULL,
        [BannerImageUrl] nvarchar(2048) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([EventId]),
        CONSTRAINT [CK_Events_Distance] CHECK ([DistanceKm] > 0),
        CONSTRAINT [CK_Events_Type] CHECK ([EventType] IN ('Run', 'Walk', 'Cycle')),
        CONSTRAINT [FK_Events_Users_OrganiserId] FOREIGN KEY ([OrganiserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [CategoryId] int NOT NULL IDENTITY,
        [EventId] int NOT NULL,
        [CategoryName] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [DistanceKm] decimal(8,2) NOT NULL,
        [Fee] decimal(10,2) NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId]),
        CONSTRAINT [AK_Categories_CategoryId_EventId] UNIQUE ([CategoryId], [EventId]),
        CONSTRAINT [CK_Categories_Distance] CHECK ([DistanceKm] > 0),
        CONSTRAINT [CK_Categories_Fee] CHECK ([Fee] >= 0),
        CONSTRAINT [FK_Categories_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE TABLE [Enrolments] (
        [EnrolmentId] int NOT NULL IDENTITY,
        [ParticipantId] int NOT NULL,
        [EventId] int NOT NULL,
        [CategoryId] int NOT NULL,
        [EnrolmentDate] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'Confirmed',
        CONSTRAINT [PK_Enrolments] PRIMARY KEY ([EnrolmentId]),
        CONSTRAINT [FK_Enrolments_Categories_CategoryId_EventId] FOREIGN KEY ([CategoryId], [EventId]) REFERENCES [Categories] ([CategoryId], [EventId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrolments_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([EventId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrolments_Users_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE TABLE [Results] (
        [ResultId] int NOT NULL IDENTITY,
        [EnrolmentId] int NOT NULL,
        [FinishTime] time NOT NULL,
        [OverallPosition] int NOT NULL,
        [CategoryPosition] int NOT NULL,
        [RecordedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Results] PRIMARY KEY ([ResultId]),
        CONSTRAINT [CK_Results_Positions] CHECK ([OverallPosition] > 0 AND [CategoryPosition] > 0 AND [CategoryPosition] <= [OverallPosition]),
        CONSTRAINT [CK_Results_Time] CHECK ([FinishTime] > '00:00:00'),
        CONSTRAINT [FK_Results_Enrolments_EnrolmentId] FOREIGN KEY ([EnrolmentId]) REFERENCES [Enrolments] ([EnrolmentId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_EventId_CategoryName] ON [Categories] ([EventId], [CategoryName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Enrolments_CategoryId_EventId] ON [Enrolments] ([CategoryId], [EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Enrolments_EventId] ON [Enrolments] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Enrolments_ParticipantId_EventId] ON [Enrolments] ([ParticipantId], [EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Events_OrganiserId] ON [Events] ([OrganiserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Results_EnrolmentId] ON [Results] ([EnrolmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261017115025_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261017115025_InitialCreate', N'10.0.13');
END;

COMMIT;
GO


-- Seed only an empty database. The whole sample dataset is one transaction.
SET XACT_ABORT ON;
IF NOT EXISTS (SELECT 1 FROM [Users])
BEGIN
    BEGIN TRANSACTION;
    DECLARE @organiser1 int, @organiser2 int, @participant1 int, @participant2 int;
    DECLARE @event1 int, @event2 int, @event3 int, @category1 int, @category2 int, @category3 int, @entry3 int;
    INSERT INTO [Users] ([FullName],[Email],[PasswordHash],[Role],[PhoneNumber],[CreatedAt])
    VALUES (N'Naledi Mokoena',N'organiser1@raceday.example',N'AQAAAAIAAYagAAAAEDnK8S+iqQ+ohleCqfMez5gf5lFtay00idhjgX+qnT1e82i2s1yuikU7ml7fXwiREA==',N'Organiser',N'0125550101',SYSUTCDATETIME());
    SET @organiser1 = SCOPE_IDENTITY();
    INSERT INTO [Users] ([FullName],[Email],[PasswordHash],[Role],[PhoneNumber],[CreatedAt])
    VALUES (N'Thabo Dlamini',N'organiser2@raceday.example',N'AQAAAAIAAYagAAAAEIo4iXbYDF1YVTvE3W4Q7XCTyZuIDyTualkvfsun7qvTavLhgvevJuY3D9CIzYloPA==',N'Organiser',N'0125550102',SYSUTCDATETIME());
    SET @organiser2 = SCOPE_IDENTITY();
    INSERT INTO [Users] ([FullName],[Email],[PasswordHash],[Role],[PhoneNumber],[CreatedAt])
    VALUES (N'Lerato Ndlovu',N'participant1@raceday.example',N'AQAAAAIAAYagAAAAEPtpOvcJCWLEQ059tJgEF0x6+0eqt5EtJCHjvqEuIGhLTTi12F8TMjmgjYjF6MhYeA==',N'Participant',N'0725550103',SYSUTCDATETIME());
    SET @participant1 = SCOPE_IDENTITY();
    INSERT INTO [Users] ([FullName],[Email],[PasswordHash],[Role],[PhoneNumber],[CreatedAt])
    VALUES (N'Sipho Nkosi',N'participant2@raceday.example',N'AQAAAAIAAYagAAAAEB/XaWmL2oNneeKeftZM2LK4gcEpLtzUiXFHCgNbR3bGYWtpghY2BIp4TJ4ciZnILA==',N'Participant',N'0725550104',SYSUTCDATETIME());
    SET @participant2 = SCOPE_IDENTITY();
    INSERT INTO [Events] ([OrganiserId],[Name],[Description],[EventDate],[Location],[DistanceKm],[EventType],[CreatedAt])
    VALUES (@organiser1,N'Pretoria Community Run',N'Community road run in Pretoria.',DATEADD(day,30,SYSUTCDATETIME()),N'Pretoria',10,N'Run',SYSUTCDATETIME());
    SET @event1 = SCOPE_IDENTITY();
    INSERT INTO [Events] ([OrganiserId],[Name],[Description],[EventDate],[Location],[DistanceKm],[EventType],[CreatedAt])
    VALUES (@organiser2,N'Lynnwood Charity Cycle',N'Charity cycling event.',DATEADD(day,45,SYSUTCDATETIME()),N'Lynnwood, Pretoria',40,N'Cycle',SYSUTCDATETIME());
    SET @event2 = SCOPE_IDENTITY();
    INSERT INTO [Events] ([OrganiserId],[Name],[Description],[EventDate],[Location],[DistanceKm],[EventType],[CreatedAt])
    VALUES (@organiser1,N'Tshwane Heritage Walk',N'Completed walk for result demonstrations.',DATEADD(day,-7,SYSUTCDATETIME()),N'Tshwane',5,N'Walk',SYSUTCDATETIME());
    SET @event3 = SCOPE_IDENTITY();
    INSERT INTO [Categories] ([EventId],[CategoryName],[Description],[DistanceKm],[Fee]) VALUES (@event1,N'Open',N'Open entry category',10,100);
    SET @category1 = SCOPE_IDENTITY();
    INSERT INTO [Categories] ([EventId],[CategoryName],[Description],[DistanceKm],[Fee]) VALUES (@event2,N'Open',N'Open entry category',40,100);
    SET @category2 = SCOPE_IDENTITY();
    INSERT INTO [Categories] ([EventId],[CategoryName],[Description],[DistanceKm],[Fee]) VALUES (@event3,N'Open',N'Open entry category',5,100);
    SET @category3 = SCOPE_IDENTITY();
    INSERT INTO [Enrolments] ([ParticipantId],[EventId],[CategoryId],[EnrolmentDate],[Status]) VALUES (@participant1,@event1,@category1,SYSUTCDATETIME(),N'Confirmed');
    INSERT INTO [Enrolments] ([ParticipantId],[EventId],[CategoryId],[EnrolmentDate],[Status]) VALUES (@participant2,@event2,@category2,SYSUTCDATETIME(),N'Confirmed');
    INSERT INTO [Enrolments] ([ParticipantId],[EventId],[CategoryId],[EnrolmentDate],[Status]) VALUES (@participant1,@event3,@category3,DATEADD(day,-8,SYSUTCDATETIME()),N'Confirmed');
    SET @entry3 = SCOPE_IDENTITY();
    INSERT INTO [Enrolments] ([ParticipantId],[EventId],[CategoryId],[EnrolmentDate],[Status]) VALUES (@participant2,@event3,@category3,DATEADD(day,-8,SYSUTCDATETIME()),N'Confirmed');
    INSERT INTO [Results] ([EnrolmentId],[FinishTime],[OverallPosition],[CategoryPosition],[RecordedAt]) VALUES (@entry3,'00:42:00',1,1,SYSUTCDATETIME());
    INSERT INTO [AuditLogs] ([UserId],[Action],[Timestamp]) VALUES (@organiser1,N'Demo data initialised.',SYSUTCDATETIME());
    COMMIT;
END;
GO
