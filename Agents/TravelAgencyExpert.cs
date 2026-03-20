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

internal class TravelAgencyExpert
{
  private static AIAgent _employee;
  private static string _endpoint;
  private static string _deploymentName;

  public static async Task<AIAgent> GetExpertAsync(string endpoint, string deploymentName)
  {
    _endpoint = endpoint;
    _deploymentName = deploymentName;
    _employee ??= await CreateExpertAsync();
    return _employee;
  }

  private static async Task<AIAgent> CreateExpertAsync()
  {
    Console.WriteLine($"Creating Travel Agency Employee...");

    string instructions = File.ReadAllText(
      Path.Combine(AppContext.BaseDirectory, $"instructions/travel_agency_employee.instructions.txt"));

    //var persistentAgentsClient = new PersistentAgentsClient(endpoint, new DefaultAzureCredential());
    //var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    //       model: deploymentName,
    //       name: "Travel Agency Employee",
    //       instructions);

    //ArgumentNullException.ThrowIfNull(persistentAgentsClient, nameof(persistentAgentsClient));

    //return await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);


    return AzureOpenAIClientFactory.Create(_endpoint)
     .GetChatClient(_deploymentName) // ← Chat Completions API statt Responses API to avaoid 404
     .AsAIAgent(
        instructions,
        name: "Reisebüroangestellter 1",
        description: "Erster Ansprechpartner im Reisebüro",
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
     //.Use(CustomMiddleware.CustomAgentRunMiddleware, CustomMiddleware.CustomAgentRunStreamingMiddleware)
     .Build();
  }
}
