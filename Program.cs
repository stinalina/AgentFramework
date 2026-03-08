using AgentFramework;
using AgentFramework.Extensions;
using AgentFramework.Workflows;
using Azure.AI.OpenAI;
using Azure.Identity;
using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Responses;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

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

//AIAgent agent = await AgentFactory.GetContinentExpert(Continent.Europe);
//await agent.StartConversationAsync("Gebe mir Reisetipps für Kongo.");

//AIAgent travelAgencyEmployee = await TravelAgency.GetEmployeeAsync();
//await travelAgencyEmployee.StartConversationAsync();

//Workflow workflow = await MyWorkflow.GetWorkflowAsync();
//await workflow.StartWorkflowAsync();

//AIAgent workflowAgent = await WorkflowAgent.GetContinentWorkflowAgent();
//await workflowAgent.StartWorkflowAgentConversationAsync();


//Workflow workflow = await MyWorkflow.GetWorkflowAsync();
//await workflow.ExecuteWorkflowAsync();

Workflow workflow = await OrchestratorWorkflowAgent.CreateWorkflowAsync();
await workflow.RunGroupChatAsync("Erstelle mir eine Reise nach Italien");

//FIX BindExecutor ist eine extension Method, die es nicht gibt...
//Func<string, TripInfo> buildTripInfoFunc = s => JsonSerializer.Deserialize<TripInfo>(s) ?? throw new InvalidOperationException("Could not deserialize TripInfo");
//var uppercase = buildTripInfoFunc.BindExecutor<TripInfo>("TransformTripInfoExecutor");

// When in-memory chat history storage is used, it's possible to access the chat history
// that is stored in the session via the provider attached to the agent.
//var provider = agent.GetService<InMemoryChatHistoryProvider>();
//List<ChatMessage>? messages = provider?.ToList(); //TODO wie setze ich die session?
//List<ChatMessage>? messages = provider?.GetMessages(session);


//// Resume from interruption point captured by the continuation token
//options.ContinuationToken = latestReceivedUpdate?.ContinuationToken;
//await foreach (var update in agent.RunStreamingAsync(session, options))
//{
//  Console.Write(update.Text);
//}
