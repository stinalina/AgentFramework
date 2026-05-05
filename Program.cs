using AgentFramework.Agents;
using AgentFramework.Extensions;

Console.ForegroundColor = ConsoleColor.DarkMagenta;
Console.WriteLine("Lokales LLM 'gemma4:e2b' ausgeführt mit Ollama.");
Console.WriteLine("Type 'exit' or 'quit' to end the conversation.");
Console.WriteLine("--------------------------------------------------");
Console.ResetColor();

var continentAgents = await AgentFactory.CreateAgentExpertsAsync();
await continentAgents["Europe"].StartConversationAsync("Nenne mir den meist besuchten Ort in Europa 2024.");