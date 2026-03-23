using ModelContextProtocol.Client;
using System.ComponentModel;

namespace AgentFramework;

internal static class Tools
{
  // Static MCP client instances to keep them alive during the application lifetime
  private static McpClient? _wikipediaMcpClient;
  private static IList<McpClientTool>? _wikipediaTools;
  private static readonly object _lockObject = new();

  private static string _mcpServerUrl = Environment.GetEnvironmentVariable("AZURE_HOSTED_MCP_SERVER")
    ?? throw new InvalidOperationException("AZURE_HOSTED_MCP_SERVER is not set.");

  [Description("Erhalte eine Liste von Ländern, welche in dem angefragtem Kontinet enthalten sind.")]
  public static string GetCountries([Description("Kontinet für den die Länder ermittelt werden sollen.")] Continent continent)
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

  [Description("Formats the the tripInfo well displayed.")]
  public static string FormatStory(string title, string author, string story) =>
    $"Title: {title}\nAuthor: {author}\n\n{story}";


  [Description("Finde detaillierte Informationen über ein Land. Hilfreich beim Erstellen von Reisen. Erhalte ebenfalls einen Wikipedia Artikel.")]
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
      var options = new HttpClientTransportOptions()
      {
        Endpoint = new Uri(_mcpServerUrl)
      };
      _wikipediaMcpClient = await McpClient.CreateAsync(new HttpClientTransport(options));

      var wikipediaMcpTools = await _wikipediaMcpClient.ListToolsAsync().ConfigureAwait(false);

      // available tools: https://github.com/Rudra-ravi/wikipedia-mcp?tab=readme-ov-file#available-mcp-tools
      _wikipediaTools = [..wikipediaMcpTools
        .Where(tool => tool.Name.Contains("search_wikipedia") || tool.Name.Contains("get_summary"))];

      return _wikipediaTools;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[WikipediaMCP] Error initializing Wikipedia MCP client: {ex.Message}");
      return [];
    }
  }

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
