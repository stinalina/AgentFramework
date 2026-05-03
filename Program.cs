using AgentFramework.Agents;
using AgentFramework.Extensions;

Console.ForegroundColor = ConsoleColor.DarkMagenta;
Console.WriteLine("Lokales LLM 'gemma4:e2b' ausgeführt mit Ollama.");
Console.WriteLine("Type 'exit' or 'quit' to end the conversation.");
Console.WriteLine("--------------------------------------------------");
Console.ResetColor();

var localAgent = await LocalAgent.CreateAgent();
await localAgent.StartConversationAsync("Bis wann reicht dein Wissensstand?");