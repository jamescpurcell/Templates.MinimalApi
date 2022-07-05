using System.Reflection;

namespace Templates.MinimalApi;

public class MapPoint
{
  public double Latitude { get; set; }
  public double Longitude { get; set; }

  public static async ValueTask<MapPoint?> BindAsync(
    HttpContext context, ParameterInfo parameterInfo)
  {
    var rawRequestBody = await new StreamReader(context.Request.Body)
      .ReadToEndAsync();

    try
    {
      var splitValue = rawRequestBody?.Split(',').Select(double.Parse).ToArray();
      return new MapPoint
      {
        Latitude = splitValue![0],
        Longitude = splitValue[1]
      };

    }
    catch (Exception)
    {
      return null;
    }
  }
}
