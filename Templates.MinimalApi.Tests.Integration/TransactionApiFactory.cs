﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Templates.MinimalApi;
using Templates.MinimalApi.Data;

namespace Library.Api.Tests.Integration;

public class TransactionApiFactory : WebApplicationFactory<IApiMarker>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureServices(collection =>
    {
      collection.RemoveAll(typeof(IDbConnectionFactory));
      collection.AddSingleton<IDbConnectionFactory>(_ =>
        new SQLServerConnectionFactory("DataSource=file:inmem?mode=memory&cache=shared")
      );
    });
  }
}
