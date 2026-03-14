using AgentFramework.Extensions;
using AgentFramework.Workflows;
using DotNetEnv;
using Microsoft.Agents.AI.Workflows;

Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
 ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
  ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");

//JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(TripInfo));
//ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(
//  schema: schema,
//  schemaName: nameof(TripInfo),
//  schemaDescription: "Information about a Trip including all required, well structures data."
// ),

Workflow workflow = await OrchestratorWorkflowAgent.CreateWorkflowAsync(endpoint, deploymentName);
await workflow.ExecuteWorkflowAsync();

// When in-memory chat history storage is used, it's possible to access the chat history
// that is stored in the session via the provider attached to the agent.
//var provider = agent.GetService<InMemoryChatHistoryProvider>();
//List<ChatMessage>? messages = provider?.ToList(); //TODO wie setze ich die session?
//List<ChatMessage>? messages = provider?.GetMessages(session);

