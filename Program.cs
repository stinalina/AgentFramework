using DotNetEnv;

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using ModelContextProtocol.Client;
using OpenAI.Chat;

using OpenAI.Assistants;
using System.Text.Json;
using AgentFramework;

Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");

JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(TripInfo));
var chatOptions_responseFormat = new ChatOptions()
{
  ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(
    schema: schema,
    schemaName: nameof(TripInfo),
    schemaDescription: "Information about a Trip including all required, well structures data."
   ),
};
// Theh deserialize the final response
//var personInfo = response.Deserialize<PersonInfo>(JsonSerializerOptions.Web);
//Console.WriteLine($"Name: {personInfo.Name}, Age: {personInfo.Age}, Occupation: {personInfo.Occupation}");

var africaAgentInstructions = File.ReadAllText(
  Path.Combine(AppContext.BaseDirectory, "instructions/africa_agent.instructions.txt")); //TODO check, if this is found when deployed!

await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
  Name = "MCPServer",
  Command = "npx",
  Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-github"], //https://github.com/modelcontextprotocol/csharp-sdk
}));

await using var wikipediaMcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
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
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
var wikipediaMcpTools = await wikipediaMcpClient.ListToolsAsync().ConfigureAwait(false);

// available tools: https://github.com/Rudra-ravi/wikipedia-mcp?tab=readme-ov-file#available-mcp-tools
var selectedWikipediaTools = wikipediaMcpTools 
  .Where(tool => tool.Name.Contains("search_wikipedia") || tool.Name.Contains("get_summary"))
  .ToList();

// Using the Azure OpenAI SDK
// Currently, only agents that use the OpenAI Responses API support background responses: OpenAI Responses Agent and Azure OpenAI Responses Agent. GetOpenAIResponseClient
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetResponsesClient(deploymentName) //GetResponseClient
     .AsAIAgent(
        instructions: africaAgentInstructions, 
        tools: [
          AIFunctionFactory.Create(AgentFramework.Tools.GetWeather),
          AIFunctionFactory.Create(AgentFramework.Tools.GetCountries),
          .. mcpTools.Cast<AITool>(), //using third party MCP Server,
          .. selectedWikipediaTools.Cast<AITool>(), //using third party MCP Server,
          ]);
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
// Create a specialized editor agent
//Use sth like this if you need DefaultAzureCredential
//var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
//{
//  ExcludeEnvironmentCredential = false,
//  ExcludeInteractiveBrowserCredential = true,
//  ExcludeManagedIdentityCredential = true,
//  ExcludeSharedTokenCacheCredential = true,
//  ExcludeVisualStudioCredential = true,
//  ExcludeVisualStudioCodeCredential = true,
//  ExcludeAzurePowerShellCredential = true,
//  ExcludeAzureCliCredential = false,
//});


AgentSession session = await agent.CreateSessionAsync();

var chatOptions = new ChatOptions()
{
  Temperature = 0.3f,
  TopP = 0.8f,
  MaxOutputTokens = 4096,
  ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(
    schema: schema,
    schemaName: nameof(TripInfo),
    schemaDescription: "Information about a Trip including all required, well structures data."
   ),
  AllowMultipleToolCalls = true,
  ToolMode = ChatToolMode.Auto,
  AllowBackgroundResponses = true,
};

AgentRunOptions options = new()
{
  AllowBackgroundResponses = true,
};

try
{
  //Console.WriteLine("What country should I vist when I fly to Oceania?");
  //var response = await agent.RunAsync("What country should I vist when I fly to Oceania?", session, options);
  var response = await agent.RunAsync("What country should I vist when I fly to Oceania? Please make a research with wikipedia.", session, options);
  // Continue to poll until the final response is received
  // The initial call may complete immediately (no continuation token) or start a background operation (with continuation token)
  while (response.ContinuationToken is not null)
  {
    // Wait before polling again.
    await Task.Delay(TimeSpan.FromSeconds(2));

    options.ContinuationToken = response.ContinuationToken; //store continuation tokens persistently for operations that may span user sessions
    response = await agent.RunAsync(session, options);
  }
  Console.WriteLine(response.Text);
  Console.WriteLine("Usage Details: " + JsonSerializer.Serialize(response.Usage));
  Console.WriteLine(JsonSerializer.Serialize(response.Messages));
} catch (Exception ex)
{
  Console.WriteLine("An error occurred: " + ex.Message);
}

//Stream the response
//AgentResponseUpdate? latestReceivedUpdate = null;

//await foreach (var update in agent.RunStreamingAsync("Write a very long novel about otters in space.", session, options))
//{
//  Console.Write(update.Text);

//  latestReceivedUpdate = update;

//  // Simulate an interruption
//  break;
//}

//// Resume from interruption point captured by the continuation token
//options.ContinuationToken = latestReceivedUpdate?.ContinuationToken;
//await foreach (var update in agent.RunStreamingAsync(session, options))
//{
//  Console.Write(update.Text);
//}

return;
//Console.WriteLine(await agent.RunAsync("What I asked you the first time?",
//   session, new ChatClientAgentRunOptions(chatOptions)));
