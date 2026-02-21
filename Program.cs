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
using Microsoft.Extensions.Options;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");

JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(TripInfo));

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


#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//ResponsesClient agent = new AzureOpenAIClient(
//ChatClientAgent agent = new AzureOpenAIClient(
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetResponsesClient(deploymentName)
     .AsAIAgent(
        instructions: africaAgentInstructions,
        tools: [
          AIFunctionFactory.Create(AgentFramework.Tools.GetWeather),
          AIFunctionFactory.Create(AgentFramework.Tools.GetCountries),
          AIFunctionFactory.Create(AgentFramework.Tools.GetDateTime),
          .. mcpTools.Cast<AITool>(), //using third party MCP Server,
          //.. selectedWikipediaTools.Cast<AITool>(),
          ],
        //services: [
        //  new ChatHistoryProvider(chatOptions)
        // ]
        clientFactory: (client) => client.AsBuilder()
          .ConfigureOptions(options => {
            options.Temperature = chatOptions.Temperature;
            options.TopP = chatOptions.TopP;
            options.MaxOutputTokens = chatOptions.MaxOutputTokens;
            options.ResponseFormat = chatOptions.ResponseFormat;
            options.AllowMultipleToolCalls = true;
            options.ToolMode = ChatToolMode.Auto;
            options.AllowBackgroundResponses = true;
          })
          .Use( //use chatclient middlware
            getResponseFunc: CustomMiddleware.CustomChatClientMiddleware,
            getStreamingResponseFunc: null
           )
          .Build()
        );
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

var middlewareEnabledAgent = agent //use agent middleware
    .AsBuilder()
        .Use(CustomMiddleware.CustomFunctionCallingMiddleware)
        .Use(runFunc: CustomMiddleware.CustomAgentRunMiddleware, runStreamingFunc: null)
        .Use(runFunc: CustomMiddleware.GuardrailMiddleware, runStreamingFunc: null)
        .Use(runFunc: CustomMiddleware.ResultOverrideMiddleware, runStreamingFunc: null)
        .Use(runFunc: CustomMiddleware.ExceptionHandlingMiddleware, runStreamingFunc: null)
    .Build();
// Blocked request — guardrail returns early without calling agent
//Console.WriteLine(await guardedAgent.RunAsync("What is my password?"));

AgentSession session = await agent.CreateSessionAsync();

AgentRunOptions options = new() //pass run level middleware here
{
  AllowBackgroundResponses = true,
};
//var runOptions = new AgentRunOptions { RunMiddleware = DebugMiddleware };
try
{
  Console.WriteLine("What country should I vist when I fly to Oceania?");
  var response = await agent.RunAsync("What country should I vist when I fly to Oceania?", session, options);
  //var response = await agent.RunAsync("What country should I vist when I fly to Oceania? Please make a research with wikipedia.", session, options);
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
}
catch (Exception ex)
{
  Console.WriteLine("An error occurred: " + ex.Message);
}
// Persist and restore later
//Treat AgentSession as an opaque state object and restore it with the same agent/provider configuration that created it.
var serialized = await agent.SerializeSessionAsync(session); //Idea. Session are passed between the agents
  Console.WriteLine("Serialized session: " + serialized); // Serialized session: {"conversationId":"resp_04196b0246fa0d0700699951f1bc1c8194884b3a5d9f506c53"}
  AgentSession resumed = await agent.DeserializeSessionAsync(serialized);


// When in-memory chat history storage is used, it's possible to access the chat history
// that is stored in the session via the provider attached to the agent.
var provider = agent.GetService<InMemoryChatHistoryProvider>();
List<ChatMessage>? messages = provider?.ToList(); //TODO wie setze ich die session?
//List<ChatMessage>? messages = provider?.GetMessages(session);

ChatClientAgentSession typedSession = (ChatClientAgentSession)session;
Console.WriteLine(typedSession.ConversationId);

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
