using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.ComponentModel;

namespace AgentFramework;

internal static class Tools
{
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
            "mcp/wikipedia-mcp",
            "--language",
            "de"
        ],
      }));

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
