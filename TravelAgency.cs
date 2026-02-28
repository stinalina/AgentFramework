using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using OpenAI.Chat;
using OpenAI.Assistants;

namespace AgentFramework;

// Signleton. ReiseBüro
internal class TravelAgency
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

    return new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
     .GetResponsesClient(deploymentName)
     .AsAIAgent(
        instructions,
                tools: [
          AIFunctionFactory.Create(Tools.GetCountries),
          AIFunctionFactory.Create(Tools.GetDateTime),
          ],
        //services: [
        //  new ChatHistoryProvider(chatOptions)
        // ]
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
     .Build();
  }
}
