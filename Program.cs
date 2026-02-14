using AgentFramework;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

AIAgent agent = new AzureOpenAIClient(
  new Uri("https://oai-coco.openai.azure.com/"),
  new AzureCliCredential())
    .GetChatClient("TripAdvisor_Agent")
    .AsAIAgent(
      instructions: "You are a trip advisor assistant.",
      tools: [
         AIFunctionFactory.Create(Tools.GetWeather),
         AIFunctionFactory.Create(Tools.GetCountries)
      ]);

Console.WriteLine(await agent.RunAsync("What country should I vist when I fly to Oceania?"));

// Stream the response
//await foreach (var update in agent.RunStreamingAsync("Tell me a one-sentence fun fact."))
//{
//  Console.Write(update);
//}


