using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Responses;

namespace AgentFramework.Agents;

internal class BookingExpert
{
  //TODO das global auslagern, damit es nicht in jedem Agenten neu erstellt wird
  private static readonly string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
   ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

  private static readonly string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");

  private static AIAgent _agent;

  public static async Task<AIAgent> GetAgentAsync()
  {
    if (_agent is AIAgent agent)
    {
      return agent;
    }

    _agent = await CreateAgentAsync();
    return _agent;
  }

  private static async Task<AIAgent> CreateAgentAsync()
  {
    Console.WriteLine($"Creating Booking Agent...");

    string instructions = File.ReadAllText(
      Path.Combine(AppContext.BaseDirectory, $"instructions/booking_agent.instructions.txt"));

    //var persistentAgentsClient = new PersistentAgentsClient(endpoint, new AzureCliCredential());
    //var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    //       model: deploymentName,
    //       name: "Travel Agency Employee",
    //       instructions);

    //ArgumentNullException.ThrowIfNull(persistentAgentsClient, nameof(persistentAgentsClient));

    //return await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);


    return new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
     .GetChatClient(deploymentName) // ← Chat Completions API statt Responses API to avaoid 404
     .AsAIAgent(
        instructions,
        name: "Reisebüroangestellter 2",
        description: "Finaler Ansprechpartner im Reisebüro, der die Buchug durchführt.",
        //tools: [ AIFunctionFactory.Create(Tools.GetCountries) ],
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
     //.Use(CustomMiddleware.FunctionMiddleware_LogUsedTool)
     .Build();
  }
}
