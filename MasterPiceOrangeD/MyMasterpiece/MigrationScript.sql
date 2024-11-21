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

CREATE TABLE [Categories] (
    [CategoryId] int NOT NULL IDENTITY,
    [CategoryName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId])
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [PasswordSalt] nvarchar(max) NOT NULL,
    [IsAdmin] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [Address] nvarchar(max) NULL,
    [Gender] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [otp] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);
GO

CREATE TABLE [Blogs] (
    [BlogId] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [PublishedAt] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    [ViewCount] int NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [ApprovalStatus] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Blogs] PRIMARY KEY ([BlogId]),
    CONSTRAINT [FK_Blogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Contacts] (
    [ContactId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Subject] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [SubmittedAt] datetime2 NOT NULL,
    [UserId] int NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([ContactId]),
    CONSTRAINT [FK_Contacts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [Products] (
    [ProductId] int NOT NULL IDENTITY,
    [ProductName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [StartingPrice] decimal(18,2) NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [SellerId] int NOT NULL,
    [UnitsSold] int NOT NULL,
    [Stock] int NOT NULL,
    [Condition] nvarchar(max) NOT NULL,
    [Location] nvarchar(max) NOT NULL,
    [Country] nvarchar(max) NOT NULL,
    [Brand] nvarchar(max) NOT NULL,
    [Views] int NOT NULL,
    [Quantity] int NOT NULL,
    [CategoryId] int NOT NULL,
    [ApprovalStatus] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Auctions] (
    [AuctionId] int NOT NULL IDENTITY,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    [CurrentHighestBid] decimal(18,2) NOT NULL,
    [CurrentHighestBidderId] int NULL,
    [ProductId] int NOT NULL,
    [AuctionStatus] nvarchar(max) NOT NULL,
    [IsNotificationSent] bit NOT NULL,
    CONSTRAINT [PK_Auctions] PRIMARY KEY ([AuctionId]),
    CONSTRAINT [FK_Auctions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Auctions_Users_CurrentHighestBidderId] FOREIGN KEY ([CurrentHighestBidderId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [Favorites] (
    [FavoriteId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ProductId] int NULL,
    [BlogId] int NULL,
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([FavoriteId]),
    CONSTRAINT [FK_Favorites_Blogs_BlogId] FOREIGN KEY ([BlogId]) REFERENCES [Blogs] ([BlogId]),
    CONSTRAINT [FK_Favorites_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_Favorites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Bids] (
    [BidId] int NOT NULL IDENTITY,
    [BidAmount] decimal(18,2) NOT NULL,
    [BidTime] datetime2 NOT NULL,
    [AuctionId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_Bids] PRIMARY KEY ([BidId]),
    CONSTRAINT [FK_Bids_Auctions_AuctionId] FOREIGN KEY ([AuctionId]) REFERENCES [Auctions] ([AuctionId]),
    CONSTRAINT [FK_Bids_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO

CREATE TABLE [Notifications] (
    [NotificationId] int NOT NULL IDENTITY,
    [Message] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    [ProductId] int NULL,
    [AuctionId] int NULL,
    [BlogId] int NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
    CONSTRAINT [FK_Notifications_Auctions_AuctionId] FOREIGN KEY ([AuctionId]) REFERENCES [Auctions] ([AuctionId]),
    CONSTRAINT [FK_Notifications_Blogs_BlogId] FOREIGN KEY ([BlogId]) REFERENCES [Blogs] ([BlogId]),
    CONSTRAINT [FK_Notifications_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [OrderHistories] (
    [OrderHistoryId] int NOT NULL IDENTITY,
    [AuctionId] int NOT NULL,
    [UserId] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_OrderHistories] PRIMARY KEY ([OrderHistoryId]),
    CONSTRAINT [FK_OrderHistories_Auctions_AuctionId] FOREIGN KEY ([AuctionId]) REFERENCES [Auctions] ([AuctionId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrderHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Payments] (
    [PaymentId] int NOT NULL IDENTITY,
    [PaymentAmount] decimal(18,2) NOT NULL,
    [PaymentStatus] nvarchar(max) NOT NULL,
    [PaymentDate] datetime2 NOT NULL,
    [AuctionId] int NOT NULL,
    [PaymentDueDate] datetime2 NOT NULL,
    [PaymentLink] nvarchar(max) NULL,
    [IsNotificationSent] bit NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_Payments_Auctions_AuctionId] FOREIGN KEY ([AuctionId]) REFERENCES [Auctions] ([AuctionId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Payments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Auctions_CurrentHighestBidderId] ON [Auctions] ([CurrentHighestBidderId]);
GO

CREATE INDEX [IX_Auctions_ProductId] ON [Auctions] ([ProductId]);
GO

CREATE INDEX [IX_Bids_AuctionId] ON [Bids] ([AuctionId]);
GO

CREATE INDEX [IX_Bids_UserId] ON [Bids] ([UserId]);
GO

CREATE INDEX [IX_Blogs_UserId] ON [Blogs] ([UserId]);
GO

CREATE INDEX [IX_Contacts_UserId] ON [Contacts] ([UserId]);
GO

CREATE INDEX [IX_Favorites_BlogId] ON [Favorites] ([BlogId]);
GO

CREATE INDEX [IX_Favorites_ProductId] ON [Favorites] ([ProductId]);
GO

CREATE INDEX [IX_Favorites_UserId] ON [Favorites] ([UserId]);
GO

CREATE INDEX [IX_Notifications_AuctionId] ON [Notifications] ([AuctionId]);
GO

CREATE INDEX [IX_Notifications_BlogId] ON [Notifications] ([BlogId]);
GO

CREATE INDEX [IX_Notifications_ProductId] ON [Notifications] ([ProductId]);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

CREATE INDEX [IX_OrderHistories_AuctionId] ON [OrderHistories] ([AuctionId]);
GO

CREATE INDEX [IX_OrderHistories_UserId] ON [OrderHistories] ([UserId]);
GO

CREATE INDEX [IX_Payments_AuctionId] ON [Payments] ([AuctionId]);
GO

CREATE INDEX [IX_Payments_UserId] ON [Payments] ([UserId]);
GO

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
GO

CREATE INDEX [IX_Products_SellerId] ON [Products] ([SellerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241110213909_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

