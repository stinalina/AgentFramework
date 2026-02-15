using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using DotNetEnv;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

//Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

//var url = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

AIAgent agent = new AzureOpenAIClient(
  new Uri("https://oai-coco.openai.azure.com/"),
  new AzureCliCredential())
    .GetChatClient("TripAdvisor_Agent")
    .AsAIAgent(
      instructions: "You are a trip advisor assistant. Don't talk to long, each Token is money and I don't have that much.",
      tools: [
         AIFunctionFactory.Create(AgentFramework.Tools.GetWeather),
         AIFunctionFactory.Create(AgentFramework.Tools.GetCountries)
      ]);

AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine(await agent.RunAsync("What country should I vist when I fly to Oceania?", session));
Console.WriteLine(await agent.RunAsync("What I asked you the first time?", session));

// Stream the response
//await foreach (var update in agent.RunStreamingAsync("Tell me a one-sentence fun fact."))
//{
//  Console.Write(update);
//}