using Dapper;

namespace Templates.MinimalApi.Data;

public class DatabaseInitializer
{
  private readonly IDbConnectionFactory _connectionFactory;
  public DatabaseInitializer(IDbConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task InitializeAsync()
  {
    using var connection = await _connectionFactory.CreateConnectionAsync();
    await connection.ExecuteAsync(
      @"USE Transactions;
        GO
        SET ANSI_NULLS ON
        GO
        SET QUOTED_IDENTIFIER ON
        GO

        CREATE TABLE [dbo].[Transactions](
          [Id] [BIGINT] IDENTITY(1,1) NOT NULL,
          [Amount] [DECIMAL](19,4) NOT NULL,
          [Description] [NVARCHAR](255) NULL,
          [Type] [NVARCHAR](50) NOT NULL,
          [IpAddressV4] [NCHAR](15) NULL,
          [IpAddressV6] [NCHAR](39) NULL
        ) ON [PRIMARY]
        GO"
    );
  }
}
