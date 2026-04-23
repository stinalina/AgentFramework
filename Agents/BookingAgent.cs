using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Responses;

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

    return AzureOpenAIClientFactory.Create(endpoint) //// Step 4
     .GetChatClient(deploymentName) // ← Chat Completions API statt Responses API to avoid 404
     .AsAIAgent(
        instructions,
        name: "Reisebüroangestellter für Buchungen",
        description: "Finaler Ansprechpartner im Reisebüro, der die Buchug durchführt.",
        clientFactory: (client) => client.AsBuilder() // Step 6
          .ConfigureOptions(options =>
          {
            options.Temperature = 0.5f;
            options.TopP = 0.8f;
            options.MaxOutputTokens = 4096;
            options.AllowMultipleToolCalls = true; // nicht nötig, da hier keine Tools verwendet werden...
            options.ToolMode = ChatToolMode.Auto; 
            options.AllowBackgroundResponses = false;
            // options.ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<TravelAgencyBookingFormat>();
          })
          .Build()
        )
     .AsBuilder()
     .Build();
  }
}
