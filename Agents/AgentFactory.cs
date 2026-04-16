using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using OpenAI.Chat;
using OpenAI.Assistants;

namespace AgentFramework.Agents;

internal static class AgentFactory
{
  public static async Task<Dictionary<string, AIAgent>> CreateAgentExpertsAsync(string endpoint, string deploymentName)
  {
    var continentExperts = new Dictionary<string, AIAgent>();

    foreach (var continent in Enum.GetValues<Continent>())
    {
      AIAgent agent = await CreateContinentExpertAsync(continent, endpoint, deploymentName);
      continentExperts.Add(continent.ToString(), agent);
    }

    return continentExperts;
  }
  
  private static async Task<AIAgent> CreateContinentExpertAsync(Continent continent, string endpoint, string deploymentName)
  {
    Console.WriteLine($"Creating {continent} Expert...");

    string instructions = File
      .ReadAllText(Path.Combine(AppContext.BaseDirectory, $"instructions/continent_agent.instructions.txt"))
      .Replace("<continent>", continent.ToString());

    var wikipediaTools = await Tools.WikipediaMCPTool();

    return AzureOpenAIClientFactory.Create(endpoint)
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
          .UseFunctionInvocation()
          .Build()
        )
      .AsBuilder()
      //.Use(runFunc: CustomMiddleware.DebugMessagesMiddleware, runStreamingFunc: null)
      //.Use(runFunc: CustomMiddleware.CustomAgentRunMiddleware, runStreamingFunc: CustomMiddleware.CustomAgentRunStreamingMiddleware)
      .Use(CustomMiddleware.FunctionMiddleware_LogUsedTool) //Die .AsBuilder().Use(FunctionMiddleware_LogUsedTool).Build() fügt die Middleware dagegen auf der Agent-Pipeline-Ebene hinzu – diese wird von der internen Chat-Pipeline nicht durchlaufen.
      .Build();
  }
}
