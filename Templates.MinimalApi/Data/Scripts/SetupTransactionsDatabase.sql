USE Transactions;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Transactions](
  [Id] [BIGINT] IDENTITY(1,1) NOT NULL,
  [Amount] [DECIMAL](19,4) NOT NULL,
  [Date] [DATETIME] NOT NULL DEFAULT GETDATE(),
  [Description] [NVARCHAR](255) NULL,
  [Type] [NVARCHAR](50) NOT NULL,
  [IpAddressV4] [NCHAR](15) NULL,
  [IpAddressV6] [NCHAR](39) NULL
) ON [PRIMARY]
GO