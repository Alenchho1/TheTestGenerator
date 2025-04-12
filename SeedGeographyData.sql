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

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastModifiedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Tests] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [TimeLimit] int NOT NULL,
    [DifficultyLevel] int NOT NULL,
    [TotalPoints] int NOT NULL,
    [CreatorId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_Tests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tests_AspNetUsers_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Questions] (
    [Id] int NOT NULL IDENTITY,
    [Content] nvarchar(1000) NOT NULL,
    [Type] int NOT NULL,
    [DifficultyLevel] int NOT NULL,
    [CorrectAnswer] nvarchar(max) NULL,
    [Keywords] nvarchar(max) NULL,
    [ImagePath] nvarchar(max) NULL,
    [Points] int NOT NULL,
    [CategoryId] int NOT NULL,
    [CreatorId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastModifiedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Questions_AspNetUsers_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Questions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [TestResults] (
    [Id] int NOT NULL IDENTITY,
    [TestId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    [Score] int NOT NULL,
    [MaxScore] int NOT NULL,
    [PercentageScore] decimal(5,2) NOT NULL,
    [IsCompleted] bit NOT NULL,
    [CorrectAnswersCount] int NOT NULL,
    [TotalQuestionsCount] int NOT NULL,
    [Grade] decimal(3,2) NOT NULL,
    [Feedback] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TestResults] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TestResults_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TestResults_Tests_TestId] FOREIGN KEY ([TestId]) REFERENCES [Tests] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Answers] (
    [Id] int NOT NULL IDENTITY,
    [Content] nvarchar(500) NOT NULL,
    [IsCorrect] bit NOT NULL,
    [QuestionId] int NOT NULL,
    [OrderNumber] int NOT NULL,
    CONSTRAINT [PK_Answers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Answers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [TestQuestions] (
    [TestId] int NOT NULL,
    [QuestionId] int NOT NULL,
    [OrderNumber] int NOT NULL,
    CONSTRAINT [PK_TestQuestions] PRIMARY KEY ([TestId], [QuestionId]),
    CONSTRAINT [FK_TestQuestions_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TestQuestions_Tests_TestId] FOREIGN KEY ([TestId]) REFERENCES [Tests] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [TestAnswerResults] (
    [Id] int NOT NULL IDENTITY,
    [TestResultId] int NOT NULL,
    [QuestionId] int NOT NULL,
    [SubmittedAnswer] nvarchar(max) NOT NULL,
    [SelectedAnswerId] int NULL,
    [IsCorrect] bit NOT NULL,
    [Points] int NOT NULL,
    [KeywordMatchPercentage] decimal(5,2) NOT NULL,
    [FeedbackNotes] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TestAnswerResults] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TestAnswerResults_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TestAnswerResults_TestResults_TestResultId] FOREIGN KEY ([TestResultId]) REFERENCES [TestResults] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Answers_QuestionId] ON [Answers] ([QuestionId]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Questions_CategoryId] ON [Questions] ([CategoryId]);
GO

CREATE INDEX [IX_Questions_CreatorId] ON [Questions] ([CreatorId]);
GO

CREATE INDEX [IX_TestAnswerResults_QuestionId] ON [TestAnswerResults] ([QuestionId]);
GO

CREATE INDEX [IX_TestAnswerResults_TestResultId] ON [TestAnswerResults] ([TestResultId]);
GO

CREATE INDEX [IX_TestQuestions_QuestionId] ON [TestQuestions] ([QuestionId]);
GO

CREATE INDEX [IX_TestResults_TestId] ON [TestResults] ([TestId]);
GO

CREATE INDEX [IX_TestResults_UserId] ON [TestResults] ([UserId]);
GO

CREATE INDEX [IX_Tests_CreatorId] ON [Tests] ([CreatorId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250223124622_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'CreatedAt');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Categories] DROP COLUMN [CreatedAt];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'LastModifiedAt');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Categories] DROP COLUMN [LastModifiedAt];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Questions]') AND [c].[name] = N'LastModifiedAt');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Questions] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Questions] ALTER COLUMN [LastModifiedAt] datetime2 NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250223124726_AddAuditFields', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Answers]') AND [c].[name] = N'OrderNumber');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Answers] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Answers] DROP COLUMN [OrderNumber];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tests]') AND [c].[name] = N'Title');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Tests] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Tests] ALTER COLUMN [Title] nvarchar(max) NOT NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tests]') AND [c].[name] = N'Description');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Tests] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Tests] ALTER COLUMN [Description] nvarchar(max) NULL;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Questions]') AND [c].[name] = N'Content');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Questions] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Questions] ALTER COLUMN [Content] nvarchar(max) NOT NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Answers]') AND [c].[name] = N'Content');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Answers] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Answers] ALTER COLUMN [Content] nvarchar(max) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250223125943_AddCreatedAtToQuestion', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [TestResults] DROP CONSTRAINT [FK_TestResults_Tests_TestId];
GO

ALTER TABLE [TestResults] ADD CONSTRAINT [FK_TestResults_Tests_TestId] FOREIGN KEY ([TestId]) REFERENCES [Tests] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250223131708_FixTestResultRelationship', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestResults]') AND [c].[name] = N'CorrectAnswersCount');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [TestResults] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [TestResults] DROP COLUMN [CorrectAnswersCount];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestResults]') AND [c].[name] = N'TotalQuestionsCount');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [TestResults] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [TestResults] DROP COLUMN [TotalQuestionsCount];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestAnswerResults]') AND [c].[name] = N'KeywordMatchPercentage');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [TestAnswerResults] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [TestAnswerResults] DROP COLUMN [KeywordMatchPercentage];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestAnswerResults]') AND [c].[name] = N'SelectedAnswerId');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [TestAnswerResults] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [TestAnswerResults] DROP COLUMN [SelectedAnswerId];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tests]') AND [c].[name] = N'Title');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Tests] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Tests] ALTER COLUMN [Title] nvarchar(100) NOT NULL;
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tests]') AND [c].[name] = N'Description');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Tests] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Tests] ALTER COLUMN [Description] nvarchar(500) NULL;
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestResults]') AND [c].[name] = N'Score');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [TestResults] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [TestResults] ALTER COLUMN [Score] decimal(5,2) NOT NULL;
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestAnswerResults]') AND [c].[name] = N'SubmittedAnswer');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [TestAnswerResults] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [TestAnswerResults] ALTER COLUMN [SubmittedAnswer] nvarchar(1000) NOT NULL;
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'Name');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [Categories] ALTER COLUMN [Name] nvarchar(50) NOT NULL;
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'Description');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [Categories] ALTER COLUMN [Description] nvarchar(200) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250305141547_UpdateScoreToDecimal', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [TestAnswerResults] ADD [KeywordMatchPercentage] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250408121045_AddKeywordMatchPercentage', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestAnswerResults]') AND [c].[name] = N'KeywordMatchPercentage');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [TestAnswerResults] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [TestAnswerResults] ALTER COLUMN [KeywordMatchPercentage] decimal(5,2) NOT NULL;
GO

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Questions]') AND [c].[name] = N'Content');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Questions] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [Questions] ALTER COLUMN [Content] nvarchar(1000) NOT NULL;
GO

UPDATE [Categories] SET [Description] = N'Въпроси за географията на България и света'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250408131048_RenameModifiedAtColumn', N'8.0.11');
GO

COMMIT;
GO

