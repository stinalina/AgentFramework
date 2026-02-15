using AgentFramework;
using Azure.AI.OpenAI;
using Azure.Identity;
using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel.Primitives;
using System.Text.Json;
using System;

//Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

//var url = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");


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

// Using the Azure OpenAI SDK
// Currently, only agents that use the OpenAI Responses API support background responses: OpenAI Responses Agent and Azure OpenAI Responses Agent. GetOpenAIResponseClient
AIAgent agent = new AzureOpenAIClient(
  new Uri("https://oai-coco.openai.azure.com/"),
  new AzureCliCredential())
    .GetChatClient("TripAdvisor_Agent")
    .AsAIAgent( //new ChatClientAgentOptions()
      instructions: africaAgentInstructions,
      tools: [
         AIFunctionFactory.Create(AgentFramework.Tools.GetWeather),
         AIFunctionFactory.Create(AgentFramework.Tools.GetCountries)
      ]);



AgentSession session = await agent.CreateSessionAsync();

var chatOptions = new ChatOptions() { 
  Temperature = 0.3f, TopP = 0.8f, MaxOutputTokens = 4096,
  ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(
    schema: schema,
    schemaName: nameof(TripInfo),
    schemaDescription: "Information about a Trip including all required, well structures data."
   ),
};

AgentRunOptions options = new()
{
  AllowBackgroundResponses = true
};

var response = await agent.RunAsync("What country should I vist when I fly to Oceania?", session, options);
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
Console.WriteLine("Used Tokens: " + response.Text.Length); //TODO find out!

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
Console.WriteLine(await agent.RunAsync("What I asked you the first time?",
   session, new ChatClientAgentRunOptions(chatOptions)));
