using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AgentFramework;

internal static class Tools
{
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

  [Description("Formats the the tripInfo well displayed.")]
  public static string FormatStory(string title, string author, string story) =>
    $"Title: {title}\nAuthor: {author}\n\n{story}";

  [Description("Found information about continent and countries, also get the summary of a Wikipedia article.")]
  public static async Task<IList<McpClientTool>> WikipediaMCPTool()
  {
    await using var wikipediaMcpClient = await McpClient.CreateAsync(new StdioClientTransport(new ()
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


    // found available servers here: https://github.com/modelcontextprotocol/servers
    // Retrieve the list of tools available on the GitHub server
    //var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
    var wikipediaMcpTools = await wikipediaMcpClient.ListToolsAsync().ConfigureAwait(false);

    // available tools: https://github.com/Rudra-ravi/wikipedia-mcp?tab=readme-ov-file#available-mcp-tools
    return wikipediaMcpTools
      .Where(tool => tool.Name.Contains("search_wikipedia") || tool.Name.Contains("get_summary"))
      .ToList();
    }
}
