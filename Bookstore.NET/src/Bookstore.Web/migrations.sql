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
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE TABLE [Books] (
        [BookId] nvarchar(50) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [ISBN] nvarchar(20) NULL,
        [Price] decimal(12,2) NOT NULL,
        [Author] nvarchar(128) NOT NULL,
        [Type] nvarchar(128) NULL,
        [ImagePath] nvarchar(256) NULL,
        CONSTRAINT [PK_Books] PRIMARY KEY ([BookId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [AzureAdObjectId] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [UserName] nvarchar(128) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [LastLoginAt] datetimeoffset NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [CustomerId] int NOT NULL IDENTITY,
        [Name] nvarchar(128) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [IdNumber] nvarchar(20) NULL,
        [Email] nvarchar(256) NOT NULL,
        [Address] nvarchar(500) NULL,
        [Gender] nvarchar(10) NULL,
        [UserId] int NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId]),
        CONSTRAINT [FK_Customers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE TABLE [Carts] (
        [CartId] int NOT NULL IDENTITY,
        [CustomerId] int NULL,
        [BookId] nvarchar(50) NULL,
        [Price] decimal(12,2) NOT NULL,
        [Quantity] int NOT NULL,
        [TotalPrice] decimal(12,2) NOT NULL,
        CONSTRAINT [PK_Carts] PRIMARY KEY ([CartId]),
        CONSTRAINT [FK_Carts_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE SET NULL,
        CONSTRAINT [FK_Carts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [OrderId] int NOT NULL IDENTITY,
        [CustomerId] int NULL,
        [BookId] nvarchar(50) NULL,
        [PurchaseDate] datetimeoffset NOT NULL,
        [Quantity] int NOT NULL,
        [TotalPrice] decimal(12,2) NOT NULL,
        [Status] nvarchar(1) NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
        CONSTRAINT [FK_Orders_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([BookId]) ON DELETE SET NULL,
        CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BookId', N'Author', N'ISBN', N'ImagePath', N'Price', N'Title', N'Type') AND [object_id] = OBJECT_ID(N'[Books]'))
        SET IDENTITY_INSERT [Books] ON;
    EXEC(N'INSERT INTO [Books] ([BookId], [Author], [ISBN], [ImagePath], [Price], [Title], [Type])
    VALUES (N''B-001'', N''Lonely Planet'', N''123-456-789-1'', N''images/travel.jpg'', 136.0, N''Lonely Planet Australia (Travel Guide)'', N''Travel''),
    (N''B-002'', N''Barbara Kanki'', N''123-456-789-2'', N''images/technical.jpg'', 599.0, N''Crew Resource Management, Second Edition'', N''Technical''),
    (N''B-003'', N''Cisco Press'', N''123-456-789-3'', N''images/technology.jpg'', 329.0, N''CCNA Routing and Switching 200-125 Official Cert Guide Library'', N''Technology''),
    (N''B-004'', N''Rockridge Press'', N''123-456-789-4'', N''images/food.jpg'', 75.9, N''Easy Vegetarian Slow Cooker Cookbook'', N''Food'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BookId', N'Author', N'ISBN', N'ImagePath', N'Price', N'Title', N'Type') AND [object_id] = OBJECT_ID(N'[Books]'))
        SET IDENTITY_INSERT [Books] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Carts_BookId] ON [Carts] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Carts_CustomerId] ON [Carts] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_UserId] ON [Customers] ([UserId]) WHERE [UserId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_BookId] ON [Orders] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_AzureAdObjectId] ON [Users] ([AzureAdObjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122194353_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260122194353_InitialCreate', N'10.0.1');
END;

COMMIT;
GO

