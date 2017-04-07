CREATE TABLE [dbo].[HistoryLines] (
    [HistoryLineId] UNIQUEIDENTIFIER NOT NULL,
    [DateTime]      DATETIME         NOT NULL,
    [TaskLine]      NVARCHAR (MAX)   NULL,
    [BurndownId]    UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_dbo.HistoryLines] PRIMARY KEY CLUSTERED ([HistoryLineId] ASC),
    CONSTRAINT [FK_dbo.HistoryLines_dbo.Burndowns_BurndownId] FOREIGN KEY ([BurndownId]) REFERENCES [dbo].[Burndowns] ([BurndownId]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_BurndownId]
    ON [dbo].[HistoryLines]([BurndownId] ASC);

