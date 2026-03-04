using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using OpenAI.Chat;
using OpenAI.Assistants;

namespace AgentFramework;

internal static class AgentFactory
{
  private static readonly string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
     ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

  private static readonly string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");

  private static readonly Dictionary<string, AIAgent> ContinentExperts = [];

  public static async Task<AIAgent> GetContinentExpert(Continent continent)
  {
    if (ContinentExperts.Values.Count == 0)
    {
      await CreateAgentExpertsAsync();
    }
    return ContinentExperts[continent.ToString()];
  }

  private static async Task CreateAgentExpertsAsync()
  {
    foreach (var continent in Enum.GetValues<Continent>())
    {
      AIAgent agent = await CreateContinentExpertAsync(continent);
      ContinentExperts.Add(continent.ToString(), agent);
    }
  }
  
  private static async Task<AIAgent> CreateContinentExpertAsync(Continent continent)
  {
    Console.WriteLine($"Creating {continent} Expert...");

    string instructions = File
      .ReadAllText(Path.Combine(AppContext.BaseDirectory, $"instructions/continent_agent.instructions.txt"))
      .Replace("<continent>", continent.ToString());

    var wikipediaTools = await Tools.WikipediaMCPTool();

    return new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
     .GetChatClient(deploymentName)
     .AsAIAgent(
        instructions,
        name: $"{continent} Expert",
        description: $"An expert in all questions related to {continent}.",
        tools: [
          AIFunctionFactory.Create(Tools.GetCountries),
          ..wikipediaTools.Cast<AITool>(),
          ]
          //new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions{
          //  ChatReducer = new MessageCountingChatReducer(10)
          //})
        ,
        clientFactory: (client) => client.AsBuilder()
          .ConfigureOptions(options =>
          {
            options.Temperature = 0.3f;
            options.TopP = 0.8f;
            options.MaxOutputTokens = 4096;
            options.AllowMultipleToolCalls = true;
            options.ToolMode = ChatToolMode.Auto;
            options.AllowBackgroundResponses = false; // Deaktiviert wegen Continuation-Fehler
          })
          .Build()
        )
       .AsBuilder()
       //.Use(runFunc: CustomMiddleware.DebugMessagesMiddleware, runStreamingFunc: null)
       .Use(CustomMiddleware.FunctionMiddleware_LogUsedTool)
       .Build();
  }
}
