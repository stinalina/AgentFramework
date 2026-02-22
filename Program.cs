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
using AgentFramework.Workflows;

Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

//JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(TripInfo));
//ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(
//  schema: schema,
//  schemaName: nameof(TripInfo),
//  schemaDescription: "Information about a Trip including all required, well structures data."
// ),

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

//var middlewareEnabledAgent = agent //use agent middleware
//    .AsBuilder()
//        .Use(CustomMiddleware.CustomFunctionCallingMiddleware)
//        .Use(runFunc: CustomMiddleware.CustomAgentRunMiddleware, runStreamingFunc: null)
//        .Use(runFunc: CustomMiddleware.GuardrailMiddleware, runStreamingFunc: null)
//        .Use(runFunc: CustomMiddleware.ResultOverrideMiddleware, runStreamingFunc: null)
//        .Use(runFunc: CustomMiddleware.ExceptionHandlingMiddleware, runStreamingFunc: null)
//    .Build();
// Blocked request — guardrail returns early without calling agent
//Console.WriteLine(await guardedAgent.RunAsync("What is my password?"));

AIAgent agent = await AgentFactory.GetAgent(Continent.Europe);
await agent.StartConversationAsync("Gebe mir Reisetipps für Kongo.");

// When in-memory chat history storage is used, it's possible to access the chat history
// that is stored in the session via the provider attached to the agent.
var provider = agent.GetService<InMemoryChatHistoryProvider>();
List<ChatMessage>? messages = provider?.ToList(); //TODO wie setze ich die session?
//List<ChatMessage>? messages = provider?.GetMessages(session);


//// Resume from interruption point captured by the continuation token
//options.ContinuationToken = latestReceivedUpdate?.ContinuationToken;
//await foreach (var update in agent.RunStreamingAsync(session, options))
//{
//  Console.Write(update.Text);
//}
