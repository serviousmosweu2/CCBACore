﻿CREATE TABLE [dbo].[SYSTEM] (
    [ID]          INT           IDENTITY (1, 1) NOT NULL,
    [SYSTEM_NAME] VARCHAR (200) NULL,
    CONSTRAINT [PK_SYSTEM] PRIMARY KEY CLUSTERED ([ID] ASC)
);
