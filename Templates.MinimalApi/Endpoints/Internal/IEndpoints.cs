namespace Templates.MinimalApi.Endpoints.Internal;

public interface IEndpoints
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2252:This API requires opting into preview features", Justification = "EnablePreviewFeaturesBroken")]
  public static abstract void DefineEndpoints(IEndpointRouteBuilder app);
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2252:This API requires opting into preview features", Justification = "EnablePreviewFeaturesBroken")]
  public static abstract void AddServices(IServiceCollection services,
    IConfiguration configuration);
}
