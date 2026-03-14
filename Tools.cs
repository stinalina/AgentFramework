using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AgentFramework;

internal static class Tools
{
  // Static MCP client instances to keep them alive during the application lifetime
  private static McpClient? _wikipediaMcpClient;
  private static IList<McpClientTool>? _wikipediaTools;
  private static readonly object _lockObject = new();

  //TODO Tool für Reisewarnungen

  [Description("Get the weather for a given location.")]
  public static string GetWeather([Description("The location to get the weather for.")] string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";

  [Description("The current datetime offset.")]
  public static string GetDateTime()
    => DateTimeOffset.Now.ToString();

  [Description("Get popular countries for a given continent.")]
  public static string GetCountries([Description("The continent to get the countries for.")] Continent continent)
  {
    return continent switch
    {
      Continent.Africa => String.Join(',', Enum.GetValues<AfricaCountries>()),
      Continent.America => String.Join(',', Enum.GetValues<AmericaCountries>()),
      Continent.Europe => String.Join(',', Enum.GetValues<EuropeCountries>()),
      Continent.Asia => String.Join(',', Enum.GetValues<AsiaCountries>()),
      Continent.Oceania => String.Join(',', Enum.GetValues<OceaniaCountries>()),
      _ => "Unknown continent"
    };
  }

  //[Description("Ordne ein Land einem Kontinent zu.")]
  //public static string GetContinent([Description("Das Land für das der zugehörige Kontinet gesucht wird.")] Continent continent)
  //{
  //  return continent switch
  //  {
  //    Continent.Africa => String.Join(',', Enum.GetValues<AfricaCountries>()),
  //    Continent.America => String.Join(',', Enum.GetValues<AmericaCountries>()),
  //    Continent.Europe => String.Join(',', Enum.GetValues<EuropeCountries>()),
  //    Continent.Asia => String.Join(',', Enum.GetValues<AsiaCountries>()),
  //    Continent.Oceania => String.Join(',', Enum.GetValues<OceaniaCountries>()),
  //    _ => "Unknown continent"
  //  };
  //}

  [Description("Formats the the tripInfo well displayed.")]
  public static string FormatStory(string title, string author, string story) =>
    $"Title: {title}\nAuthor: {author}\n\n{story}";

  [Description("Found information about continent and countries, also get the summary of a Wikipedia article.")]
  public static async Task<IList<McpClientTool>> WikipediaMCPTool()
  {
    // Use lock to ensure thread-safe initialization
    lock (_lockObject)
    {
      if (_wikipediaTools != null)
      {
        return _wikipediaTools;
      }
    }

    try
    {
      // Create the MCP client once and keep it alive
      _wikipediaMcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
      {
        Name = "Wikipedia MCP Server",
        Command = "docker",
        Arguments = [
          "run",
          "-i",
          "--rm",
          "mcp/wikipedia-mcp"
        ],
      }));

      // Retrieve the list of tools available on the Wikipedia MCP server
      var wikipediaMcpTools = await _wikipediaMcpClient.ListToolsAsync().ConfigureAwait(false);

      // Filter for the tools we want
      // available tools: https://github.com/Rudra-ravi/wikipedia-mcp?tab=readme-ov-file#available-mcp-tools
      _wikipediaTools = wikipediaMcpTools
        .Where(tool => tool.Name.Contains("search_wikipedia") || tool.Name.Contains("get_summary"))
        .ToList();

      return _wikipediaTools;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[WikipediaMCP] Error initializing Wikipedia MCP client: {ex.Message}");
      // Return empty list on error instead of throwing
      return new List<McpClientTool>();
    }
  }

  // Clean up MCP client on application shutdown
  public static async ValueTask DisposeMcpClientsAsync()
  {
    if (_wikipediaMcpClient != null)
    {
      try
      {
        await _wikipediaMcpClient.DisposeAsync();
        _wikipediaMcpClient = null;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[WikipediaMCP] Error disposing Wikipedia MCP client: {ex.Message}");
      }
    }
  }
}
