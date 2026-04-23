using AgentFramework;
using AgentFramework.Entdpoints;
using AgentFramework.Extensions;
using AgentFramework.Workflows;
using DotNetEnv;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var envFile = Path.Combine(AppContext.BaseDirectory, ".env");
if (File.Exists(envFile))
{
  Env.Load(envFile);
}

string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
 ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
  ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");


#if false
#region API

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAnyOrigin", builder =>
    builder.AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
  options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
  options.KnownIPNetworks.Clear(); 
  options.KnownProxies.Clear(); 
});

var app = builder.Build();

app.UseForwardedHeaders();


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

app.UseCors("AllowAnyOrigin");

app.MapTravelAgentEndpoints(endpoint, deploymentName); // Step 13

app.Lifetime.ApplicationStopped.Register(() => { });
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

Workflow workflow = await OrchestratorWorkflowAgent.CreateWorkflowAsync(endpoint, deploymentName); // Step 1
await workflow.ExecuteWorkflowAsync();

// When in-memory chat history storage is used, it's possible to access the chat history
// that is stored in the session via the provider attached to the agent.
//var provider = agent.GetService<InMemoryChatHistoryProvider>();
//List<ChatMessage>? messages = provider?.ToList(); //TODO wie setze ich die session?
//List<ChatMessage>? messages = provider?.GetMessages(session);

#endregion
#endif