using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace AgentFramework.Agents;

internal class LocalAgent
{
    public static async Task<AIAgent> CreateAgent() 
    { 
        var chatClient = new OllamaApiClient(
            new Uri("http://localhost:11434"),
            defaultModel: "gemma4:e2b");

        return chatClient.AsAIAgent(
            instructions: "Du bist ein lokales LLM laufen auf Ollama.");
    }
}