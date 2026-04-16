using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Responses;
using System.Runtime.CompilerServices;

namespace AgentFramework.Agents;

internal class BookingExpert
{
  public static async Task<AIAgent> GetAgentAsync(string endpoint, string deploymentName)
  {
    return await CreateAgentAsync(endpoint, deploymentName);
  }


  private static async Task<AIAgent> CreateAgentAsync(string endpoint, string deploymentName)
  {
    Console.WriteLine($"Creating Booking Agent...");

    string instructions = File.ReadAllText(
      Path.Combine(AppContext.BaseDirectory, $"instructions/booking_agent.instructions.txt"));

    //var persistentAgentsClient = new PersistentAgentsClient(endpoint, new DefaultAzureCredential());
    //var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    //       model: deploymentName,
    //       name: "Travel Agency Employee",
    //       instructions);

    //ArgumentNullException.ThrowIfNull(persistentAgentsClient, nameof(persistentAgentsClient));

    //return await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);


    return AzureOpenAIClientFactory.Create(endpoint)
     .GetChatClient(deploymentName) // ← Chat Completions API statt Responses API to avoid 404
     .AsAIAgent(
        instructions,
        name: "Reisebüroangestellter 2",
        description: "Finaler Ansprechpartner im Reisebüro, der die Buchug durchführt.",
        clientFactory: (client) => client.AsBuilder()
          .ConfigureOptions(options =>
          {
            options.Temperature = 0.5f;
            options.TopP = 0.8f;
            options.MaxOutputTokens = 4096;
            options.AllowMultipleToolCalls = true;
            options.ToolMode = ChatToolMode.Auto;
            options.AllowBackgroundResponses = false; // Deaktiviert wegen Continuation-Fehler
            //options.ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<TravelAgencyBookingFormat>();
          })
          .Build()
        )
     .AsBuilder()
     //.Use(CustomMiddleware.FunctionMiddleware_LogUsedTool)
     .Build();
  }
}
