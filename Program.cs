using AgentFramework;
using AgentFramework.Entdpoints;
using AgentFramework.Extensions;
using AgentFramework.Workflows;
using DotNetEnv;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
 ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
  ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");


#if true
#region API

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowLocalhost", builder =>
    builder.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
  options.SwaggerEndpoint("/openapi/v1.json", "Travel Agent API");
  options.RoutePrefix = string.Empty; // Set Swagger UI at app's root
});

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
  app.UseHttpsRedirection();
}

app.UseCors("AllowLocalhost");

app.MapTravelAgentEndpoints(endpoint, deploymentName);

app.Lifetime.ApplicationStopped.Register(() => {
  Tools.DisposeMcpClientsAsync().AsTask().GetAwaiter().GetResult(); //Do not use .Wait() to get the original StackTrace!
});
app.Run();

#endregion
#else

#region Console

//JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(TripInfo));
//ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(
//  schema: schema,
//  schemaName: nameof(TripInfo),
//  schemaDescription: "Information about a Trip including all required, well structures data."
// ),

Workflow workflow = await OrchestratorWorkflowAgent.CreateWorkflowAsync(endpoint, deploymentName);
await workflow.ExecuteWorkflowAsync();

await Tools.DisposeMcpClientsAsync();

// When in-memory chat history storage is used, it's possible to access the chat history
// that is stored in the session via the provider attached to the agent.
//var provider = agent.GetService<InMemoryChatHistoryProvider>();
//List<ChatMessage>? messages = provider?.ToList(); //TODO wie setze ich die session?
//List<ChatMessage>? messages = provider?.GetMessages(session);

#endregion
#endif