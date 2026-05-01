using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace AgentFramework.Agents;

internal class LocalAgent
{
    public static async Task CreateAgent() 
    { 
        var chatClient = new OllamaApiClient(
            new Uri("http://localhost:11434"),
            defaultModel: "gemma4:e2b");

        AIAgent agent = chatClient.AsAIAgent(
            instructions: "You are a helpful assistant running locally via Ollama.");

        Console.WriteLine(await agent.RunAsync("What is the largest city in France?"));
    }
}