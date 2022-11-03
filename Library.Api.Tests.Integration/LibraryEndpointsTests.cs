using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests
  : IClassFixture<WebApplicationFactory<IApiMarker>>
{
  // Just talking thru what Microsoft.AspNetCore.Mvc.Testing has
  /*private readonly WebApplicationFactory<IApiMarker> _factory;

  public LibraryEndpointsTests()
  {
    // This is actually the httpClient to call your api.
    // All calls expecting integration tests should pass thru here
    _factory = new WebApplicationFactory<IApiMarker>();

    // in-memory deferred host of the api
    var httpClient = _factory.CreateClient();
  }*/

  private readonly WebApplicationFactory<IApiMarker> _factory;

  public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
  {
    _factory = factory;
  }
}
