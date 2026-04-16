using ModelContextProtocol.Client;
using System.ComponentModel;

namespace AgentFramework;

internal static class Tools
{
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
    try
    {
      // Always create a fresh McpClient per call.
      // MCP Streamable HTTP sessions are closed by the server after a request cycle;
      // reusing the old client/session causes silent failures on subsequent tool invocations.
      var options = new HttpClientTransportOptions()
      {
        Endpoint = new Uri(_mcpServerUrl)
      };
      var client = await McpClient.CreateAsync(new HttpClientTransport(options));
      var allTools = await client.ListToolsAsync().ConfigureAwait(false);

      // available tools: https://github.com/Rudra-ravi/wikipedia-mcp?tab=readme-ov-file#available-mcp-tools
      return [..allTools.Where(tool => tool.Name.Contains("search_wikipedia") || tool.Name.Contains("get_summary"))];
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[WikipediaMCP] Error initializing Wikipedia MCP client: {ex.Message}");
      return [];
    }
  }
}
