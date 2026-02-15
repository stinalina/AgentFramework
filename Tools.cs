using System;
using System.ComponentModel;

namespace AgentFramework;

internal static class Tools
{
  [Description("Get the weather for a given location.")]
  public static string GetWeather([Description("The location to get the weather for.")] string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";

  [Description("Get popular countries for a given continent.")]
  public static string GetCountries([Description("The continent to get the countries for.")] Continents continent)
  {
    return continent switch
    {
      Continents.Africa => String.Join(',', Enum.GetValues<AfricaCountries>()),
      Continents.America => String.Join(',', Enum.GetValues<AmericaCountries>()),
      Continents.Europe => String.Join(',', Enum.GetValues<EuropeCountries>()),
      Continents.Asia => String.Join(',', Enum.GetValues<AsiaCountries>()),
      Continents.Oceania => String.Join(',', Enum.GetValues<OceaniaCountries>()),
      _ => "Unknown continent"
    };
  }
}
