using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;

AIAgent agent = new AzureOpenAIClient(
  new Uri("https://oai-coco.openai.azure.com/"),
  new AzureCliCredential())
    .GetChatClient("TripAdvisor_Agent")
    .AsAIAgent(instructions: "You are good at telling jokes.");

Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate."));

// Stream the response
//await foreach (var update in agent.RunStreamingAsync("Tell me a one-sentence fun fact."))
//{
//  Console.Write(update);
//}