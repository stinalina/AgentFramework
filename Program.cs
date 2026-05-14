using AgentFramework.Extensions;
using AgentFramework.Workflows;
using System.Text.Json;

Console.ForegroundColor = ConsoleColor.DarkMagenta;
Console.WriteLine("Lokales LLM 'gemma4:e2b' ausgeführt mit Ollama.");
Console.WriteLine("Type 'exit' or 'quit' to end the conversation.");
Console.WriteLine("--------------------------------------------------");
Console.ResetColor();

var workflowAgent = HandoffWorkflowAgent.GetContinentWorkflowAgentAsync();
var conversation = await workflowAgent.StartWorkflowAgentConversationAsync();

Console.WriteLine(JsonSerializer.Serialize(conversation));
