CREATE TABLE [dbo].[Burndowns] (
    [BurndownId]       UNIQUEIDENTIFIER NOT NULL,
    [Title]            NVARCHAR (MAX)   NULL,
    [Definition]       NVARCHAR (MAX)   NULL,
    [OwnerUserId]      UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]      DATETIME         NOT NULL,
    [LastModifiedDate] DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.Burndowns] PRIMARY KEY CLUSTERED ([BurndownId] ASC)
);

