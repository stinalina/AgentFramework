using Microsoft.Extensions.AI;
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
  public static IList<AIFunction> WikipediaAIFunctions()
  {
    return [
      AIFunctionFactory.Create(SearchWikipedia),
      AIFunctionFactory.Create(GetWikipediaSummary)
    ];
  }

  [Description("Suche nach Wikipedia-Artikeln zu einem Reiseziel, Land oder Sehenswürdigkeit.")]
  private static async Task<string> SearchWikipedia(
    [Description("Suchbegriff, z.B. Ländername, Stadtname oder Reiseziel")] string query,
    CancellationToken cancellationToken = default)
  {
    return await InvokeMcpToolFreshAsync("search_wikipedia",
      new Dictionary<string, object?> { ["query"] = query },
      cancellationToken);
  }

  [Description("Rufe die Zusammenfassung eines Wikipedia-Artikels anhand seines Titels ab.")]
  private static async Task<string> GetWikipediaSummary(
    [Description("Titel des Wikipedia-Artikels")] string title,
    CancellationToken cancellationToken = default)
  {
    return await InvokeMcpToolFreshAsync("get_summary",
      new Dictionary<string, object?> { ["title"] = title },
      cancellationToken);
  }

  private static async Task<string> InvokeMcpToolFreshAsync(
    string toolName,
    Dictionary<string, object?> arguments,
    CancellationToken cancellationToken)
  {
    try
    {
      await using var client = await McpClient.CreateAsync(new StdioClientTransport(new()
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

      //var options = new HttpClientTransportOptions { Endpoint = new Uri(_mcpServerUrl) };
      //await using var client = await McpClient.CreateAsync(new HttpClientTransport(options));

      var tools = await client.ListToolsAsync(cancellationToken: cancellationToken);
      var tool = tools.FirstOrDefault(t => t.Name == toolName);
      if (tool is null)
      {
        Console.WriteLine($"[WikipediaMCP] Tool '{toolName}' not found on server.");
        return string.Empty;
      }

      Console.WriteLine($"[WikipediaMCP] Invoking '{toolName}' with fresh session.");
      var result = await tool.InvokeAsync(
        new AIFunctionArguments(arguments),
        cancellationToken);
        
      return result?.ToString() ?? string.Empty;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[WikipediaMCP] Error invoking '{toolName}': {ex.Message}");
      return string.Empty;
    }
  }
}
