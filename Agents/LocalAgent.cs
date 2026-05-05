using Microsoft.Extensions.AI;
using OllamaSharp;

namespace AgentFramework.Agents;

internal class LocalAgent
{
    public static IChatClient Create() 
    { 
        return new OllamaApiClient(
            new Uri("http://localhost:11434"),
            defaultModel: "gemma4:e2b");
    }
}