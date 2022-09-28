using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Templates.MinimalApi.Endpoints.Internal;

public static class EndpointExtensions
{
  // Accept a marker as a type
  public static void AddEndpoints<TMarker>(this IServiceCollection services, IConfiguration configuration)
  {
    AddEndpoints(services, typeof(TMarker), configuration);
  }

  // Or pass the marker thru
  public static void AddEndpoints(this IServiceCollection services,
    Type typeMarker, IConfiguration configuration)
  {
    var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

    foreach (var endpointType in endpointTypes)
    {
      endpointType.GetMethod(nameof(IEndpoints.AddServices))!
        .Invoke(null, new object[] { services, configuration});
    }
  }

  public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
  {
    UseEndpoints(app, typeof(TMarker));
  }

  public static void UseEndpoints(this IApplicationBuilder app,
    Type typeMarker)
  {
    var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

    foreach (var endpointType in endpointTypes)
    {
      endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
        .Invoke(null, new object[] { app });
    }
  }

  private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
  {
    var endpointTypes = typeMarker.Assembly.DefinedTypes
      .Where(t => !t.IsAbstract && !t.IsInterface &&
        typeof(IEndpoints).IsAssignableFrom(t));

    return endpointTypes;
  }
}
