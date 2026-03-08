using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Responses;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AgentFramework.Agents;

internal class TravelAgencyExpert
{
  //TODO das global auslagern, damit es nicht in jedem Agenten neu erstellt wird
  private static readonly string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
   ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

  private static readonly string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");


  private static AIAgent _employee;

  public static async Task<AIAgent> GetEmployeeAsync()
  {
    if (_employee is AIAgent agent)
    {
      return agent;
    }

    _employee = await CreateEmployeeAsync();
    return _employee;
  }

  private static async Task<AIAgent> CreateEmployeeAsync()
  {
    Console.WriteLine($"Creating Travel Agency Employee...");

    string instructions = File.ReadAllText(
      Path.Combine(AppContext.BaseDirectory, $"instructions/travel_agency_employee.instructions.txt"));

    //var persistentAgentsClient = new PersistentAgentsClient(endpoint, new AzureCliCredential());
    //var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    //       model: deploymentName,
    //       name: "Travel Agency Employee",
    //       instructions);

    //ArgumentNullException.ThrowIfNull(persistentAgentsClient, nameof(persistentAgentsClient));

    //return await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);


    return new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
     .GetResponsesClient(deploymentName)
     .AsAIAgent(
        instructions,
        tools: [ AIFunctionFactory.Create(Tools.GetCountries) ],
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
     .Use(CustomMiddleware.FunctionMiddleware_LogUsedTool)
     .Build();
  }
}
